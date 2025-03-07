using UnityEngine;

namespace AerodynamicObjects.Demos
{
    public class Generator : MonoBehaviour
    {
        Rigidbody hub;
        public Vector3 torque;
        public float rotorAngularVelocity, targetRotorAngularVelocity, controllerGain, controllerTorque, brakeGain, brakeTorque, power, powerBrightnessGain, powerFilterTimeConstant;
        public Light powerLight;
        float targetPower;
        public float driveTorque;
        public Material lightMaterial;

        // Start is called before the first frame update
        void Start()
        {
            hub = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            rotorAngularVelocity = hub.transform.InverseTransformDirection(hub.angularVelocity).z;
            controllerTorque = 0;
            if (rotorAngularVelocity > targetRotorAngularVelocity) //we are generating
            {
                controllerTorque = controllerGain * (targetRotorAngularVelocity - rotorAngularVelocity);
                hub.AddRelativeTorque(new Vector3(0, 0, controllerTorque));
            }
            if (rotorAngularVelocity < 0) //we are going backwards - use the brake
            {
                brakeTorque = brakeGain * (0 - rotorAngularVelocity);
                hub.AddRelativeTorque(new Vector3(0, 0, brakeTorque));
            }

            targetPower = -controllerTorque * rotorAngularVelocity;
            power += (targetPower - power) * powerFilterTimeConstant * Time.deltaTime;

            powerLight.intensity = power * powerBrightnessGain;

            lightMaterial.SetColor("_EmissionColor", new Color(.1f, 1, .1f) * power * powerBrightnessGain * 10);
        }
    }
}