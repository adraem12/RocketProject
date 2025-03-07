using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleThruster : MonoBehaviour
{
    Rigidbody rb;
    public float thrust = 50;
    public float onTime = 0.5f; //time to turn thruster on
    public float offTime = 1f; //time to turn thruster off
    Transform ps; //thruster particle system
    ConfigurableJoint[] cj;
    public Vector3 weight;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //ps = transform.Find("Particle System");
        //ps.gameObject.SetActive(false);
         cj = GetComponents<ConfigurableJoint>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        weight = cj[1].currentForce;
        
        if (Time.time > onTime && Time.time < offTime)
        {
            //ps.gameObject.SetActive(true);
            rb.AddRelativeForce(new Vector3(0, thrust, 0));
        }
        else
        {
           // ps.gameObject.SetActive(false);
        }
    }
}