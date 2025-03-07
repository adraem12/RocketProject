using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AerodynamicObjects.Demos
{
    public class GliderSpawner : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject planePF;
        public float spawnRadius, launchSpeed, spawnRate;
        public int totalPlanes, planeCount;
        public Transform spawnTarget;
        float time;
        public GroundViewCamera groundCamera;
        public OrbitalCamera orbitalCamera;
        public ChaseCamera chaseCamera;
        GameObject go;
        Vector3 spawnPosition;
        float theta;

        // Start is called before the first frame update
        void Start()
        {
            Time.fixedDeltaTime = 0.01f;
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;
            if (time > (1 / spawnRate) & planeCount < totalPlanes)
            {
                //print("spawned");
                theta = Random.Range(0, 2f * Mathf.PI);
                spawnPosition = new Vector3(spawnRadius * Mathf.Sin(theta), 0f, spawnRadius * Mathf.Cos(theta));
                theta += Mathf.PI / 2;
                go = Instantiate(planePF, spawnTarget.position + spawnPosition, new Quaternion(0, Mathf.Sin(theta/2f), 0, Mathf.Cos(theta/2f)));
                go.GetComponentInChildren<Rigidbody>().velocity = launchSpeed * go.transform.forward;
                time = 0;
                planeCount++;
            }
            //if (planeCount == 1)
            //{
            //    groundCamera.target = orbitalCamera.target = chaseCamera.target = go.transform;

            //}
        }
    }
}