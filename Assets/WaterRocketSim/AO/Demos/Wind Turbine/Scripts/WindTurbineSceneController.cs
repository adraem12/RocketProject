using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AerodynamicObjects.Demos
{
    public class WindTurbineSceneController : MonoBehaviour
    {
        public WindTurbineController turbineController;

        public void UpdateWindDirection(float newValue)
        {
            turbineController.windDirection = newValue;
        }
        public void UpdateWindSpeed(float newValue)
        {
            turbineController.windSpeed = newValue;
        }
        public void UpdateBladePitch(float newValue)
        {
            turbineController.bladePitchAngle = newValue;
        }
        public void UpdateBladeAspectRatio(float newValue)
        {
            turbineController.bladeAspectRatio = newValue;
        }
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // loads current scene
        }
    }
}