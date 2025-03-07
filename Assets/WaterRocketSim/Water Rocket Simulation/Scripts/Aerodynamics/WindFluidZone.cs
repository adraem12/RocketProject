using UnityEngine;

namespace AerodynamicObjects
{
    public class WindFluidZone : FluidZone
    {
        public WindVelocity windVelocity;

        public bool enableTurbulence = true;
        [Tooltip("Controls the amount of turbulence relative to the base wind velocity. For a value of 1 the peak turbulent velocity is equal to the base wind velocity. For values greater than 1 the overall speed of the flow is increased.")]
        public float turbulenceMagnitude = 1f;

        [Tooltip("Controls how quickly turbulence changes with distance. Use a larger value for higher spatial frequency. Scaled with respect to the size of the fluid zone.")]
        public float turbulenceLengthScale = 1f;

        [Tooltip("Controls how quickly turbulence changes with time. Use a larger value for higher temporal frequency. Unscaled.")]
        public float turbulenceTimeScale = 1f;

        private float referenceLength = 1f;

        public bool useBoundaryLayerProfile = true;

        private void OnValidate()
        {
            turbulenceMagnitude = Mathf.Max(0, turbulenceMagnitude);
            turbulenceLengthScale = Mathf.Max(0, turbulenceLengthScale);
            turbulenceTimeScale = Mathf.Max(0, turbulenceTimeScale);
        }


        private void Update()
        {
            referenceLength = transform.lossyScale.magnitude;
        }

        public override Vector3 VelocityFunction(Vector3 position)
        {
            // I've kept the variable declarations local to the function here
            // For some reason I am compelled to make sure this is thread safe
            Vector3 uniformFluidVelocity = Vector3.zero;

            if (useBoundaryLayerProfile)
            {
                uniformFluidVelocity = windVelocity.GetVelocity() * Mathf.Sqrt(Mathf.Abs(position.y / 50f));
            }
            else
            {
                uniformFluidVelocity = windVelocity.GetVelocity();
            }



            if (!enableTurbulence)
            {
                return uniformFluidVelocity;
            }

            position *= turbulenceLengthScale / referenceLength;
            float time = Time.time * turbulenceTimeScale;

            return new Vector3(uniformFluidVelocity.x + 2f * windVelocity.speed * turbulenceMagnitude * (Mathf.PerlinNoise(position.y + time, position.z + time) - 0.5f),
                                uniformFluidVelocity.y + 2f * windVelocity.speed * turbulenceMagnitude * (Mathf.PerlinNoise(position.x + time, position.z + time) - 0.5f),
                                uniformFluidVelocity.z + 2f * windVelocity.speed * turbulenceMagnitude * (Mathf.PerlinNoise(position.x + time, position.y + time) - 0.5f));
        }
    }
}