using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCam : MonoBehaviour
{
    public float radius, angularSpeed, radialSpeed, radialTimeDelay;
    public float angleOffset;
    public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        var cam=GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float angle = -Time.time * angularSpeed+angleOffset;
        if (Time.time>radialTimeDelay) radius += Time.time * radialSpeed;
        float x = radius * Mathf.Sin(angle);
        float z = radius * Mathf.Cos(angle);
        transform.position = new Vector3(x+target.position.x,target.position.y+1f, z + target.position.z);
        transform.LookAt(target);
    }
}
