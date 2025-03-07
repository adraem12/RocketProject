using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Demos
{
    public class TransportAircraft : MonoBehaviour
    {
        public Rigidbody aircraftRigidbody, engine1Rigidbody, engine2Rigidbody, engine3Rigidbody, engine4Rigidbody;
        public Transform CoMMarker;

        public float aileronGain, elevatorGain, rudderGain, engineSpeedGain;
        public AeroObject portWing, starboardWing, tailplane, fin, airSpeedSensor;
        public float fixedDeltaTime, controlSensitivity, throttleSensitivity;
        float pitchInput, rollInput, yawInput, pitchTrim, engineSpeedInput;


        PlayerInput playerInput;
        InputAction rollAction, pitchAction, yawAction, engineSpeedAction, pitchTrimAction;


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
            Time.fixedDeltaTime = fixedDeltaTime;// physics update rate
            aircraftRigidbody.centerOfMass = CoMMarker.localPosition;
            Physics.defaultMaxAngularSpeed = 200; // raise the default angular velocity limit to something sensible

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
            yawInput = Mathf.MoveTowards(yawInput, yawAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            engineSpeedInput += engineSpeedAction.ReadValue<float>() * 0.1f * throttleSensitivity * Time.fixedDeltaTime;
            engineSpeedInput = Mathf.Clamp(engineSpeedInput, -0.5f, 1f);

            portWing.ao.ControlCamber = aileronGain * rollInput;
            starboardWing.ao.ControlCamber = -aileronGain * rollInput;
            tailplane.ao.ControlCamber = elevatorGain * (pitchTrim + pitchInput);
            fin.ao.ControlCamber = rudderGain * yawInput;
            engine1Rigidbody.angularVelocity = engine1Rigidbody.transform.TransformDirection(new Vector3(0, 0, engineSpeedInput * engineSpeedGain));
            engine2Rigidbody.angularVelocity = engine2Rigidbody.transform.TransformDirection(new Vector3(0, 0, engineSpeedInput * engineSpeedGain));
            engine3Rigidbody.angularVelocity = engine3Rigidbody.transform.TransformDirection(new Vector3(0, 0, -engineSpeedInput * engineSpeedGain));
            engine4Rigidbody.angularVelocity = engine4Rigidbody.transform.TransformDirection(new Vector3(0, 0, -engineSpeedInput * engineSpeedGain));

        }

        public float AirSpeed { get { return airSpeedSensor.ao.LocalRelativeVelocity.ToUnity().magnitude; } }
        public float Altitude { get { return transform.position.y; } }
        public float Throttle { get { return engineSpeedInput * 100; } }

    }
}