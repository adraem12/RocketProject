using UnityEngine;

namespace AerodynamicObjects
{
    public class RotationalLiftArrow : ArrowComponent
    {
        AeroObject aeroObject;
        Arrow arrow;
        public int modelIndex;
        RotationalLiftModel model;
        public Color colour = new Color(78f / 255f, 224f / 255f, 62f / 255f, 0.5f);

        void OnEnable()
        {
            aeroObject = TryGetAeroObject();
            model = aeroObject.GetModel<RotationalLiftModel>();

            if (model == null)
            {
                Debug.LogWarning("No rotational lift model was found for the rotational lift arrow component on " + name + ". Destroying the arrow component.");
                Destroy(this);
                return;
            }

            modelIndex = aeroObject.GetModelIndex<RotationalLiftModel>();
            arrow = new Arrow(colour, "Rotational Lift Arrow");
        }

        void FixedUpdate()
        {
            // Get the lift force in the earth frame
            Vector3 lift_earthFrame = aeroObject.transform.TransformDirection(aeroObject.ao.AerodynamicLoads[modelIndex].force.ToUnity());

            if (UseCoefficientForScale)
            {
                // Need to use the absolute value of the coefficient because we already have the direction from the force
                SetArrowPositionAndRotationFromVector(arrow, model.CLr.ToUnity(), aeroObject.transform.position);
            }
            else
            {
                SetArrowPositionAndRotationFromVector(arrow, lift_earthFrame, aeroObject.transform.position);
            }
        }
    }
}