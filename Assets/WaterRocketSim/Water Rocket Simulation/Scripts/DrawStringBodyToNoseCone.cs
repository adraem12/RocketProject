using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawStringBodyToNoseCone : MonoBehaviour
{
    Transform starttr, endtr;
    LineRenderer lr;
    
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        starttr = GameObject.Find("Nose String Body Attachment").GetComponent<Transform>();
        endtr = GameObject.Find("Nose String Nose Attachment").GetComponent<Transform>();
        
    }

    // Update is called once per frame
    void Update()
    {
        lr.SetPosition(0, starttr.position);
        lr.SetPosition(1, endtr.position );
    }
}
