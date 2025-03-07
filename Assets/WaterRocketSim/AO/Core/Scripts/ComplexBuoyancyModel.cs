using UnityEngine;

namespace AerodynamicObjects
{
    /// <summary>
    /// Uses the point of intersection between the fluid and this game object's collider - provided by Unity's physics.
    /// The point of intersection is used to split the ellipsoid body into two sections when the object is partially
    /// submerged in a fluid zone, e.g. when floating on water.
    /// If the object is completely submerged then the volume of the ellipsoid is used to determine the buoyant force acting on the object.
    /// </summary>
    public class ComplexBuoyancyModel : MonoBehaviour, IAerodynamicModel
    {
        public Rigidbody rb;

        // Checks for how submerged the object is in a fluid zone   
        bool inFluid = false;

        // This is set to false whenever we're not in a fluid zone
        // It's necessary to make sure the previous values for volumes don't jump when transitioning into a new fluid zone
        bool initialised = false;

        // If we're inside the bounds, we might not be inside the ellipsoid!
        bool inBounds;
        float ellipsoidCheck;
        bool inEllipsoid;

        Vector3 scaleToSphere, scaleFromSphere;
        float sphereRadius;
        float capHeight;

        // Rotations to and from the body frame of reference
        Quaternion rotationToBodyFrame, rotationFromBodyFrame;

        // Point of intersection between the object and the fluid zone
        Vector3 worldPointOfIntersection, objectPointOfIntersection, bodyPointOfIntersection, spherePointOfIntersection;

        // These details are reluctantly given by the physics engine
        float collisionPenetration;
        Vector3 worldCollisionNormal, objectCollisionNormal, bodyCollisionNormal;

        // We need to store our own collider so we can get the penetration between it and the fluid zone we're in contact with
        Collider thisCollider;

        // Density of the fluid zone
        float fluidZoneDensity;

        // Keeping track of the current volumes and their positions
        // As well as the previous values.
        // This allows us to apply drag due to added mass as the object displaces the fluid
        float capVolume, previousCapVolume;
        float remainderVolume, previousRemainderVolume;
        Vector3 capCentreOfVolume, previousCapCentreOfVolume;
        Vector3 remainderCentreOfVolume, previousRemainderCentreOfVolume;
        Vector3 capVolumeVelocityDirection;
        Vector3 remainderVolumeVelocityDirection;

        // Does the spherical cap represent the fluid zone?
        bool capIsColliderFluid;
        // Need to keep track of if the cap used to represent the fluid zone
        // If we transition from it being so to not or vice versa, then we need
        // to take care that the previous positions of the cap and remainder volumes
        // are swapped when the transition occurs
        bool capWasColliderFluid;

        // Use this to determine if the cap is in the fluid zone or not
        Vector3 fluidCentre;

        // Forces and moments and such
        Vector3 objectGravity;
        Vector3 capForce, capBuoyantForce, capDragForce, capMoment;
        Vector3 remainderForce, remainderBuoyantForce, remainderDragForce, remainderMoment;


        Vector3 bodyPointOnEllipsoid1, bodyPointOnEllipsoid2;
        Vector3 bodyPenetrationCentre;
        Vector3 bodyCollisionOrthogonal;
        Vector3 bodyAxisAlignedCollisionNormal;

        // Point of intersection
        float Ix;
        float Iy;
        float Iz;

        // The normal of the plane
        float Nx;
        float Ny;
        float Nz;

        // The vector direction we want to intersect with the plane - note we're assuming the vector crosses the origin
        float Vx;
        float Vy;
        float Vz;

        // The length of the vector which defines the point of intersection between V and the plane defined by point I and direction N
        float vectorMagnitude;

        // Major, mid and minor axes of the ellipsoid body
        float a;
        float b;
        float c;

        // Vector which is orthogonal to the collision normal
        float Ox;
        float Oy;
        float Oz;

        // Distance from the point of intersection between the ellipsoid and the fluid to the ellipsoid surface
        float distanceToEllipsoidSurface1;
        float distanceToEllipsoidSurface2;


