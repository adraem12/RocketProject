using System;
using System.Collections.Generic;
using UnityEngine;

namespace AerodynamicObjects
{
    [System.Serializable]
    public class AeroObject : MonoBehaviour, IFluidInteractive
    {
        //public event Action<AeroObject> UpdateFluidEvent;

        // This doesn't really need to be public!
        public AerodynamicObject ao = new AerodynamicObject();

        public Rigidbody rb;

        public Vector3 ObjectDimensions
        {
            get { return Vector3.Scale(transform.lossyScale, localDimensions); }
        }

        public List<FluidZone> FluidZones { get { return fluidZones; } set { fluidZones = value; } }
        private List<FluidZone> fluidZones = new List<FluidZone>();

        public Vector3 localDimensions = Vector3.one;

        public bool hasDrag;
        public bool hasLift;
        public bool hasRotationalDamping;
        public bool hasRotationalLift;
        public bool hasBuoyancy;

        public bool isKinematic;

        // Velocity of the object in the global (world) reference frame
        Vector3 velocity;

        // Velocity of the object in the object's reference frame
        // This is the velocity used by the aerodynamic objects library
        Vector3 objectVelocity;


        // Angular velocity of the object in the global (world) reference frame
        Vector3 angularVelocity;

        // Angular velocity of the object in the object's reference frame
        // This is the angular velocity used by the aerodynamic objects library
        Vector3 objectAngularVelocity;

        // The position of the object in the global reference frame at the previous time step
        Vector3 previousPosition;

        // The rotation of the object in the global reference frame at the previous time step
        Quaternion currentRotation;
        Quaternion previousRotation;
        Vector3 orientationAxis;
        float rotationAngle = 0.0f;

        private void Awake()
        {
            Initialise();
        }

        private void OnValidate()
        {
            UpdateDimensions();
        }

        private void Reset()
        {
            ao = new AerodynamicObject();
            AddCheckedModels();
        }

        public void UpdateDimensions()
        {
            Vector3 dimensions = ObjectDimensions;
            ao.SetDimensions(dimensions.x, dimensions.y, dimensions.z);
        }

        public void SetVelocity(Vector3 globalVelocity)
        {
            ao.SetVelocity(globalVelocity.ToNumerics(), Numerics.Vector3.Zero);
        }

        public void SetVelocity(Vector3 globalVelocity, Vector3 globalAngularVelocity)
        {
            ao.SetVelocity(globalVelocity.ToNumerics(), globalAngularVelocity.ToNumerics());
        }

        public void GetVelocity()
        {
            if (isKinematic)
            {
                // Translational velocity
                velocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
                //objectVelocity = transform.InverseTransformDirection(velocity);

                // Rotational velocity
                currentRotation = transform.rotation;
                (currentRotation * Quaternion.Inverse(previousRotation)).ToAngleAxis(out rotationAngle, out orientationAxis);
                angularVelocity = Mathf.Deg2Rad * rotationAngle * orientationAxis / Time.deltaTime;
                //objectAngularVelocity = transform.InverseTransformDirection(angularVelocity);

                previousPosition = transform.position;
                previousRotation = currentRotation;
            }
            else
            {
                // Translational velocity from rigidbody
                velocity = rb.GetPointVelocity(transform.position);
                //objectVelocity = transform.InverseTransformDirection(velocity);

                // Angular velocity from rigidbody
                angularVelocity = rb.angularVelocity;
                //objectAngularVelocity = transform.InverseTransformDirection(angularVelocity);

                // Store previous position and rotation so we can smoothly switch to kinematic
                previousPosition = transform.position;
                previousRotation = transform.rotation;
            }
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            previousPosition = position;
            previousRotation = rotation;
        }

        /// <summary>
        /// The net aerodynamic force acting on this object in the global frame of reference.
        /// </summary>
        public Vector3 GlobalNetForce()
        {
            return transform.TransformDirection(ao.NetAerodynamicLoad.force.ToUnity());
        }

        /// <summary>
        /// The net aerodynamic force acting on this object in the local frame of reference.
        /// </summary>
        public Vector3 LocalNetForce()
        {
            return ao.NetAerodynamicLoad.force.ToUnity();
        }

        /// <summary>
        /// The net aerodynamic torque acting on this object in the global frame of reference.
        /// </summary>
        public Vector3 GlobalNetTorque()
        {
            return transform.TransformDirection(ao.NetAerodynamicLoad.moment.ToUnity());
        }

        /// <summary>
        /// The net aerodynamic torque acting on this object in the local frame of reference.
        /// </summary>
        public Vector3 LocalNetTorque()
        {
            return ao.NetAerodynamicLoad.moment.ToUnity();
        }

