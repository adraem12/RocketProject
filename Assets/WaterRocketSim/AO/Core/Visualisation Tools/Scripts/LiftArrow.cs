using System;
using UnityEngine;

namespace AerodynamicObjects
{
    public class LiftArrow : ArrowComponent
    {
        AeroObject aeroObject;
        Arrow arrow;
        int modelIndex;
        LiftModel model;
        public Color colour = new Color(78f / 255f, 224f / 255f, 62f / 255f, 0.5f);

        void OnEnable()
        {
            aeroObject = TryGetAeroObject();
            model = aeroObject.GetModel<LiftModel>();

            if (model == null)
            {
                Debug.LogWarning("No lift model was found for the lift arrow component on " + name + ". Destroying the lift arrow component.");
                Destroy(this);
                return;
            }

            modelIndex = aeroObject.GetModelIndex<LiftModel>();
            arrow = new Arrow(colour, "Lift Arrow");
        }

        void FixedUpdate()
        {
            // Get the lift force in the earth frame
            Vector3 lift_earthFrame = aeroObject.transform.TransformDirection(aeroObject.ao.AerodynamicLoads[modelIndex].force.ToUnity());

            // ===============================================
            // Draw the arrow at the aerodynamic centre!!!
            // ===============================================

            // Get the AC in body coordinates, using sideslip then convert to object (Unity's local) frame
            Vector3 aerodynamicCentre = aeroObject.ao.TransformBodyToLocal(new AerodynamicObjects.Numerics.Vector3(-model.aerodynamicCentre_z * Math.Sin(aeroObject.ao.AngleOfSideslip), 0, -model.aerodynamicCentre_z * Math.Cos(aeroObject.ao.AngleOfSideslip))).ToUnity();
            //Vector3 aerodynamicCentre = new Vector3((float)model.aerodynamicCentre_z * Mathf.Sin((float)aeroObject.ao.AngleOfSideslip), 0, -(float)model.aerodynamicCentre_z * Mathf.Cos((float)aeroObject.ao.AngleOfSideslip));

            aerodynamicCentre = aeroObject.transform.position + aeroObject.transform.TransformDirection(aerodynamicCentre);

            if (UseCoefficientForScale)
            {
                // Need to use the absolute value of the coefficient because we already have the direction from the force
                SetArrowPositionAndRotationFromVector(arrow, Mathf.Abs((float)model.CL) * lift_earthFrame.normalized, aerodynamicCentre);
            }
            else
            {
                SetArrowPositionAndRotationFromVector(arrow, lift_earthFrame, aerodynamicCentre);
            }
        }
    }
}