using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimulationConfig))]
public class ConfigurationEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Update Configuration"))
        {
            SimulationConfig simConfig = (SimulationConfig)target;
            simConfig.Setup();

        }
    }
}