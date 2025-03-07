using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Demos
{
    public class HangGlider : MonoBehaviour
    {
        public Rigidbody aircraftRigidbody;
        public Transform CGMarker;
        public float aileronGain, elevatorGain;
        public AeroObject portWingOuter, starboardWingOuter, airSpeedSensor;
        public float fixedDeltaTime;
        public float controlSensitivity;
        public float pitchTrim;
        PlayerInput playerInput;
        InputAction rollAction;
        InputAction pitchAction;
        InputAction pitchTrimAction;

        float pitchInput, rollInput;


        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            rollAction = playerInput.actions.FindAction("Roll");
            pitchAction = playerInput.actions.FindAction("Pitch");
            pitchTrimAction = playerInput.actions.FindAction("Pitch Trim");

        }

        void Start()
        {
            aircraftRigidbody.centerOfMass = CGMarker.localPosition;
            Time.fixedDeltaTime = fixedDeltaTime;// physics update rate
        }


        void FixedUpdate()
        {
            ApplyControlInputs();
        }
        void ApplyControlInputs()
        {
            pitchTrim += pitchTrimAction.ReadValue<float>() * 0.1f * controlSensitivity * Time.fixedDeltaTime;
            pitchTrim = Mathf.Clamp(pitchTrim, -0.1f, 0.1f);
            pitchInput = Mathf.MoveTowards(pitchInput, pitchAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            rollInput = Mathf.MoveTowards(rollInput, rollAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            portWingOuter.ao.ControlCamber = elevatorGain * (pitchTrim + pitchInput) + aileronGain * rollInput;
            starboardWingOuter.ao.ControlCamber = elevatorGain * (pitchTrim + pitchInput) - aileronGain * rollInput;
        }

        public float AirSpeed { get { return airSpeedSensor.ao.LocalRelativeVelocity.ToUnity().magnitude; } }
        public float Altitude { get { return transform.position.y; } }
        public float Throttle { get { return 0; } }
    }
}