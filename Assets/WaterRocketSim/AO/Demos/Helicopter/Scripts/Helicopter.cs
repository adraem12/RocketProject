using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Demos
{
    //Main code for controlling all aspects of the helicopter
    public class Helicopter : MonoBehaviour
    {
        public Rigidbody fuselageRigidbody;
        public Transform CGMarker;
        float yawRateDemand, pitchRateDemand, rollRateDemand, heaveRateDemand;
        public float yawRateGain, heaveRateGain;
        public float pitchRateGain, rollRateGain, rotorSpeedGain;

        public TailRotorController tailRotorController;
        public MainRotorController mainRotorController;
        float mainRotorSpeedDemand;
        float bladePitchInput, bladeRollInput;
        public AeroObject airSpeedSensor;
        public float fixedDeltaTime;
        public float controlSensitivity;
        PlayerInput playerInput;
        InputAction rollAction;
        InputAction pitchAction;
        InputAction yawAction;
        InputAction heaveAction;
        InputAction rotorSpeedAction;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            rollAction = playerInput.actions.FindAction("Roll");
            pitchAction = playerInput.actions.FindAction("Pitch");
            yawAction = playerInput.actions.FindAction("Yaw");
            heaveAction = playerInput.actions.FindAction("Heave");
            rotorSpeedAction = playerInput.actions.FindAction("Rotor Speed");
        }

        void Start()
        {
            Physics.defaultMaxAngularSpeed = 200; // raise the default angular velocity limit to something sensible
            Time.fixedDeltaTime = fixedDeltaTime;// physics update rate

        }

        void FixedUpdate()
        {
            pitchRateDemand = Mathf.MoveTowards(pitchRateDemand, pitchAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            rollRateDemand = Mathf.MoveTowards(rollRateDemand, rollAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            yawRateDemand = Mathf.MoveTowards(yawRateDemand, yawAction.ReadValue<float>(), controlSensitivity * Time.fixedDeltaTime);
            heaveRateDemand += 0.02f * heaveAction.ReadValue<float>() * controlSensitivity * Time.fixedDeltaTime;
            mainRotorSpeedDemand += rotorSpeedAction.ReadValue<float>() * controlSensitivity * Time.fixedDeltaTime;
            mainRotorSpeedDemand = Mathf.Clamp(mainRotorSpeedDemand, 0, 100);
            mainRotorController.collective = heaveRateGain * heaveRateDemand;
            tailRotorController.angularVelocity = 10 * mainRotorController.hubAngularVelocity;
            mainRotorController.angularVelocityDemand = mainRotorSpeedDemand;
            tailRotorController.bladePitch = Mathf.Clamp(yawRateGain * (yawRateDemand - fuselageRigidbody.transform.InverseTransformDirection(fuselageRigidbody.angularVelocity).y), -20, 20);
            bladePitchInput = pitchRateDemand;
            bladeRollInput = rollRateDemand;
            mainRotorController.cyclic = Mathf.Clamp(new Vector2(bladePitchInput, bladeRollInput).magnitude, -15, 15);
            mainRotorController.phase = Mathf.Rad2Deg * Mathf.Atan2(bladePitchInput, bladeRollInput); ;

        }

        public float AirSpeed { get { return airSpeedSensor.ao.LocalRelativeVelocity.ToUnity().magnitude; } }
        public float Altitude { get { return transform.position.y; } }
        public float Throttle { get { return 100f * mainRotorSpeedDemand / rotorSpeedGain; } }

    }
}