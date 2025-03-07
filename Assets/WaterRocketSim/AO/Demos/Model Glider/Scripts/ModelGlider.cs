using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Demos
{
    public class ModelGlider : MonoBehaviour
    {
        public Rigidbody aircraftRigidbody;
        public Transform CGMarker;
        public AeroObject fin, tailplane, airSpeedSensor;
        public float elevatorGain, rudderGain;
        float pitchTrim, yawTrim;
        public float controlSensitivity;
        InputAction yawTrimAction, pitchTrimAction;
        PlayerInput playerInput;


        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            yawTrimAction = playerInput.actions.FindAction("Yaw Trim");
            pitchTrimAction = playerInput.actions.FindAction("Pitch Trim");

            aircraftRigidbody.centerOfMass = CGMarker.localPosition;
        }

        void FixedUpdate()
        {
            ApplyControlInputs();
        }

        void ApplyControlInputs()
        {
            pitchTrim += pitchTrimAction.ReadValue<float>() * controlSensitivity * Time.fixedDeltaTime;
            pitchTrim = Mathf.Clamp(pitchTrim, -1f, 1f);
            yawTrim += yawTrimAction.ReadValue<float>() * controlSensitivity * Time.fixedDeltaTime;
            yawTrim = Mathf.Clamp(yawTrim, -1f, 1f);

            tailplane.ao.ControlCamber = elevatorGain * pitchTrim;
            fin.ao.ControlCamber = rudderGain * yawTrim;
        }
    }
}