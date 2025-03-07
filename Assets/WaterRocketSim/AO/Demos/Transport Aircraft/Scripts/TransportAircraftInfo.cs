using UnityEngine;
using UnityEngine.UI;

namespace AerodynamicObjects.Demos
{
    public class TransportAircraftInfo : MonoBehaviour
    {
        public Text airspeed, altitude, throttle;
        public TransportAircraft transportAircraft;

        // Update is called once per frame
        void Update()
        {
            airspeed.text = Mathf.Round(transportAircraft.AirSpeed).ToString() + " m/s";
            altitude.text = Mathf.Round(transportAircraft.Altitude).ToString() + " m";
            throttle.text = Mathf.Round(transportAircraft.Throttle).ToString() + " %";
        }
    }
}

