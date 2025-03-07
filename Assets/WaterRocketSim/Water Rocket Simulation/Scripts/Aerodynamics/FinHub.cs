using AerodynamicObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinHub : MonoBehaviour
{
    public Rigidbody rocketRigidBody;
    //public float finMaterialDensity = 40; // density in kg/m^3. Default of 40kg/m^3 for depron foam
    public float finThickness = 0.003f; // Material thickness of fins in m. Default of 3mm set based on standard depron
    public float finArea = 0.0025f; // area of one fin, m^2
    public float finAspectRatio = 2;
    public float finSweepAngle = 45; // leading edge sweep in degrees
    public float finOffset = 0.07f; //offset radius for fin from bottle centreline
    public float hubVerticalOffset = 0;
    
    

    public void UpdateFins()
    {
        Transform[] fins = transform.GetComponentsInChildren<Transform>().Where(t => t.name == "FinPrefab").ToArray();
        for (int i = 0; i < fins.Length; i++)
        {
            Transform scaleandRotation = fins[i].transform.Find("Scale and rotation");
            float b = Mathf.Sqrt(finAspectRatio * finArea);
            float c = b / finAspectRatio;
            scaleandRotation.localPosition = new Vector3(finOffset, hubVerticalOffset, 0); ;
            scaleandRotation.localRotation = Quaternion.Euler(0, 0, -finSweepAngle);
            scaleandRotation.localScale = new Vector3(b, c, finThickness);
            fins[i].GetComponentInChildren<AeroObject>().rb = rocketRigidBody;
        }

    }


   
}
