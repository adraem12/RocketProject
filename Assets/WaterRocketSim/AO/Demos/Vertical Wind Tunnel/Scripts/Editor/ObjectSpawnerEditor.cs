using UnityEngine;
using UnityEditor;

namespace AerodynamicObjects.Demos
{
    [CustomEditor(typeof(AeroObjectSpawner))]
    public class ObjectSpawnerEditor : Editor
    {
        readonly string summary = "Drop random shaped objects into a vertical wind tunnel. Change the wind speed and see what happens.  Wind tunnel is built using a custom Fluid Zone that produces a local vertical wind that drops off with height and has entrainment round the edges. Objects are randomised in terms of shape and centre of mass location.";

        public override void OnInspectorGUI()
        {
            GUIStyle textStyle = EditorStyles.label;
            textStyle.wordWrap = true;
            EditorGUILayout.LabelField(summary, textStyle);

            DrawDefaultInspector();
        }
    }
}