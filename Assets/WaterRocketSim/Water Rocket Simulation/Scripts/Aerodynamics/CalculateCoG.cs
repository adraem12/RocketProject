using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateCoG : MonoBehaviour
{
    Rigidbody[] rigidbodies;
    public Vector3 cg = Vector3.zero; // Rocket overall centre of gravity position
    public Transform centreOfGravityMarker; // graphic object for c of g
    public float massSum; // total mass of rocket

    public void Awake()
    {
        GetRigidBodies();
    }

    public void GetRigidBodies()
    {
        rigidbodies = gameObject.GetComponentsInChildren<Rigidbody>();
    }

    public void FindCoG()
    {

        Vector3 momentSum = Vector3.zero;
        massSum = 0;
        int count = 0;
        for (int i = 0; i < rigidbodies.Length; i++)
        {

            if (rigidbodies[i].isKinematic is false)
            {
                count++;
                float mass = rigidbodies[i].mass;
                Vector3 position = rigidbodies[i].worldCenterOfMass;

                momentSum += position * mass;
                massSum += mass;

                // in this mode we need to turn off gravity because for some reason the calculated cg is not
                // exactly on the true cg and there is a remaining small gravitational moment. No obvious cause for this...
                if (SimulationConfig.testMode == SimulationConfig.TestMode.WindTunnel)                                                             
                {
                    rigidbodies[i].useGravity = false;
                }
                else
                {
                    rigidbodies[i].useGravity = true;
                }
            }
        }
        cg = momentSum / massSum;
    }

    public void UpdateCoGMarker()
    {
        centreOfGravityMarker.transform.position = cg;
        centreOfGravityMarker.GetComponent<Rigidbody>().mass = massSum;
        centreOfGravityMarker.GetComponent<Rigidbody>().centerOfMass = cg;
    }
}