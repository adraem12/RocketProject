using UnityEngine;
using UnityEditor;

namespace AerodynamicObjects.Demos
{
    [CustomEditor(typeof(PropellerEngine))]
    public class PropellerEngineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            PropellerEngine propellerEngine = (PropellerEngine)target;
            if (GUILayout.Button("Update propeller geometry")) propellerEngine.UpdatePropellerGeometry();
        }
    }
}