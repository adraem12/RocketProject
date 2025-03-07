using UnityEngine;
using UnityEngine.UI;

namespace AerodynamicObjects.Demos
{
    public class HelicopterFlightInfo : MonoBehaviour
    {
        public Text airspeed, altitude, throttle;
        public Helicopter helicopter;

        // Update is called once per frame
        void Update()
        {
            airspeed.text = Mathf.Round(helicopter.AirSpeed).ToString() + " m/s";
            altitude.text = Mathf.Round(helicopter.Altitude).ToString() + " m";
            throttle.text = Mathf.Round(helicopter.Throttle).ToString() + " %";
        }
    }
}

