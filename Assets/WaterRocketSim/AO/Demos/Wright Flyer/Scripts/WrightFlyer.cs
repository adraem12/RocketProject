using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Demos
{
    public class WrightFlyer : MonoBehaviour
    {
        public Rigidbody aircraftRigidbody, portEngineRigidbody, starboardEngineRigidbody;
        public Transform CoMMarker, blade1Geometry, blade2Geometry, blade3Geometry, blade4Geometry;
        public float aileronGain, elevatorGain, rudderGain, engineSpeedGain;
        public AeroObject portWingLower, starboardWingLower, portWingUpper, starboardWingUpper, airSpeedSensor;
        public Transform forePlane, portFin, starboardFin;
        [Range(-45, 45)]
        public float propellerPitchAngle;
        public float fixedDeltaTime;
        PlayerInput playerInput;
        float pitchInput, rollInput, yawInput, pitchTrim, engineSpeedInput;
        InputAction rollAction, pitchAction, yawAction, engineSpeedAction, pitchTrimAction;
        public float controlSensitivity, throttleSensitivity;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            rollAction = playerInput.actions.FindAction("Roll");
            pitchAction = playerInput.actions.FindAction("Pitch");
            yawAction = playerInput.actions.FindAction("Yaw");
            engineSpeedAction = playerInput.actions.FindAction("Engine Speed");
            pitchTrimAction = playerInput.actions.FindAction("Pitch Trim");

        }

        void Start()
        {
            aircraftRigidbody.centerOfMass = CoMMarker.localPosition;
            blade1Geometry.localEulerAngles = blade2Geometry.localEulerAngles = new Vector3(0, propellerPitchAngle, 0);
            blade3Geometry.localEulerAngles = blade4Geometry.localEulerAngles = new Vector3(0, -propellerPitchAngle, 0);
            Time.fixedDeltaTime = fixedDeltaTime;// physics update rate
            Physics.defaultMaxAngularSpeed = 200;
        }

        public float AirSpeed { get { return airSpeedSensor.ao.LocalRelativeVelocity.ToUnity().magnitude; } }
        public float Altitude { get { return transform.position.y; } }
        public float Throttle { get { return engineSpeedInput * 100; } }

        void FixedUpdate()
        {
            ApplyControlInputs();
        }
        void ApplyControlInputs()
        {

            pitchTrim += pitchTrimAction.ReadValue<float>() * 0.1f * controlSensitivity * Time.fixedDeltaTime;
            pitchTrim = Mathf.Clamp(pitchTrim, -1f, 1f);
            pitchInput = Mathf.MoveTowards(pitchInput, pitchAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            rollInput = Mathf.MoveTowards(rollInput, rollAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            yawInput = Mathf.MoveTowards(yawInput, yawAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            engineSpeedInput += engineSpeedAction.ReadValue<float>() * 0.1f * throttleSensitivity * Time.fixedDeltaTime;
            engineSpeedInput = Mathf.Clamp(engineSpeedInput, 0f, 1f);

            portWingLower.ao.ControlCamber = portWingUpper.ao.ControlCamber = aileronGain * rollInput;
            starboardWingLower.ao.ControlCamber = starboardWingUpper.ao.ControlCamber = -aileronGain * rollInput;
            forePlane.localEulerAngles = new Vector3(-4 + elevatorGain * (pitchTrim + pitchInput), 0, 0);
            portFin.localEulerAngles = starboardFin.localEulerAngles = new Vector3(0, rudderGain * yawInput, 90);
            portEngineRigidbody.angularVelocity = portEngineRigidbody.transform.TransformDirection(new Vector3(0, 0, -engineSpeedInput * engineSpeedGain));
            starboardEngineRigidbody.angularVelocity = starboardEngineRigidbody.transform.TransformDirection(new Vector3(0, 0, engineSpeedInput * engineSpeedGain));

        }

    }
}