        void FixedUpdate()
        {
            // Update the dimensions of the object. If the dimensions won't change then this can be called
            // once in Start or OnEnable
            UpdateDimensions();

            // This doesn't need to be done every time step but it accounts for users changing the direction of gravity
            // Might be better to take this out and just let people know that it's possible to do.
            ao.Gravity = Physics.gravity.ToNumerics();

            // Set the rotation of the local frame of reference relative to the global frame
            ao.SetLocalRotation(transform.rotation.ToNumerics());

            // Get and set the velocity of the aerodynamic object
            GetVelocity();
            ao.SetVelocity(velocity.ToNumerics(), angularVelocity.ToNumerics());

            // First, set the state and properties of the fluid to the global fluid state and properties
            GetGlobalFluid();

            // Then call the update fluid event to have any fluid zones and effectors apply their effects to this object's fluid
            //UpdateFluidEvent?.Invoke(this);
            GetFluidZoneEffects();

            // Run all the aerodynamic calculations
            ao.RunAerodynamics();

            if (rb)
            {
                // Apply the resulting aerodynamic force to the rigid body - note the aero object transform might not align
                // with the rigid body transform!
                rb.AddForceAtPosition(GlobalNetForce(), transform.position);

                // Apply the resulting aerodynamic moment to the rigid body - we can't use relative torque here
                // because the aero object transform could be at a different orientation to the transform of the rigid body
                // i.e. they could be on separate game objects
                rb.AddTorque(GlobalNetTorque());
            }
        }

        private void GetFluidZoneEffects()
        {
            // Iterate over all the fluid zones which should be affecting this object
            for (int i = 0; i < fluidZones.Count; i++)
            {
                // Update the fluid properties - note this will usually mean that the most recent
                // fluid zone to be added will control the fluid properties as it will be last in the list
                // and fluid properties are not cumulative.
                fluidZones[i].UpdateFluidProperties(this);

                // Add the velocity produced by the fluid zone to the object's fluid velocity.
                ao.AddToFluidVelocity(fluidZones[i].VelocityFunction(transform.position).ToNumerics());
            }
        }

        private void GetGlobalFluid()
        {
            // This feels clunky
            ao.SetFluidVelocity(GlobalFluid.FluidVelocity.ToNumerics());
            ao.FluidPressure = GlobalFluid.FluidProperties.pressure;
            ao.FluidDynamicViscosity = GlobalFluid.FluidProperties.dynamicViscosity;
            ao.FluidDensity = GlobalFluid.FluidProperties.density;
        }

        /// <summary>
        /// Adds the given aerodynamic model to the aerodynamic object.
        /// If the object already has a model of the same type then an additional model will not be added.
        /// </summary>
        public void AddMonoBehaviourModel<T>() where T : MonoBehaviour, IAerodynamicModel, new()
        {

            if (gameObject.TryGetComponent<T>(out T modelComponent))
            {
                // The gameobject already has the model attached
                // Now we just need to make sure the AO has the model too

                if (ao == null)
                {
                    ao = new AerodynamicObject();
                    ao.AerodynamicModels = new IAerodynamicModel[] { modelComponent };
                    return;
                }

                if (ao.AerodynamicModels.Length == 0)
                {
                    ao.AerodynamicModels = new IAerodynamicModel[] { modelComponent };
                    return;
                }

                for (int i = 0; i < ao.AerodynamicModels.Length; i++)
                {
                    // Need to make sure we can cast before checking it's the correct model.
                    // This doesn't make sense to do both checks when we're removing just based on type and not instance but hey
                    if (ao.AerodynamicModels[i].GetType() == typeof(T))
                    {
                        if (((T)ao.AerodynamicModels[i]).Equals(modelComponent))
                        {
                            // The aerodynamic object already has this model
                            return;
                        }
                    }
                }

                // Increase the model array size
                Array.Resize(ref ao.AerodynamicModels, ao.AerodynamicModels.Length + 1);
                // Add the new model
                ao.AerodynamicModels[ao.AerodynamicModels.Length - 1] = modelComponent;
            }
            else
            {
                // We need to add the model component first and then check
                modelComponent = (T)gameObject.AddComponent<T>();

                if (ao == null)
                {
                    ao = new AerodynamicObject();
                    ao.AerodynamicModels = new IAerodynamicModel[] { modelComponent };
                    return;
                }

                if (ao.AerodynamicModels.Length == 0)
                {
                    ao.AerodynamicModels = new IAerodynamicModel[] { modelComponent };
                    return;
                }

                // Just to be safe, we'll remove any existing monobehaviour models on the object
                //RemoveMonoBehaviourModel<T>();
                // This could cause problems if users want multiple instances of the same model on
                // one aero object. Though I can't really think of a case where this would make sense

                // Increase the model array size
                Array.Resize(ref ao.AerodynamicModels, ao.AerodynamicModels.Length + 1);
                // Add the new model
                ao.AerodynamicModels[ao.AerodynamicModels.Length - 1] = modelComponent;
            }

        }



