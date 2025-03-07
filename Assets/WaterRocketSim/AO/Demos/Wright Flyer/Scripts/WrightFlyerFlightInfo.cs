using UnityEngine;
using UnityEngine.UI;

namespace AerodynamicObjects.Demos
{
    public class WrightFlyerFlightInfo : MonoBehaviour
    {
        public Text airspeed, altitude, throttle;
        public WrightFlyer wrightFlyer;

        // Update is called once per frame
        void Update()
        {
            airspeed.text = Mathf.Round(wrightFlyer.AirSpeed).ToString() + " m/s";
            altitude.text = Mathf.Round(wrightFlyer.Altitude).ToString() + " m";
            throttle.text = Mathf.Round(wrightFlyer.Throttle).ToString() + " %";
        }
    }
}
