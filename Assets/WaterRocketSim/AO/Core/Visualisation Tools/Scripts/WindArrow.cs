using UnityEngine;

namespace AerodynamicObjects
{
    public class WindArrow : ArrowComponent
    {
        AeroObject aeroObject;
        Arrow windArrow;
        public Color colour = new Color(35f / 255f, 211f / 255f, 251f / 255f, 0.5f);

        private void OnEnable()
        {
            aeroObject = TryGetAeroObject();
            windArrow = new Arrow(colour, "Wind Arrow");
        }

        private void Reset()
        {
            // For wind we need the arrow head to point at the position instead of coming out of the position
            HeadAimsAtPoint = true;
        }

        Vector3 velocity;

        void FixedUpdate()
        {
            velocity = -aeroObject.ao.GlobalRelativeVelocity.ToUnity();


            if (UseCoefficientForScale)
            {
                SetArrowPositionAndRotationFromVector(windArrow, velocity.normalized, aeroObject.transform.position);
            }
            else
            {
                // Wind just uses normalised vector instead when in coefficient mode
                SetArrowPositionAndRotationFromVector(windArrow, velocity, aeroObject.transform.position);
            }
        }
    }
}