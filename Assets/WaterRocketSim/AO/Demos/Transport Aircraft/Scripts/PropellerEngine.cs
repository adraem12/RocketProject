using UnityEngine;

namespace AerodynamicObjects.Demos
{
    public class PropellerEngine : MonoBehaviour
    {

        public float bladePitchAngle, propellerDiameter, bladeAspectRatio;
        public Transform[] bladeGeometry;
        float chord;

        // Update is called once per frame
        void Update()
        {
            //UpdatePropellerGeometry();
        }

        public void UpdatePropellerGeometry()
        {
            for (int i = 0; i < bladeGeometry.Length; i++)
            {

                bladeGeometry[i].localEulerAngles = new Vector3(bladePitchAngle, 0, 0);
                chord = 0.5f * propellerDiameter / bladeAspectRatio;
                bladeGeometry[i].localScale = new Vector3(propellerDiameter / 2, 0.05f * chord, chord); // span, thickness, chord

            }
        }
    }
}