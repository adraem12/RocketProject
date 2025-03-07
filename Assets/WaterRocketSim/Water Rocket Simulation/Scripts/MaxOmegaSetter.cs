using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxOmegaSetter : MonoBehaviour
{
    // This script is used to set the max angular velocity of all rigid bodies in the scene
    [Tooltip("Max angular speed of a rigid body. (rad/s)")]
    public int maxAngularSpeed = 10000;

    // Start is called before the first frame update
    void Start()
    {
        Physics.defaultMaxAngularSpeed = maxAngularSpeed;

        Rigidbody[] rigidbodies = FindObjectsOfType<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].maxAngularVelocity = maxAngularSpeed;
        }
    }
}
