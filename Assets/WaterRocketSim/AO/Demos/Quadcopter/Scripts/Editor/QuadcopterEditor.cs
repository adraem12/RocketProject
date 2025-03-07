using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AerodynamicObjects.Demos
{
    [CustomEditor(typeof(Quadcopter))]
    public class QuadcopterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Texture banner = Resources.Load("Quadcopter graphic") as Texture;
            GUILayout.Box(banner, GUILayout.ExpandWidth(true));
            DrawDefaultInspector();

        }

    }
}