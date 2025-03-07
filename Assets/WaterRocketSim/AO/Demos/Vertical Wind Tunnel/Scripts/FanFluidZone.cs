using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AerodynamicObjects.Demos
{
    public class FanFluidZone : FluidZone
    {
        // Note that in this demo the fan is only emperically coupled with the FluidZone in this demo (fan goes faster = more wind)
        
        // Storing variables outside of the VelocityFunction like this will save on Garbage Collection calls
        // However, it will not be thread safe. If you are experiencing strange behaviours it may be worth
        // declaring your varibles inside the function so that values are carried between calls.
        // This hasn't been an issue during development, it is only something anticipated if users are implementing their own threads.
        
        public float windSpeedAtFanFace, speedFallOffDistance;
        CapsuleCollider capsuleCollider;
        public float fanRotationSpeedToWindSpeed;
        public Transform fanHub;
        public float entrainmentMagnitude, entrainmentRadius, fanRadius;
        Vector3 xzPosition, entrainmentDirection, fanVelocity;
        float radius, entrainmentSpeed, radiusFromCentre;
        public Vector3 _fluidVelocity;

        // This method sets the fluid zone wind speed as a function of position
        public override Vector3 VelocityFunction(Vector3 position)
        {
            xzPosition = new Vector3(position.x, 0, position.z);
            radiusFromCentre = xzPosition.magnitude;

            // Fan effect
            fanVelocity = new Vector3(0, windSpeedAtFanFace * (1 - position.y / speedFallOffDistance), 0);

            if (radiusFromCentre < fanRadius)
            {
                _fluidVelocity = fanVelocity;
            }
            else
            {
                _fluidVelocity = Vector3.zero;
            }

            // Entrainment effect
            radius = radiusFromCentre - fanRadius;
            if (radius > 0 & radius < entrainmentRadius & position.y > .5f)
            {
                entrainmentSpeed = entrainmentMagnitude * fanVelocity.y * (1 - radius / entrainmentRadius);
                entrainmentDirection = -xzPosition.normalized;
                _fluidVelocity += entrainmentSpeed * entrainmentDirection;
            }
            return _fluidVelocity;
        }


        private void Start()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        void FixedUpdate()
        {
            capsuleCollider.height = 2f * speedFallOffDistance;
            fanHub.Rotate(0, 0, windSpeedAtFanFace * fanRotationSpeedToWindSpeed * Time.deltaTime);
        }
    }
}