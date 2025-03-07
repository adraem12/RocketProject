using System.Collections.Generic;
using UnityEngine;

namespace AerodynamicObjects
{
    public class AngleSegment : MonoBehaviour
    {
        //MeshFilter mf;
        [Range(-180, 180)]
        public float includedAngleDeg;
        [Range(1, 5)]
        public float radius = 1;
        [Range(2, 10)]
        public int numberOfPoints = 5;

        public Color colour = new Color(1, 1, 0, 0.5f);

        Vector3[] vertices;
        Mesh mesh;

        Transform rootTransform;
        AeroObject aeroObject;

        void CreateWedgeMesh(Color color)
        {
            // Create a new game object to store the wedge on
            GameObject angleOfAttackWedge = new GameObject("Angle of Attack Wedge");
            angleOfAttackWedge.transform.parent = transform;
            rootTransform = angleOfAttackWedge.transform;
            rootTransform.localPosition = Vector3.zero;
            rootTransform.localRotation = Quaternion.identity;
            rootTransform.localScale = new Vector3(1f / rootTransform.parent.localScale.x, 1f / rootTransform.parent.localScale.y, 1f / rootTransform.parent.localScale.z);

            MeshFilter mf = angleOfAttackWedge.AddComponent<MeshFilter>();
            MeshRenderer mr = angleOfAttackWedge.AddComponent<MeshRenderer>();
            mesh = new Mesh();
            mf.mesh = mesh;
            mr.materials[0].color = color;
            mr.materials[0].ToFadeMode();

            // Add vertices to mesh
            List<Vector3> verticesList = new List<Vector3>();

            float angle = 0;
            // Add zero point
            verticesList.Add(Vector3.zero);
            // Add radial points
            for (int i = 0; i < numberOfPoints; i++)
            {
                angle = ((float)i / (numberOfPoints - 1)) * includedAngleDeg * Mathf.Deg2Rad;
                float z = radius * Mathf.Cos(angle);
                float y = radius * Mathf.Sin(angle);
                verticesList.Add(new Vector3(0, y, z));
            }

            vertices = verticesList.ToArray();


            // Triangles
            List<int> trianglesList = new List<int>();
            // Front face
            for (int i = 0; i < (numberOfPoints - 1); i++)
            {
                trianglesList.Add(0);
                trianglesList.Add(i + 1);
                trianglesList.Add(i + 2);
            }
            // Back face
            for (int i = 0; i < (numberOfPoints - 1); i++)
            {
                trianglesList.Add(i + 2);
                trianglesList.Add(i + 1);
                trianglesList.Add(0);
            }


            int[] triangles = trianglesList.ToArray();

            // Normals
            List<Vector3> normalsList = new List<Vector3>();

            for (int i = 0; i < vertices.Length; i++)
            {
                normalsList.Add(Vector3.forward);
            }

            Vector3[] normals = normalsList.ToArray();

            // Initialise
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
        }

        void SetWedgeAngle(float angle_radians)
        {
            float angle = 0;
            for (int i = 0; i < numberOfPoints; i++)
            {
                angle = -((float)i / (numberOfPoints - 1)) * angle_radians;
                float z = radius * Mathf.Cos(angle);
                float y = radius * Mathf.Sin(angle);
                vertices[i + 1] = new Vector3(0, y, z);
            }

            mesh.vertices = vertices;
        }

        void SetWedgeLocalRotation(Quaternion rotation)
        {
            rootTransform.localRotation = rotation;
        }

        void SetWedgeRotation(Quaternion rotation)
        {
            rootTransform.rotation = rotation;
        }

        // Start is called before the first frame update
        void Awake()
        {
            aeroObject = GetComponent<AeroObject>();
            CreateWedgeMesh(colour);
        }

        // Update is called once per frame
        void Update()
        {
            SetWedgeAngle((float)aeroObject.ao.AngleOfAttack);
            SetWedgeLocalRotation(Quaternion.Euler(new Vector3(0, (float)aeroObject.ao.AngleOfSideslip)));
        }
    }
}