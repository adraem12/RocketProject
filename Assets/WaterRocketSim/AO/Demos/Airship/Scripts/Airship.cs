using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Demos
{
    public class Airship : MonoBehaviour
    {
        public Rigidbody aircraftRigidbody, PortEngineRigidbody, StarboardEngineRigidbody;
        public Transform CGMarker, StarboardEngineGimbal, PortEngineGimbal;
        public float elevatorGain, rudderGain, engineSpeedGain;
        public AeroObject horizontalTail, verticalTail, airSpeedSensor;
        [Range(-45, 45)]
        public float propellerPitchAngle;
        [HideInInspector]
        public float pitchInput, rollInput, yawInput, engineSpeedInput, verticalInput;
        public float engineYawGain;
        public Transform blade1Geometry, blade2Geometry, blade3Geometry, blade4Geometry;
        public float elevatorTrim, verticalTrim, lateralTrim;
        public float fixedDeltaTime;
        PlayerInput playerInput;
        InputAction rollAction, pitchAction, yawAction, engineSpeedAction, verticalAction;
        public float controlSensitivity, throttleSensitivity;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            rollAction = playerInput.actions.FindAction("Roll");
            pitchAction = playerInput.actions.FindAction("Pitch");
            yawAction = playerInput.actions.FindAction("Yaw");
            engineSpeedAction = playerInput.actions.FindAction("Engine Speed");
            verticalAction = playerInput.actions.FindAction("Vertical");

        }
        void Start()
        {
            aircraftRigidbody.centerOfMass = CGMarker.localPosition;
            blade1Geometry.localEulerAngles = blade2Geometry.localEulerAngles = blade3Geometry.localEulerAngles = blade4Geometry.localEulerAngles = new Vector3(0, propellerPitchAngle, 0); ;
            Time.fixedDeltaTime = fixedDeltaTime;// physics update rate    
            Physics.defaultMaxAngularSpeed = 200; // raise the default angular velocity limit to something sensible
        }

        void FixedUpdate()
        {
            ApplyControlInputs();
        }
        void ApplyControlInputs()
        {
            pitchInput = Mathf.MoveTowards(pitchInput, pitchAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            rollInput = Mathf.MoveTowards(rollInput, rollAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            yawInput = Mathf.MoveTowards(yawInput, yawAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            engineSpeedInput += engineSpeedAction.ReadValue<float>() * throttleSensitivity * Time.fixedDeltaTime;
            engineSpeedInput = Mathf.Clamp(engineSpeedInput, -1f, 1f);
            verticalInput += verticalAction.ReadValue<float>() * 0.1f * controlSensitivity * Time.fixedDeltaTime;

            horizontalTail.ao.ControlCamber = elevatorGain * pitchInput;
            verticalTail.ao.ControlCamber = rudderGain * yawInput;
            PortEngineRigidbody.angularVelocity =
                PortEngineRigidbody.transform.TransformDirection(new Vector3(0, 0, engineYawGain * yawInput + engineSpeedGain * engineSpeedInput));
            StarboardEngineRigidbody.angularVelocity =
                StarboardEngineRigidbody.transform.TransformDirection(new Vector3(0, 0, -engineYawGain * yawInput + engineSpeedGain * engineSpeedInput));

            StarboardEngineGimbal.transform.localEulerAngles = PortEngineGimbal.transform.localEulerAngles =
               90 * new Vector3(-verticalInput, 0, 0);
        }
    }
}