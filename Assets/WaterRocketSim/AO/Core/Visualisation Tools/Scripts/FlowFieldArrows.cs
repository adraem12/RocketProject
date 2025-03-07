using System;
using UnityEngine;

namespace AerodynamicObjects
{
    public class FlowFieldArrows : MonoBehaviour
    {
        [Tooltip("The number of arrows in each axis.")]
        public Vector3Int numArrows = new Vector3Int(3, 3, 3);

        [Tooltip("The size of the arrows relative to this object's transform")]
        public float arrowSize = 1f;

        [Tooltip("The scaling of the arrow length with wind speed.")]
        public float windSpeedScale = 1f;

        [Tooltip("Arrows will not be drawn for wind speeds below this threshold. (m/s)")]
        public float thresholdWindSpeed = 0.1f;

        [Tooltip("Controls how fast arrows fade with distance from visual centre. Used to declutter and focus attention on object of interest within wind field. A higher value increases the number of arrows visible")]
        [Range(0, 1)]
        public float visualRange = 0.5f;

        [Tooltip("Visual centre of arrow field. Usually the transfrom of an object of interest. If left unassigned, this object's transform is used")]
        public Transform focusPoint;

        [Tooltip("The fluid zones to draw arrows for.")]
        public FluidZone[] fluidZones;

        private Transform[] arrowTransforms;
        private Material[] arrowMaterials;
        private Vector3Int previousArrowNumbers;
        private Color arrowColour;

        private void OnValidate()
        {
            thresholdWindSpeed = Mathf.Max(0, thresholdWindSpeed);
            arrowSize = Mathf.Max(0, arrowSize);
            windSpeedScale = Mathf.Max(0, windSpeedScale);

            // Make sure we have a positive number of arrows being created
            if (numArrows.x < 0)
                numArrows.x = 0;

            if (numArrows.y < 0)
                numArrows.y = 0;

            if (numArrows.z < 0)
                numArrows.z = 0;
        }

        // Start is called before the first frame update
        void Start()
        {
            CreateArrows();
        }

        private void OnEnable()
        {
            if (arrowTransforms != null)
            {
                for (int i = 0; i < arrowTransforms.Length; i++)
                {
                    arrowTransforms[i].gameObject.SetActive(true);
                }
            }
        }

        private void OnDisable()
        {
            if (arrowTransforms != null)
            {
                for (int i = 0; i < arrowTransforms.Length; i++)
                {
                    arrowTransforms[i].gameObject.SetActive(false);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (previousArrowNumbers != numArrows)
            {
                DestroyArrows();
                CreateArrows();
            }

            Vector3 position;
            Vector3 velocity;
            float range;
            float scaleMagnitude = transform.lossyScale.magnitude;

            // If we don't have a focus point then we will centre the arrow's fade on the manager's position
            Vector3 focalPosition = (focusPoint) ? focusPoint.position : transform.position;

            for (int i = 0; i < arrowTransforms.Length; i++)
            {
                position = arrowTransforms[i].position;
                velocity = Vector3.zero;

                // Get the net velocity for the arrow based on all the fluid zones
                for (int j = 0; j < fluidZones.Length; j++)
                {
                    if (fluidZones[j].IsPositionInsideZone(position))
                    {
                        velocity += fluidZones[j].VelocityFunction(position);
                    }
                }

                // If the speed is below the threshold make the arrow invisible for now
                if (velocity.magnitude < thresholdWindSpeed)
                {
                    arrowColour.a = 0f;
                    arrowMaterials[i].color = arrowColour;
                    continue;
                }

                // Set the alpha of the arrow based on its distance from the focus point
                range = (focalPosition - position).magnitude / scaleMagnitude;
                arrowColour.a = 0.5f * Mathf.Exp(-Mathf.Pow(range / visualRange, 2));
                arrowMaterials[i].color = arrowColour;

                // Align the arrow with the wind and set its size based on the wind speed
                arrowTransforms[i].forward = velocity.normalized;
                arrowTransforms[i].localScale = new Vector3(arrowSize / scaleMagnitude, arrowSize / scaleMagnitude, velocity.magnitude * windSpeedScale / scaleMagnitude);
            }
        }

        private void DestroyArrows()
        {
            for (int i = 0; i < arrowTransforms.Length; i++)
            {
                Destroy(arrowTransforms[i].gameObject);
            }
        }

        void CreateArrows()
        {
            // Make sure we have a positive number of arrows being created
            if (numArrows.x < 0)
                numArrows.x = 0;

            if (numArrows.y < 0)
                numArrows.y = 0;

            if (numArrows.z < 0)
                numArrows.z = 0;

            previousArrowNumbers = numArrows;

            arrowTransforms = new Transform[numArrows.x * numArrows.y * numArrows.z];
            arrowMaterials = new Material[numArrows.x * numArrows.y * numArrows.z];

            GameObject arrow = Resources.Load("Wind Field Arrow") as GameObject;

            arrowColour = arrow.GetComponent<Renderer>().sharedMaterial.color;

            float xspacing = (numArrows.x == 1) ? 0 : transform.lossyScale.x / (float)(numArrows.x);
            float yspacing = (numArrows.y == 1) ? 0 : transform.lossyScale.y / (float)(numArrows.y);
            float zspacing = (numArrows.z == 1) ? 0 : transform.lossyScale.z / (float)(numArrows.z);

            int count = 0;

            float xStart = (numArrows.x == 1) ? 0 : (xspacing/2f) - transform.lossyScale.x / 2f;
            float yStart = (numArrows.y == 1) ? 0 : (yspacing/2f) - transform.lossyScale.y / 2f;
            float zStart = (numArrows.z == 1) ? 0 : (zspacing/2f) - transform.lossyScale.z / 2f;

            for (int x = 0; x < numArrows.x; x++)
            {
                for (int y = 0; y < numArrows.y; y++)
                {
                    for (int z = 0; z < numArrows.z; z++)
                    {
                        Vector3 position = transform.position + new Vector3(xStart + xspacing * x, yStart + yspacing * y, zStart + zspacing * z);
                        GameObject go = Instantiate(arrow, position, Quaternion.identity);
                        go.transform.localScale = arrowSize * Vector3.one;
                        go.transform.parent = transform;

                        arrowTransforms[count] = go.transform;
                        arrowMaterials[count] = go.GetComponent<Renderer>().material;
                        count++;
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }
    }
}