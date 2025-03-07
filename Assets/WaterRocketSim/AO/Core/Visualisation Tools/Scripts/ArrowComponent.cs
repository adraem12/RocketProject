using UnityEngine;

namespace AerodynamicObjects
{
    // Adding this so that the arrows will use the up to date values from the aerobody without having race conditions
    [DefaultExecutionOrder(100)]
    public class ArrowComponent : MonoBehaviour
    {
        // This script will be responsible for adding arrows to the various areodynamic components
        // each arrow will need to be properly scaled, coloured and faded

        // Scale, sensitivity and offset should be up here but also need to be accessible to the Arrow class
        // and I can't be bothered just passing those values in every time, maybe the functions for arrows
        // should be out here too and the Arrow class can just be a data structure.

        [Tooltip("Width of the arrow. (m)")]
        public float Diameter = 0.2f;

        [Tooltip("Scaling of the arrow length with the value it represents. A larger sensitivity will give longer arrows.")]
        public float Sensitivity = 1f;

        [Tooltip("Distance between the arrow head and the point of action")]
        public float Offset = 0f;

        [Tooltip("The fraction of the arrow's length that is used to draw the head of the arrow.")]
        public float HeadFractionOfLength = 0.25f;

        [Tooltip("If the arrow would be smaller than this length then it will not be drawn.")]
        public float MinimumLength = 0.01f;

        [Tooltip("Does the arrow point towards the point of action or away from it")]
        public bool HeadAimsAtPoint;

        [Tooltip("Use the coefficient for the aerodynamic force to scale the length of the arrow?" +
            "If false then the force is used to scale the arrow length. Wind arrows will be normalised to a direction vector is this is true.")]
        public bool UseCoefficientForScale;