        private void Reset()
        {
            AeroObject aeroObject = GetComponent<AeroObject>();
            rb = aeroObject.rb;
        }


        private void Awake()
        {
            thisCollider = GetComponent<Collider>();
        }

        void GetFluidInfo(Collider other)
        {
            if (other.gameObject.TryGetComponent(out FluidZone fluidZone))
            {
                fluidZoneDensity = fluidZone.fluidProperties.density;
                inFluid = Physics.ComputePenetration(other, other.transform.position, other.transform.rotation, thisCollider, transform.position, transform.rotation, out worldCollisionNormal, out collisionPenetration);
                fluidCentre = other.transform.position;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            GetFluidInfo(other);
        }

        private void OnTriggerStay(Collider other)
        {
            GetFluidInfo(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out FluidZone _))
            {
                inFluid = false;
                capIsColliderFluid = false;
                initialised = false;
            }
        }

        void GetRegularForce(AerodynamicObject ao, float density)
        {
            // We aren't intersecting a fluid boundary so we only need the volume of the entire ellipsoid
            capVolume = (4f / 3f) * Mathf.PI * (float)(ao.MajorAxis * ao.MidAxis * ao.MinorAxis);
            remainderVolume = 0;

            capForce = -density * capVolume * objectGravity;
            remainderForce = Vector3.zero;

            capMoment = Vector3.zero;
            remainderMoment = Vector3.zero;

            // Make sure the visuals can be drawn
            capHeight = 0;
            capCentreOfVolume = Vector3.zero;
            remainderCentreOfVolume = Vector3.zero;
        }

