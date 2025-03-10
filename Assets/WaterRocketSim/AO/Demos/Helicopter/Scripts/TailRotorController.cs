using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AerodynamicObjects.Demos
{
    public class TailRotorController : MonoBehaviour
    {
        AeroObject[] aeroObjects;
        [HideInInspector]
        public float angularVelocity;
        [HideInInspector]
        public float bladePitch;
        Rigidbody tailrotorHubRigidbody;
        [HideInInspector]
        public Vector3 aerodynamicForce;

        // Start is called before the first frame update
        void Start()
        {
            aeroObjects = GetComponentsInChildren<AeroObject>();
            tailrotorHubRigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            aerodynamicForce = Vector3.zero;
            for (int i = 0; i < aeroObjects.Length; i++)
            {
                aeroObjects[i].transform.localRotation = Quaternion.Euler(bladePitch, 0, 0);
                aerodynamicForce += aeroObjects[i].transform.TransformDirection(aeroObjects[i].ao.NetAerodynamicLoad.force.ToUnity()); // aeroforce in global frame
            }
            tailrotorHubRigidbody.angularVelocity = transform.TransformDirection(new Vector3(0, angularVelocity, 0));

            aerodynamicForce = transform.InverseTransformDirection(aerodynamicForce); // aeroforce in frame local to tail rotor (y normal to disc)

        }
    }
}