using UnityEngine;
using UnityEngine.UI;

namespace AerodynamicObjects.Demos
{
    public class QuadcopterFlightInfo : MonoBehaviour
    {
        public Text airspeed, altitude, throttle;
        public Quadcopter quadcopter;

        // Update is called once per frame
        void Update()
        {
            airspeed.text = Mathf.Round(quadcopter.AirSpeed).ToString() + " m/s";
            altitude.text = Mathf.Round(quadcopter.Altitude).ToString() + " m";
            throttle.text = Mathf.Round(quadcopter.Throttle).ToString() + " %";
        }
    }
}
