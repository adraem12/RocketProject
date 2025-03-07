using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Demos
{
    public class Quadcopter : MonoBehaviour
    {
        public Rigidbody aircraftRigidbody, engine1Rigidbody, engine2Rigidbody, engine3Rigidbody, engine4Rigidbody;
        public Transform CGMarker;
        public AeroObject airSpeedSensor;
        public float baseEngineSpeed;
        public float pitchGain, rollGain, yawGain, engineSpeedGain;
        float engineSpeed1, engineSpeed2, engineSpeed3, engineSpeed4;
        public float fixedDeltaTime;
        PlayerInput playerInput;
        float pitchInput, rollInput, yawInput, engineSpeedInput;
        InputAction rollAction, pitchAction, yawAction, engineSpeedAction;
        public float controlSensitivity, throttleSensitivity;
        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            rollAction = playerInput.actions.FindAction("Roll");
            pitchAction = playerInput.actions.FindAction("Pitch");
            yawAction = playerInput.actions.FindAction("Yaw");
            engineSpeedAction = playerInput.actions.FindAction("Engine Speed");


        }

        // Engine numbering: 1 aft, 2 fwd, 3 stb, 4 port
        void Start()
        {
            aircraftRigidbody.centerOfMass = CGMarker.localPosition;
            Time.fixedDeltaTime = fixedDeltaTime;// physics update rate
            Physics.defaultMaxAngularSpeed = 200;
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
            engineSpeedInput += engineSpeedAction.ReadValue<float>() * 0.1f * throttleSensitivity * Time.fixedDeltaTime;
            engineSpeedInput = Mathf.Clamp(engineSpeedInput, 0f, 1f);



            baseEngineSpeed = engineSpeedGain * engineSpeedInput;
            engineSpeed3 = baseEngineSpeed + rollGain * rollInput;
            engineSpeed4 = baseEngineSpeed - rollGain * rollInput;
            engineSpeed1 = baseEngineSpeed + pitchGain * pitchInput;
            engineSpeed2 = baseEngineSpeed - pitchGain * pitchInput;
            aircraftRigidbody.AddRelativeTorque
                (new Vector3(0, yawGain * (yawInput - aircraftRigidbody.transform.InverseTransformDirection(aircraftRigidbody.angularVelocity).y), 0));

            engine1Rigidbody.angularVelocity = engine1Rigidbody.transform.TransformDirection(new Vector3(0, 0, engineSpeed1));
            engine2Rigidbody.angularVelocity = engine2Rigidbody.transform.TransformDirection(new Vector3(0, 0, engineSpeed2));
            engine3Rigidbody.angularVelocity = engine3Rigidbody.transform.TransformDirection(new Vector3(0, 0, engineSpeed3));
            engine4Rigidbody.angularVelocity = engine4Rigidbody.transform.TransformDirection(new Vector3(0, 0, engineSpeed4));

        }
        public float AirSpeed { get { return airSpeedSensor.ao.LocalRelativeVelocity.ToUnity().magnitude; } }
        public float Altitude { get { return transform.position.y; } }
        public float Throttle { get { return engineSpeedInput * 100; } }
    }
}