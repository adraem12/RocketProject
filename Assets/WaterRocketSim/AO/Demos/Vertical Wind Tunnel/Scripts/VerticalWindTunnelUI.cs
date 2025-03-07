using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AerodynamicObjects.Demos
{
    public class VerticalWindTunnelUI : MonoBehaviour
    {
        public FanFluidZone fanEffector;

        public void ChangeFanSpeed(float newValue)
        {
            fanEffector.windSpeedAtFanFace = newValue;
        }
    }
}