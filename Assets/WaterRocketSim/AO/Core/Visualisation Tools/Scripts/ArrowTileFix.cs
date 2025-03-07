using UnityEngine;

namespace AerodynamicObjects
{
    public class ArrowTileFix : MonoBehaviour
    {
        public float scale = 1f;

        Material material;

        private void Start()
        {
            material = GetComponent<MeshRenderer>().material;
        }

        // Update is called once per frame
        void Update()
        {
            material.mainTextureScale = new Vector2(1f, scale * transform.lossyScale.y);
        }
    }
}