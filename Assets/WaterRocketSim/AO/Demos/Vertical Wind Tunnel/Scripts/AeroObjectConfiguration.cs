using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AerodynamicObjects.Demos
{
    public class AeroObjectConfiguration : MonoBehaviour
    {
        public float minDimension, maxDimension, minMass, maxMass, maxCamber;
        AeroObject aeroObject;

        public void Initialise()
        {
            aeroObject = GetComponentInChildren<AeroObject>();
            Transform aeroObjectTransform = aeroObject.transform;
            Transform geometryTransform = aeroObjectTransform.parent;
            geometryTransform.localScale = new Vector3(Random.Range(minDimension, maxDimension), Random.Range(minDimension, maxDimension) / Random.Range(2, 20f), Random.Range(minDimension, maxDimension));
            GetComponent<Rigidbody>().centerOfMass = Vector3.Scale(Random.insideUnitCircle, geometryTransform.localScale / 2);
            GetComponent<Rigidbody>().mass = Random.Range(minMass, maxMass);
            GetComponentInChildren<AeroObject>().ao.BodyCamber = Random.Range(0, maxCamber);

            transform.Find("CentreOfGravity").localPosition = GetComponent<Rigidbody>().centerOfMass;
        }
    }
}