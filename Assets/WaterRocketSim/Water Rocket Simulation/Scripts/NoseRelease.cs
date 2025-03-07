using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoseRelease : MonoBehaviour
    //This script models a temporary concentric joint between the nose cone and the rocket body that keeps both objects axially aligned for a given
    //overlap distance. This keeps the nose cone from coming off sideways until it at least partially separated axially from the body. Some sort of concentric
    // joint on the nose cone is essential practically - otherwise the nose cone can't react any lateral forces and will fall off almost immediately
    // in a normal launch.
{
    public bool fixNoseconeToBody = false; // The option to keep the nose cone permanently attached may be used when testing in 'Wind tunnel mode'
    // or in other situations where you dont want the parachute to deploy, e.g. when you just want to test the effect of different fin size and placement.
    bool jointDestroyed=false; // The joint is destroyed (removed from the simulation) after its job is done as the nose cone never goes back on again
    // until the simulation is reset.
    ConfigurableJoint cj; //Reference to the configurable joint 
    Transform noseCone;// reference to the nose cone
    float startDistance; // variable to note the position of the nose relative to the body at launch. Overlap distance is measured relative to this
    public float overlap=0.05f; // distance nose cone overlaps body as concentric mate. AFter this distance the nosecone will separate comp,etey
    // 
    void Start()
    {
        cj = GetComponent<ConfigurableJoint>(); // get reference to the joint
        if (fixNoseconeToBody == true)
        {
            cj.yMotion = ConfigurableJointMotion.Locked; // stop nose cone from coming away from body
        }
        noseCone = transform.parent.Find("Nose").transform; // get reference to nose cone
        startDistance = (noseCone.position - transform.position).magnitude; // get distance between nose cone and body at start
        
    }

   void FixedUpdate()
    {
        // Check how far the nose cone has come off each simulation timestep. If its bigger than the user defined overalap then
        //destroy the joint and allow the nose cone to move in 6 degrees of freedom. The nose cone is still constrained by a separate 'string' 
        // joint that keeps it with a set distance of the body attachment point.
        if ((noseCone.position- transform.position).magnitude> startDistance+overlap && !jointDestroyed) 
        {
            Destroy(cj); // remove the joint
            print("Nose cone  slider joint deactivated"); // used to tell the user this event has happened
            jointDestroyed = true; // set flag to true so the 'if' statement above becomes false and we never enter this code again.
            
        }
    }
}
