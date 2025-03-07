using UnityEngine;

namespace AerodynamicObjects.Demos
{
    public class BalloonString : MonoBehaviour
    {
        LineRenderer lr;
        public Transform stringStart, stringEnd;
        // Start is called before the first frame update
        void Start()
        {
            lr = GetComponent<LineRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            lr.SetPosition(0, stringStart.position);
            lr.SetPosition(1, stringEnd.position);
        }
    }
}