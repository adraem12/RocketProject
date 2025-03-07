using UnityEngine;
using UnityEngine.UI;

namespace AerodynamicObjects.Demos
{
    public class StringForceSensor : MonoBehaviour
    {
        public ConfigurableJoint cj;
        public Vector3 measuredForce;
        public Text sensortext;
        public Canvas canvas;
        public Transform panelAnchor, connectedObjectAnchor;
        public LineRenderer lr;
        Camera mainCamera;
        // Start is called before the first frame update
        void Start()
        {
            mainCamera = Camera.main;
        }


        void FixedUpdate()
        {
            if (cj != null)
            {
                measuredForce = cj.currentForce;
                sensortext.text = (Mathf.Round(10f * measuredForce.magnitude) / 10).ToString() + " N";
            }
            canvas.transform.LookAt(mainCamera.transform);
            canvas.transform.Rotate(0, 180, 0);
            canvas.transform.rotation = Quaternion.Euler(Vector3.Scale(new Vector3(0, 1, 1), canvas.transform.rotation.eulerAngles));

            lr.SetPosition(0, connectedObjectAnchor.position);
            lr.SetPosition(1, panelAnchor.position);
            transform.position = connectedObjectAnchor.position;
        }
    }
}