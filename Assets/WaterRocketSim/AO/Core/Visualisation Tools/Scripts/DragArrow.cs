using UnityEngine;

namespace AerodynamicObjects
{
    public class DragArrow : ArrowComponent
    {
        AeroObject aeroObject;
        DragModel model;
        int modelIndex;
        Arrow arrow;
        public Color colour = new Color(1, 31f / 255f, 31f / 255f, 0.5f);

        private void OnEnable()
        {
            aeroObject = TryGetAeroObject();
            model = aeroObject.GetModel<DragModel>();

            if (model == null)
            {
                Debug.LogWarning("No drag model was found for the drag arrow component on " + name + ". Destroying the drag arrow component.");
                Destroy(this);
                return;
            }

            modelIndex = aeroObject.GetModelIndex<DragModel>();
            arrow = new Arrow(colour, "Drag Arrow");
        }


        void FixedUpdate()
        {
            Vector3 dragEarthFrame = aeroObject.transform.TransformDirection(aeroObject.ao.AerodynamicLoads[modelIndex].force.ToUnity());

            if (UseCoefficientForScale)
            {
                SetArrowPositionAndRotationFromVector(arrow, Mathf.Abs((float)model.CD) * dragEarthFrame.normalized, aeroObject.transform.position);
            }
            else
            {
                SetArrowPositionAndRotationFromVector(arrow, dragEarthFrame, aeroObject.transform.position);
            }
        }
    }
}