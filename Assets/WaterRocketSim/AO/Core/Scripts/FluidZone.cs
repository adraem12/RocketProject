using UnityEngine;

namespace AerodynamicObjects
{
    /// <summary>
    /// Fluid Zones define the properties of a fluid for a given volume in space.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public abstract class FluidZone : MonoBehaviour
    {
        public FluidProperties fluidProperties;
        private new Collider collider;

        public virtual void Awake()
        {
            collider = GetComponent<Collider>();
        }

        public bool IsPositionInsideZone(Vector3 position)
        {
            // This sucks but I can't find a better way to cheaply do this kind of detection
            return collider.bounds.Contains(position);
        }

        /// <summary>
        /// Override this function to change the way the velocity of the fluid is calculated.
        /// </summary>
        /// <param name="position">Position of the object in the global (earth) frame of reference.</param>
        /// <returns>The velocity of the fluid at the given position.</returns>
        public virtual Vector3 VelocityFunction(Vector3 position)
        {
            // MAKE SURE TO USE aeroObject.SetFluidVelocity or aeroObject.AddToFluidVelocity 
            return Vector3.zero;
        }

        /// <summary>
        /// Override this function to change the way fluid properties are calculated. E.g. to add a pressure gradient.
        /// </summary>
        /// <param name="aeroObject">The aerodynamic object for which the fluid properties are relevant.</param>
        public virtual void UpdateFluidProperties(AeroObject aeroObject)
        {
            // An example use for this could be to include a change in density and pressure with height of the aeroObject
            aeroObject.ao.FluidDensity = fluidProperties.density;
            aeroObject.ao.FluidDynamicViscosity = fluidProperties.dynamicViscosity;
            aeroObject.ao.FluidPressure = fluidProperties.pressure;
        }

        //private void UpdateObject(AeroObject aeroObject)
        //{
        //    // Update the fluid properties for the object
        //    UpdateFluidProperties(aeroObject);

        //    // And update the velocity of the fluid for the object
        //    aeroObject.ao.AddToFluidVelocity((VelocityFunction(aeroObject.transform.position)).ToNumerics());
        //}

        //private void UpdateParticle(ref ParticleSystem.Particle particle)
        //{
        //    // Particles only need the velocity updating
        //    particle.velocity += VelocityFunction(particle.position);
        //}


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out IFluidInteractive fluidInteractiveObject))
            {
                fluidInteractiveObject.SubscribeToFluidZone(this);
            }

            //if (other.gameObject.TryGetComponent(out AeroObject aeroObject))
            //{
            //    SubscribeToAeroObject(aeroObject);
            //    return;
            //}

                //if (other.gameObject.TryGetComponent(out FlowPointParticles flowParticles))
                //{
                //    SubscribeToFlowParticles(flowParticles);
                //}
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out IFluidInteractive fluidInteractiveObject))
            {
                fluidInteractiveObject.UnsubscribeFromFluidZone(this);
            }

            //if (other.gameObject.TryGetComponent(out AeroObject aeroObject))
            //{
            //    UnsubscribeFromAeroObject(aeroObject);
            //    return;
            //}

            //if (other.gameObject.TryGetComponent(out FlowPointParticles flowParticles))
            //{
            //    UnsubscribeFromFlowParticles(flowParticles);
            //}
        }

        // Moving away from events and over to a simple list of fluid zones
        // It's easier to maintain and more flexible for adding new interactive objects
        // as we can use an interface between fluid zones and different objects

        //// We need to subscribe to the aero object's event
        //private void SubscribeToAeroObject(AeroObject aeroObject)
        //{
        //    aeroObject.UpdateFluidEvent += UpdateObject;
        //}

        //// We need to unsubscribe from the aero object's event
        //private void UnsubscribeFromAeroObject(AeroObject aeroObject)
        //{
        //    aeroObject.UpdateFluidEvent -= UpdateObject;
        //}

        //private void SubscribeToFlowParticles(FlowPointParticles flowParticles)
        //{
        //    flowParticles.ParticleVelocityEvent += UpdateParticle;
        //}

        //private void UnsubscribeFromFlowParticles(FlowPointParticles flowParticles)
        //{
        //    flowParticles.ParticleVelocityEvent -= UpdateParticle;
        //}

        private void Reset()
        {
            fluidProperties = GlobalFluid.FluidProperties;
        }
    }
}