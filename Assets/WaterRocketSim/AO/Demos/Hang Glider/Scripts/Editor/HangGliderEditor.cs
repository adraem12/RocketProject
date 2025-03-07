using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AerodynamicObjects.Demos
{
    [CustomEditor(typeof(HangGlider))]
    public class HangGliderEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            Texture banner = Resources.Load("Hang Glider graphic") as Texture;
            GUILayout.Box(banner, GUILayout.ExpandWidth(true));
            DrawDefaultInspector();

        }

    }
}