using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FinHub))]
public class HubEditor : Editor
{
    FinHub hub;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        hub = (FinHub)target;
        if (GUILayout.Button("Update Fins"))
        {
            hub.UpdateFins();
        }
    } 
}