        public AerodynamicLoad GetAerodynamicLoad(AerodynamicObject ao)
        {
            // Get gravity in object frame
            objectGravity = transform.InverseTransformDirection(Physics.gravity);

            // If we're not intersecting a fluid zone
            if (!inFluid)
            {
                initialised = false;
                GetRegularForce(ao, GlobalFluid.FluidProperties.density);
                return new AerodynamicLoad()
                {
                    force = (capForce).ToNumerics(),
                    moment = (capMoment).ToNumerics()
                };
            }


            // Get the point of intersection based on the collider's bounds.
            // This isn't a foolproof method for every collider but it kind of works...
            // It's perfect when using a box collider!
            worldPointOfIntersection = transform.position + Vector3.Scale(worldCollisionNormal.normalized, thisCollider.bounds.extents) - collisionPenetration * worldCollisionNormal;

            // If the intersection point is not inside the bounds, then we are fully submerged in the fluid
            inBounds = thisCollider.bounds.Contains(worldPointOfIntersection);
            if (!inBounds)
            {
                initialised = false;
                GetRegularForce(ao, fluidZoneDensity);
                return new AerodynamicLoad()
                {
                    force = (capForce).ToNumerics(),
                    moment = (capMoment).ToNumerics()
                };
            }

            // Rotations to and from the (span, thickness, chord) body frame of reference
            rotationToBodyFrame = ao.BodyInverseRotation.ToUnity();
            rotationFromBodyFrame = ao.BodyRotation.ToUnity();

            // Get the point of intersection in object and body frames of reference
            objectPointOfIntersection = transform.InverseTransformDirection(worldPointOfIntersection - transform.position);
            bodyPointOfIntersection = rotationToBodyFrame * objectPointOfIntersection;


            // The geometry is simplified by scaling down to a sphere with radius equal to the minor axis of the ellipsoid
            sphereRadius = (float)ao.MinorAxis;
            scaleToSphere = new Vector3((float)ao.MinorAxis / (float)ao.MajorAxis, 1, (float)ao.MinorAxis / (float)ao.MidAxis);
            scaleFromSphere = new Vector3((float)ao.MajorAxis / (float)ao.MinorAxis, 1, (float)ao.MidAxis / (float)ao.MinorAxis);

            // Get the collision normal in object and body frames - though we only need it in body frame
            objectCollisionNormal = transform.InverseTransformDirection(worldCollisionNormal);
            bodyCollisionNormal = rotationToBodyFrame * objectCollisionNormal;

            // Checking if the cap of the ellipsoid is in the fluid
            capIsColliderFluid = Vector3.Distance(fluidCentre, worldPointOfIntersection) < Vector3.Distance(fluidCentre, transform.position);

            // Check that we're inside the ellipsoid - otherwise we'll need to use an alternative method.
            // For now we're just ignoring the parts of the collider that don't represent the ellipsoid body
            ellipsoidCheck = Mathf.Pow(bodyPointOfIntersection.x / (float)ao.MajorAxis, 2) + Mathf.Pow(bodyPointOfIntersection.y / (float)ao.MinorAxis, 2) + Mathf.Pow(bodyPointOfIntersection.z / (float)ao.MidAxis, 2);
            inEllipsoid = ellipsoidCheck < 1;
            if (!inEllipsoid)
            {
                initialised = false;
                // If the cap is the fluid zone then when we aren't inside the ellipsoid we can say we're above the fluid zone
                if (capIsColliderFluid)
                {
                    GetRegularForce(ao, GlobalFluid.FluidProperties.density);
                }
                else
                {
                    // Otherwise, the point is outside of the ellipsoid, but the ellipsoid itself is submerged!
                    GetRegularForce(ao, fluidZoneDensity);
                }
                return new AerodynamicLoad()
                {
                    force = capForce.ToNumerics(),
                    moment = capMoment.ToNumerics()
                };
            }


            // XOR - checking that the two conditions are not the same
            // I.e. we have transitioned from/to the cap being in the fluid
            if (capIsColliderFluid ^ capWasColliderFluid)
            {
                // Swap the position values - this prevents a huge spike in the transition
                (previousRemainderCentreOfVolume, previousCapCentreOfVolume) = (previousCapCentreOfVolume, previousRemainderCentreOfVolume);
            }

            capWasColliderFluid = capIsColliderFluid;

            // Reaching here means that we've got far enough inside the bounds of the collider to also be inside of the ellipsoid body


            // ======================== Getting edges either side of collision centre ========================

            // This makes sure that the collision normal points along the ellipsoid body's axes rather than just
            // directly towards the fluid collider.
            bodyAxisAlignedCollisionNormal = Vector3.Normalize(Vector3.Scale(bodyCollisionNormal, new Vector3((float)-ao.Span, (float)-ao.Thickness, (float)-ao.Chord)));


            // We then project the aligned vector onto the plane defined by the collision normal and the point of intersection
            // to find the point in the middle of the ellipsoid made by the intersection

            // The intersection point, used to define the plane
            Ix = bodyPointOfIntersection.x;
            Iy = bodyPointOfIntersection.y;
            Iz = bodyPointOfIntersection.z;

            // The normal of the plane
            Nx = bodyCollisionNormal.x;
            Ny = bodyCollisionNormal.y;
            Nz = bodyCollisionNormal.z;

            // The vector direction we want to intersect with the plane - note we're assuming the vector crosses the origin
            Vx = bodyAxisAlignedCollisionNormal.x;
            Vy = bodyAxisAlignedCollisionNormal.y;
            Vz = bodyAxisAlignedCollisionNormal.z;

            vectorMagnitude = (Ix * Nx + Iy * Ny + Iz * Nz) / (Nx * Vx + Ny * Vy + Nz * Vz);

            // All of this just to get the direction we want to look for the edges of the ellipsoid body along!
            bodyCollisionOrthogonal = Vector3.Normalize(new Vector3(vectorMagnitude * Vx, vectorMagnitude * Vy, vectorMagnitude * Vz) - bodyPointOfIntersection);

            // If the collision orthogonal vector is zero then our body point of intersection is in the correct place already!
            if (bodyCollisionOrthogonal == Vector3.zero)
            {
                bodyPenetrationCentre = bodyPointOfIntersection;
            }
            else
            {
                // Getting the points on the ellipsoid body surface based on the intersection point
                // and the vector which is orthogonal to the collision normal. This is essentially
                // finding the edges of the ellipsoid in the collision plane. From those points we can find the centre
                // and that's how we can find the centre of volume of the submerged part of the body

                // I cheated and used matlab symbolics to get the solution for this
                // it could be made much more efficient by computing things separately but for now it works
                a = (float)ao.MajorAxis;
                b = (float)ao.MidAxis;
                c = (float)ao.MinorAxis;

                Ox = bodyCollisionOrthogonal.x;
                Oy = bodyCollisionOrthogonal.y;
                Oz = bodyCollisionOrthogonal.z;

                // This is pre-optimisation, keeping it incase things go wrong
                ////                                                                                                         > < it's this +- which is different, so we could calculated either side of this only once
                //distanceToEllipsoidSurface1 = -(Iz * Oz * a * a * b * b + Iy * Oy * a * a * c * c + Ix * Ox * b * b * c * c + a * b * c * Mathf.Sqrt(-Ix * Ix * Oy * Oy * c * c - Ix * Ix * Oz * Oz * b * b + 2 * Ix * Iy * Ox * Oy * c * c + 2 * Ix * Iz * Ox * Oz * b * b - Iy * Iy * Ox * Ox * c * c - Iy * Iy * Oz * Oz * a * a + 2 * Iy * Iz * Oy * Oz * a * a - Iz * Iz * Ox * Ox * b * b - Iz * Iz * Oy * Oy * a * a + Ox * Ox * b * b * c * c + Oy * Oy * a * a * c * c + Oz * Oz * a * a * b * b)) / (Ox * Ox * b * b * c * c + Oy * Oy * a * a * c * c + Oz * Oz * a * a * b * b);
                //distanceToEllipsoidSurface2 = -(Iz * Oz * a * a * b * b + Iy * Oy * a * a * c * c + Ix * Ox * b * b * c * c - a * b * c * Mathf.Sqrt(-Ix * Ix * Oy * Oy * c * c - Ix * Ix * Oz * Oz * b * b + 2 * Ix * Iy * Ox * Oy * c * c + 2 * Ix * Iz * Ox * Oz * b * b - Iy * Iy * Ox * Ox * c * c - Iy * Iy * Oz * Oz * a * a + 2 * Iy * Iz * Oy * Oz * a * a - Iz * Iz * Ox * Ox * b * b - Iz * Iz * Oy * Oy * a * a + Ox * Ox * b * b * c * c + Oy * Oy * a * a * c * c + Oz * Oz * a * a * b * b)) / (Ox * Ox * b * b * c * c + Oy * Oy * a * a * c * c + Oz * Oz * a * a * b * b);



                // This is just madness at this point bloody hell.
                // Absolutely MatLab'd the hell out of this
                float aa = a * a;
                float bb = b * b;
                float cc = c * c;
                float aabb = aa * bb;
                float aacc = aa * cc;
                float bbcc = bb * cc;
                float IxIx = Ix * Ix;
                float IyIy = Iy * Iy;
                float IzIz = Iz * Iz;
                float OxOx = Ox * Ox;
                float OyOy = Oy * Oy;
                float OzOz = Oz * Oz;
                float IxOx = Ix * Ox;
                float IyOy = Iy * Oy;
                float IzOz = Iz * Oz;

                // Don't ask for the explanation to this equation. I threw the equation for an ellipsoid and the equation for a vector
                // into matlab's symbolic toolbox and solved for the intersection points
                // Maybe it would be easier to solve directly for the point x y z values but I went for the t in "x = t*Vx" form of the line equation
                float lhs = IzOz * aabb + IyOy * aacc + IxOx * bbcc;
                float rhs = a * b * c * Mathf.Sqrt(-IxIx * OyOy * cc - IxIx * OzOz * bb + 2 * IyOy * IxOx * cc + 2 * IxOx * IzOz * bb - IyIy * OxOx * cc - IyIy * OzOz * aa
                                             + 2 * IyOy * IzOz * aa - IzIz * OxOx * bb - IzIz * OyOy * aa + OxOx * bbcc + OyOy * aacc + OzOz * aabb);
                float denominator = OxOx * bbcc + OyOy * aacc + OzOz * aabb;
                distanceToEllipsoidSurface1 = -(lhs + rhs) / denominator;
                distanceToEllipsoidSurface2 = (rhs - lhs) / denominator;

                // Get the two points at the edges of the penetration
                bodyPointOnEllipsoid1 = bodyPointOfIntersection + distanceToEllipsoidSurface1 * bodyCollisionOrthogonal;
                bodyPointOnEllipsoid2 = bodyPointOfIntersection + distanceToEllipsoidSurface2 * bodyCollisionOrthogonal;

                // Get the centre of the penetration
                bodyPenetrationCentre = 0.5f * (bodyPointOnEllipsoid1 + bodyPointOnEllipsoid2);
            }

            // Now that we know where the centre of the penetration occurs, we can get on with transforming into spherical coordinates
            // Thinking about this... could we just transform into the sphere first and make the above an easier calculation? I swear...
            spherePointOfIntersection = Vector3.Scale(bodyPenetrationCentre, scaleToSphere);

            // Distance between the sphere surface and the point of intersection forms the height of a spherical cap
            capHeight = sphereRadius - spherePointOfIntersection.magnitude;

            // Make sure we're moving in the correct direction
            if (capIsColliderFluid)
            {
                capCentreOfVolume = spherePointOfIntersection - (3f / 8f) * capHeight * bodyAxisAlignedCollisionNormal;
            }
            else
            {
                capCentreOfVolume = spherePointOfIntersection + (3f / 8f) * capHeight * bodyAxisAlignedCollisionNormal;
            }

            // This is a real fudge factor - I feel like it puts the remainder volume too far back
            // but we need it to move such that it's at the same point as the cap centre of volume when the
            // cap height is equal to the sphere radius (in that case the cap and remainder are both halves of the sphere)
            remainderCentreOfVolume = -(capHeight / sphereRadius) * capCentreOfVolume;

            // At this point I've stopped using separate variables for each frame of reference...
            // Going from sphere to body frame here
            capCentreOfVolume = Vector3.Scale(capCentreOfVolume, scaleFromSphere);
            remainderCentreOfVolume = Vector3.Scale(remainderCentreOfVolume, scaleFromSphere);

            // Going from body frame to object frame here
            capCentreOfVolume = rotationFromBodyFrame * capCentreOfVolume;
            remainderCentreOfVolume = rotationFromBodyFrame * remainderCentreOfVolume;

            // Calculate the volume of the spherical cap
            capVolume = (1f / 3f) * Mathf.PI * capHeight * capHeight * (3f * sphereRadius - capHeight);

            // Scale the volume up for the ellipsoid
            capVolume *= (float)ao.MidAxis * (float)ao.MajorAxis / (sphereRadius * sphereRadius);

            // Get the remaining volume for the other part of the ellipsoid
            remainderVolume = ((4f / 3f) * Mathf.PI * (float)(ao.MajorAxis * ao.MidAxis * ao.MinorAxis)) - capVolume;

            // This stops the very first step from being a big jump
            // If we haven't initialised then the following values will all be zero or whatever they
            // were last time we were in a fluid zone. Make sure they don't jump when we enter a new fluid zone
            if (!initialised)
            {
                previousCapCentreOfVolume = capCentreOfVolume;
                previousCapVolume = capVolume;
                previousRemainderCentreOfVolume = remainderCentreOfVolume;
                previousRemainderVolume = remainderVolume;

                initialised = true;
            }


            // Force due to buoyancy (not including density here as it depends on the cap being in the fluid or not
            capBuoyantForce = -capVolume * objectGravity;
            remainderBuoyantForce = -remainderVolume * objectGravity;

            // Get the drag due to the volume of fluid that is being displaced by the object's motion
            // Again, we're not including the fluid density just yet
            capVolumeVelocityDirection = Vector3.Normalize(transform.InverseTransformDirection(rb.GetRelativePointVelocity(capCentreOfVolume))); // Vector3.Normalize(previousCapCentreOfVolume - capCentreOfVolume);
            float capVolumeRate = (capVolume - previousCapVolume) / Time.fixedDeltaTime;

            // This should be 0.5x the values but I'm trying to include more drag for a better look
            capDragForce = -CD * capVolumeRate * capVolumeRate * capVolumeVelocityDirection;

            remainderVolumeVelocityDirection = Vector3.Normalize(transform.InverseTransformDirection(rb.GetRelativePointVelocity(remainderCentreOfVolume))); // Vector3.Normalize(previousRemainderCentreOfVolume - remainderCentreOfVolume);
            float remainderVolumeRate = (remainderVolume - previousRemainderVolume) / Time.fixedDeltaTime;

            // This should be 0.5x the values but I'm trying to include more drag for a better look
            remainderDragForce = -CD * remainderVolumeRate * remainderVolumeRate * remainderVolumeVelocityDirection;

            // Add the drag to the buoyancy force
            capForce = capBuoyantForce + capDragForce;
            remainderForce = remainderBuoyantForce + remainderDragForce;

            if (capIsColliderFluid)
            {
                capForce *= fluidZoneDensity;
                remainderForce *= GlobalFluid.FluidProperties.density;
            }
            else
            {
                capForce *= GlobalFluid.FluidProperties.density;
                remainderForce *= fluidZoneDensity;
            }

            // Record the previous volumes and positions of the volumes for the drag calculations
            previousCapCentreOfVolume = capCentreOfVolume;
            previousCapVolume = capVolume;
            previousRemainderCentreOfVolume = remainderCentreOfVolume;
            previousRemainderVolume = remainderVolume;

            // Calculate the moments due to the centre of volumes not being at the centre of mass
            capMoment = Vector3.Cross(capCentreOfVolume, capForce);
            remainderMoment = Vector3.Cross(remainderCentreOfVolume, remainderForce);

            // REMEMBER FORCES ARE IN THE OBJECT FRAME (i.e. Unity's object frame)
            return new AerodynamicLoad()
            {
                force = (capForce + remainderForce).ToNumerics(),
                moment = (capMoment + remainderMoment).ToNumerics()
            };
        }

