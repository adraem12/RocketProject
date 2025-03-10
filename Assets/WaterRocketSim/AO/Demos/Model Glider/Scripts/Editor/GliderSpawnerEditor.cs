using UnityEngine;
using UnityEditor;

namespace AerodynamicObjects.Demos
{
    [CustomEditor(typeof(GliderSpawner))]
    public class GliderSpawnerEditor : Editor
    {
        readonly string summary = "The Model Glider is a simple aircraft modelled with two Aerodynamic Object panels for the wings and one each for the tailplane and fin. There is no engine. Multiple gliders are spawned over time. Gliders start with a fixed rudder deflection that causes them to turn. Cameras follow the first aircraft spawned � watch the planes spiral upwards. The user can incrementally change the pitch and yaw trim settings of all aircraft via keyboard inputs.";

        public override void OnInspectorGUI()
        {
            Texture banner = (Texture)Resources.Load("Model glider graphic", typeof(Texture));
            GUILayout.Box(banner, GUILayout.ExpandWidth(true));

            GUIStyle textStyle = EditorStyles.label;
            textStyle.wordWrap = true;
            EditorGUILayout.LabelField(summary, textStyle);

            DrawDefaultInspector();
        }
    }
}