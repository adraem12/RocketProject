using UnityEditor;

namespace AerodynamicObjects
{
    [CustomEditor(typeof(AeroGroup))]
    public class AeroGroupEditor : Editor
    {

        public void OnEnable()
        {
            // Make sure the aero objects array matches the group's array
            //AeroGroup aeroGroup = (AeroGroup)target;
            //aeroGroup.UpdateGroup();
        }

        public override void OnInspectorGUI()
        {
            AeroGroup aeroGroup = (AeroGroup)target;

            if (aeroGroup.group == null)
            {
                aeroGroup.group = new AerodynamicGroup();
            }

            EditorGUI.BeginChangeCheck();

            aeroGroup.group.planformArea = EditorGUILayout.FloatField("Planform Area", (float)aeroGroup.group.planformArea);
            aeroGroup.group.Span = EditorGUILayout.FloatField("Span", (float)aeroGroup.group.Span);

            EditorGUILayout.LabelField("Object area scaling factor", aeroGroup.group.objectAreaScale.ToString());

            // Draw the aero object array        
            var property = serializedObject.FindProperty("aeroObjects");
            //var floatArray = serializedObject.FindProperty("group.objectPlanformAreas");
            serializedObject.Update();
            EditorGUILayout.PropertyField(property, true);
            //EditorGUILayout.PropertyField(floatArray, true);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(aeroGroup);
                aeroGroup.UpdateGroup();
            }
        }
    }
}