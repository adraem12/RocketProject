using UnityEngine;

namespace AerodynamicObjects
{
    public class GlobalFluid : FluidZone
    {
        private static FluidProperties _fluidProperties;

        /// <summary>
        /// Properties of the global fluid.
        /// </summary>
        public static FluidProperties FluidProperties
        {
            get
            {
                if (_fluidProperties == null)
                {
                    _fluidProperties = Resources.Load("GlobalFluidProperties") as FluidProperties;
                }
                return _fluidProperties;
            }
            set { _fluidProperties = value; }
        }


        /// <summary>
        /// Velocity of the global fluid.
        /// </summary>
        public static Vector3 FluidVelocity;

        /// <summary>
        /// The velocity value accessible in the inspector.
        /// </summary>
        public Vector3 fluidVelocity;

        public override void Awake()
        {
            FluidVelocity = fluidVelocity;
            FluidProperties = fluidProperties;
        }

        private void FixedUpdate()
        {
            FluidVelocity = fluidVelocity;
            FluidProperties = fluidProperties;
        }
    }
}