using UnityEngine;
using UnityEngine.UI;

namespace AerodynamicObjects.Demos
{
    public class HangGliderFlightInfo : MonoBehaviour
    {
        public Text airspeed, altitude, throttle;
        public HangGlider hangGlider;

        // Update is called once per frame
        void Update()
        {
            airspeed.text = Mathf.Round(hangGlider.AirSpeed).ToString() + " m/s";
            altitude.text = Mathf.Round(hangGlider.Altitude).ToString() + " m";
            throttle.text = "0 %";
        }
    }
}