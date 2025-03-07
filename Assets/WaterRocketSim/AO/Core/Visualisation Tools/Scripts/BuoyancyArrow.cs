using UnityEngine;

namespace AerodynamicObjects
{
    public class BuoyancyArrow : ArrowComponent
    {
        AeroObject aeroObject;
        BuoyancyModel model;
        int modelIndex;
        Arrow arrow;
        public Color colour = new Color(0.15f, 0.15f, 0.15f, 0.5f);

        private void OnEnable()
        {
            aeroObject = TryGetAeroObject();
            model = aeroObject.GetModel<BuoyancyModel>();

            if (model == null)
            {
                Debug.LogWarning("No buoyancy model was found for the buoyancy arrow component on " + name + ". Destroying the buoyancy arrow component.");
                Destroy(this);
                return;
            }

            modelIndex = aeroObject.GetModelIndex<BuoyancyModel>();
            arrow = new Arrow(colour, "Buoyancy Arrow");
        }


        void FixedUpdate()
        {
            Vector3 force = aeroObject.transform.TransformDirection(aeroObject.ao.AerodynamicLoads[modelIndex].force.ToUnity());

            if (UseCoefficientForScale)
            {
                SetArrowPositionAndRotationFromVector(arrow, Physics.gravity.normalized, aeroObject.transform.position);
            }
            else
            {
                SetArrowPositionAndRotationFromVector(arrow, force, aeroObject.transform.position);
            }
        }
    }
}