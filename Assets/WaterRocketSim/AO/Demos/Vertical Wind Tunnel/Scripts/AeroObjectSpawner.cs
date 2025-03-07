using UnityEngine;

namespace AerodynamicObjects.Demos
{
    public class AeroObjectSpawner : MonoBehaviour
    {
        public GameObject aeroObject;
        public float minDimension, maxDimension, minMass, maxMass, maxCamber, spawnRadius;
        public Transform spawnCentre;

        void Start()
        {
            // Default max angular speed is too low for aerodynamics
            // This can be set to infinity providing that objects have rotational damping turned on
            Physics.defaultMaxAngularSpeed = 100;
        }

        public void SpawnAeroObject()
        {
            GameObject go = Instantiate(aeroObject, Random.insideUnitSphere * spawnRadius + spawnCentre.position, Random.rotation);
            go.name = "Aero Object";

            AeroObjectConfiguration aeroObjectConfiguration = go.GetComponent<AeroObjectConfiguration>();
            aeroObjectConfiguration.minDimension = minDimension;
            aeroObjectConfiguration.maxDimension = maxDimension;
            aeroObjectConfiguration.minMass = minMass;
            aeroObjectConfiguration.maxMass = maxMass;
            aeroObjectConfiguration.maxCamber = maxCamber;
            aeroObjectConfiguration.Initialise();
        }
    }
}