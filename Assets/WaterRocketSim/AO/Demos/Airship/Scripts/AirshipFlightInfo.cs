using UnityEngine;
using UnityEngine.UI;

namespace AerodynamicObjects.Demos
{
    public class AirshipFlightInfo : MonoBehaviour
    {
        public Text airspeed, altitude, throttle;
        public Airship airship;

        private void Update()
        {
            airspeed.text = (Mathf.Round(airship.airSpeedSensor.ao.LocalRelativeVelocity.ToUnity().magnitude)).ToString() + " m/s";
            altitude.text = (Mathf.Round(airship.transform.position.y)).ToString() + " m";
            throttle.text = (Mathf.Round(airship.engineSpeedInput * 100)).ToString() + " %";
        }
    }
}