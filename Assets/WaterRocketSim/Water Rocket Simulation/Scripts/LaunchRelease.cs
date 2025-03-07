using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchRelease : MonoBehaviour
{
    Transform body;
    float startDistance;
    bool clearedTheLaunchPlug=false;
    ConfigurableJoint cj;
    // Start is called before the first frame update
    void Start()
    {
        body = transform.parent.Find("Rocket").Find("Body");
        startDistance = (transform.position - body.position).magnitude;
        //print("start distance " + startDistance);
        cj = GetComponent<ConfigurableJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (clearedTheLaunchPlug == false)
        {

            var distance = (transform.position - body.position).magnitude;
            //print(" distance " + distance);
            if (distance -startDistance > 0.02f)
            {
                clearedTheLaunchPlug = true;
                Destroy(cj);
                print("Launch plug slider joint deactivated");

            }
        }
    }
}
