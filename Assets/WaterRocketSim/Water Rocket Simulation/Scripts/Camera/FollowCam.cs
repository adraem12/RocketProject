using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform objectToLookAt;
    Vector3 forward;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        forward = objectToLookAt.position - transform.position;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}