        private float CD = 0.5f;

        private void OnDrawGizmos()
        {
            //Vector3 worldGeoCentre1 = transform.position + transform.TransformDirection(capCentreOfVolume);
            //Vector3 worldGeoCentre2 = transform.position + transform.TransformDirection(remainderCentreOfVolume);

            //Vector3 worldEllispoid1 = transform.position + transform.TransformDirection(rotationFromBodyFrame * bodyPointOnEllipsoid1);
            //Vector3 worldEllispoid2 = transform.position + transform.TransformDirection(rotationFromBodyFrame * bodyPointOnEllipsoid2);
            //Vector3 penetrationPoint = transform.position + transform.TransformDirection(rotationFromBodyFrame * bodyPointOfIntersection);

            //Gizmos.DrawSphere(worldEllispoid1, 0.1f);
            //Gizmos.DrawSphere(worldEllispoid2, 0.1f);
            //Gizmos.DrawSphere(worldGeoCentre1, 0.1f);
            //Gizmos.DrawSphere(worldGeoCentre2, 0.1f);
            //Gizmos.DrawSphere(penetrationPoint, 0.1f);

            //Gizmos.color = Color.gray;
            //Gizmos.DrawLine(worldGeoCentre1, worldGeoCentre1 + 0.02f * transform.TransformDirection(capBuoyantForce));
            //Gizmos.DrawLine(worldGeoCentre2, worldGeoCentre2 + 0.02f * transform.TransformDirection(remainderBuoyantForce));

            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(worldGeoCentre1, worldGeoCentre1 + 0.02f * transform.TransformDirection(capDragForce));
            //Gizmos.DrawLine(worldGeoCentre2, worldGeoCentre2 + 0.02f * transform.TransformDirection(remainderDragForce));

            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(worldGeoCentre1, worldGeoCentre1 + transform.TransformDirection(capVolumeVelocityDirection));
            //Gizmos.DrawLine(worldGeoCentre2, worldGeoCentre2 + transform.TransformDirection(remainderVolumeVelocityDirection));
        }
    }
}