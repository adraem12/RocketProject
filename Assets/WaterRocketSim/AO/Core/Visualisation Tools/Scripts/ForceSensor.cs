using UnityEditor;
using UnityEngine;

namespace AerodynamicObjects
{
    [DefaultExecutionOrder(100)]
    public class ForceSensor : MonoBehaviour
    {
        [Tooltip("The aero objects to measure.")]
        public AeroObject[] aeroObjects;

        [Tooltip("The net force in global coordinates.")]
        public Vector3 netForce;

        [Tooltip("The net moment in global coordinates. This is a combination of the moments from the aero objects and the moments produced by the aero object's forces. Moving this sensor transform's position will affect the measured net moment.")]
        public Vector3 netMoment;

        private Vector3 sensorPosition;

        private void FixedUpdate()
        {
            netForce = Vector3.zero;
            netMoment = Vector3.zero;
            sensorPosition = transform.position;

            for (int i = 0; i < aeroObjects.Length; i++)
            {
                // Resolving the force from the aero object
                Vector3 objectForce = aeroObjects[i].transform.TransformDirection(aeroObjects[i].ao.NetAerodynamicLoad.force.ToUnity());
                netForce += objectForce;
                netMoment += Vector3.Cross(objectForce, sensorPosition - aeroObjects[i].transform.position);

                // Resolving the moment from the aero object
                netMoment += aeroObjects[i].transform.TransformDirection(aeroObjects[i].ao.NetAerodynamicLoad.moment.ToUnity());
            }

            // Get the force and moment in the local coordinates for the sensor
            netForce = transform.InverseTransformDirection(netForce);
            netMoment = transform.InverseTransformDirection(netMoment);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            Handles.Label(transform.position, "Force: " + netForce.ToString() + '\n' + "Moment: " + netMoment.ToString(), style);
        }
#endif
    }
}