        /// <summary>
        /// Adds the given aerodynamic model to the aerodynamic object.
        /// If the object already has a model of the same type then an additional model will not be added.
        /// </summary>
        public void AddModel<T>() where T : IAerodynamicModel, new()
        {

            if (ao == null)
            {
                ao = new AerodynamicObject();
                ao.AerodynamicModels = new IAerodynamicModel[] { new T() };
                return;
            }

            if (ao.AerodynamicModels.Length == 0)
            {
                ao.AerodynamicModels = new IAerodynamicModel[] { new T() };
                return;
            }

            for (int i = 0; i < ao.AerodynamicModels.Length; i++)
            {
                if (ao.AerodynamicModels[i].GetType() == typeof(T))
                {
                    // The aerodynamic object already has this type of model
                    return;
                }
            }

            // Increase the model array size
            Array.Resize(ref ao.AerodynamicModels, ao.AerodynamicModels.Length + 1);
            // Add the new model
            ao.AerodynamicModels[ao.AerodynamicModels.Length - 1] = new T();
        }

        public void RemoveMonoBehaviourModel<T>() where T : MonoBehaviour, IAerodynamicModel, new()
        {

            if (gameObject.TryGetComponent(out T component))
            {
                DestroyImmediate(component);
            }


            if (ao == null)
            {
                return;
            }

            if (ao.AerodynamicModels.Length == 0)
            {
                return;
            }

            int removeID = -1;

            // Find the index of the model we want to remove
            for (int i = 0; i < ao.AerodynamicModels.Length; i++)
            {
                if (ao.AerodynamicModels[i].GetType() == typeof(T))
                {
                    removeID = i;
                    break;
                }
            }

            // Check if we found the model or not
            if (removeID < 0)
                return;


            // Remove the model from the array

            // Shift all the elements
            for (int i = removeID; i < ao.AerodynamicModels.Length - 1; i++)
            {
                ao.AerodynamicModels[i] = ao.AerodynamicModels[i + 1];
            }

            // Decrease the model array size
            Array.Resize(ref ao.AerodynamicModels, ao.AerodynamicModels.Length - 1);
        }

        public void RemoveModel<T>() where T : IAerodynamicModel, new()
        {
            if (ao == null)
            {
                return;
            }

            if (ao.AerodynamicModels.Length == 0)
            {
                return;
            }

            int removeID = -1;

            // Find the index of the model we want to remove
            for (int i = 0; i < ao.AerodynamicModels.Length; i++)
            {
                if (ao.AerodynamicModels[i].GetType() == typeof(T))
                {
                    removeID = i;
                    break;
                }
            }

            // Check if we found the model or not
            if (removeID < 0)
                return;


            // Remove the model from the array

            // Shift all the elements
            for (int i = removeID; i < ao.AerodynamicModels.Length - 1; i++)
            {
                ao.AerodynamicModels[i] = ao.AerodynamicModels[i + 1];
            }

            // Decrease the model array size
            Array.Resize(ref ao.AerodynamicModels, ao.AerodynamicModels.Length - 1);
        }

        public void ClearModels()
        {
            ao.AerodynamicModels = new IAerodynamicModel[0];
        }

        public T GetModel<T>() where T : IAerodynamicModel
        {
            for (int i = 0; i < ao.AerodynamicModels.Length; i++)
            {
                if (ao.AerodynamicModels[i].GetType() == typeof(T))
                {
                    return (T)ao.AerodynamicModels[i];
                }
            }
            return default;
        }

        public int GetModelIndex<T>() where T : IAerodynamicModel
        {
            for (int i = 0; i < ao.AerodynamicModels.Length; i++)
            {
                if (ao.AerodynamicModels[i].GetType() == typeof(T))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Initialise()
        {
            if (ao == null)
            {
                ao = new AerodynamicObject();
            }

            AddCheckedModels();

            UpdateDimensions();

            // This doesn't need to be done every time step but it accounts for users changing the direction of gravity
            ao.Gravity = Physics.gravity.ToNumerics();

            // Make sure to set the previous rotation and position so that they aren't zero
            // which would lead to huge velocities in the first time step!
            previousPosition = transform.position;
            previousRotation = transform.rotation;
        }

        private void AddCheckedModels()
        {
            // There MUST be a better way to do this...
            if (hasDrag)
            {
                AddModel<DragModel>();
            }

            if (hasLift)
            {
                AddModel<LiftModel>();
            }

            if (hasRotationalDamping)
            {
                AddModel<RotationalDampingModel>();
            }

            if (hasRotationalLift)
            {
                AddModel<RotationalLiftModel>();
            }

            if (hasBuoyancy)
            {
                AddModel<BuoyancyModel>();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;

            if (ao.hasGroup)
            {
                Vector3 transformScale = transform.lossyScale;
                float scaling = (Mathf.PI / 4f) * (float)ao.MyGroup.objectAreaScale;
                Vector3 scaledDimensions = new Vector3((float)ao.Span * scaling, (float)ao.Thickness, (float)ao.Chord * scaling);
                scaledDimensions = ao.TransformBodyToLocal(scaledDimensions.ToNumerics()).ToUnity();
                scaledDimensions.x /= transformScale.x;
                scaledDimensions.y /= transformScale.y;
                scaledDimensions.z /= transformScale.z;


                Gizmos.DrawWireCube(Vector3.zero, scaledDimensions);
            }
            else
            {
                Gizmos.DrawWireMesh(GizmoMeshLoader.GizmoMesh, Vector3.zero, Quaternion.identity, localDimensions);
            }
        }
#endif
    }
}