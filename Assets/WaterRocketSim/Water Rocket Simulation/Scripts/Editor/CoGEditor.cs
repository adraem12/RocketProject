using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CalculateCoG))]
public class CoGEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Calculate Centre of Gravity"))
        {
            CalculateCoG rigger = (CalculateCoG)target;
            rigger.GetRigidBodies();
            rigger.FindCoG();
            rigger.UpdateCoGMarker();
        }
    }
}