        public static GameObject ArrowContainer
        {
            get
            {
                if (arrowContainer == null)
                {
                    arrowContainer = new GameObject("Force Arrows");
                    arrowContainer.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
                return arrowContainer;
            }
        }

        private static GameObject arrowContainer;


        public AeroObject TryGetAeroObject()
        {
            AeroObject aeroObject;
            // Make sure we have access to the aero body. This can be made into a public field
            // for reference, though it makes sense to have the arrows on the body
            if (!TryGetComponent<AeroObject>(out aeroObject))
            {
                Debug.LogError("No Aerobody component found on " + gameObject.name);
            }
            return aeroObject;
        }

        public void SetArrowPositionAndRotation(Arrow arrow, float length, Vector3 rootPosition, Vector3 direction)
        {
            length *= Sensitivity;

            if (length <= MinimumLength)
            {
                arrow.SetVisible(false);
                return;
            }

            arrow.SetVisible(true);

            // Direction MUST BE NORMALISED
            direction.Normalize();

            if (HeadAimsAtPoint)
            {
                rootPosition -= length * direction;
            }

            // All the offsetting is handled in here so the higher up scripts just need to specify the point of action for the arrow
            arrow.body.position = rootPosition + Offset * direction;
            arrow.body.up = direction;

            // This might become a fixed value rather than scaling the head proportionally like this
            arrow.body.localScale = new Vector3(Diameter, (1f - HeadFractionOfLength) * length, Diameter);

            arrow.head.position = arrow.body.position + direction * ((1f - HeadFractionOfLength) * length);
            arrow.head.up = direction;
            // This might become a fixed value rather than scaling the head proportionally like this
            arrow.head.localScale = new Vector3(2 * Diameter, HeadFractionOfLength * length, 2 * Diameter);

        }

        public void SetArrowPositionAndRotationFromVector(Arrow arrow, Vector3 vector, Vector3 rootPosition)
        {
            // This is just lazy so that we can pass in the initial vector and get the length and direction here instead
            // of having to do it in every function in the higher upss

            float vectorMagnitude = vector.magnitude;
            if (vectorMagnitude <= MinimumLength)
            {
                arrow.SetVisible(false);
                return;
            }

            arrow.SetVisible(true);

            Vector3 direction = vector.normalized;
            float length = vectorMagnitude * Sensitivity;

            if (HeadAimsAtPoint)
            {
                rootPosition -= length * direction;
            }

            rootPosition += Offset * direction;

            // All the offsetting is handled in here so the higher up scripts just need to specify the point of action for the arrow
            arrow.body.position = rootPosition;
            arrow.body.up = direction;

            // This might become a fixed value rather than scaling the head proportionally like this
            arrow.body.localScale = new Vector3(Diameter, (1f - HeadFractionOfLength) * length, Diameter);

            arrow.head.position = arrow.body.position + direction * ((1f - HeadFractionOfLength) * length);
            arrow.head.up = direction;
            // This might become a fixed value rather than scaling the head proportionally like this
            arrow.head.localScale = new Vector3(2 * Diameter, HeadFractionOfLength * length, 2 * Diameter);
        }




        public class Arrow
        {
            // Need this class because a component can have more than one arrow attached to it.

            public Transform body;
            public Transform head;

            private void CreateArrow()
            {
                CreateHeadAndBody();
                GameObject root = CreateRoot();
                SetLayerID(root);
            }

            private void CreateArrow(string name)
            {
                CreateHeadAndBody();
                GameObject root = CreateRoot();
                root.name = name;
                SetLayerID(root);
            }

            public Arrow()
            {
                CreateArrow();
            }

            public Arrow(Color color)
            {
                CreateArrow();
                SetMaterialColour(color);
            }

            public Arrow(Color color, string name)
            {
                CreateArrow(name);

                body.name = "Arrow Body";
                head.name = "Arrow Head";

                SetMaterialColour(color);
            }

            public Arrow(Color color, string name, Texture2D texture)
            {
                CreateArrow(name);

                body.name = "Arrow Body";
                head.name = "Arrow Head";

                SetMaterialColour(color);
                SetTexture(texture);
            }

            private void CreateHeadAndBody()
            {
                GameObject bodyGO = Resources.Load("Arrow Body") as GameObject;
                GameObject headGO = Resources.Load("Arrow Head") as GameObject;
                body = Instantiate(bodyGO).transform;
                head = Instantiate(headGO).transform;

                Material material = Resources.Load("Arrow Material") as Material;
                body.GetChild(0).GetComponent<MeshRenderer>().materials[0] = material;
                head.GetChild(0).GetComponent<MeshRenderer>().materials[0] = material;

                // At Bill's request, arrows will not cast shadows
                body.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                head.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            }

            private GameObject CreateRoot()
            {
                // Create a root gameobject which holds both parts of the arrow, makes it neater in scene hierarchy
                GameObject arrow = new GameObject("Arrow Root");
                Transform arrowT = arrow.transform;

                arrowT.SetParent(ArrowContainer.transform);

                body.SetParent(arrowT);
                head.SetParent(arrowT);

                return arrow;
            }

            private void SetLayerID(GameObject root)
            {
                int layerID = LayerMask.NameToLayer("Arrows");
                if (layerID == -1)
                {
                    Debug.LogError("No layer for Arrows was found. Please add a new layer named: Arrows");
                }

                SetLayerRecursively(root, layerID);
            }


            private void SetTexture(Texture2D texture)
            {
                body.GetChild(0).GetComponent<MeshRenderer>().materials[0].mainTexture = texture;
                head.GetChild(0).GetComponent<MeshRenderer>().materials[0].mainTexture = texture;

                // Add the tiling fix script so the texture doesn't stretch
                body.GetChild(0).gameObject.AddComponent<ArrowTileFix>();
                head.GetChild(0).gameObject.AddComponent<ArrowTileFix>();
            }

            private void SetMaterialColour(Color color)
            {
                // Set colour and shader to fade
                body.GetChild(0).GetComponent<MeshRenderer>().materials[0].color = color;
                head.GetChild(0).GetComponent<MeshRenderer>().materials[0].color = color;
            }

            private void SetLayerRecursively(GameObject obj, int layer)
            {
                obj.layer = layer;

                foreach (Transform child in obj.transform)
                {
                    SetLayerRecursively(child.gameObject, layer);
                }
            }

            public void SetVisible(bool isActive)
            {
                body.gameObject.SetActive(isActive);
                head.gameObject.SetActive(isActive);
            }
        }
    }
}