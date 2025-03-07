using UnityEngine;

namespace AerodynamicObjects
{
    public class WeightArrow : ArrowComponent
    {
        public Rigidbody rb;
        Arrow arrow;
        public Color colour = new Color(1, 199f / 255f, 31f / 255f, 0.5f);

        void OnEnable()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    Debug.LogError("No rigidbody component found for " + gameObject.name);
                }
            }
            arrow = new Arrow(colour, "Weight Arrow", Resources.Load("Weight Texture") as Texture2D);
        }

        void FixedUpdate()
        {
            if (UseCoefficientForScale)
            {
                // This doesn't make a lot of sense but gravity has direction and mass doesn't, so it's the only
                // thing we can effectively use as a "coefficient"
                SetArrowPositionAndRotationFromVector(arrow, Physics.gravity, rb.position);
            }
            else
            {
                SetArrowPositionAndRotationFromVector(arrow, rb.mass * Physics.gravity, rb.position);
            }
        }
    }
}