using UnityEngine;
using UnityEditor;

namespace AerodynamicObjects
{
    [CustomEditor(typeof(AeroObject)), CanEditMultipleObjects]
    public class AeroObjectEditor : Editor
    {
        AeroObject ao;

        // This will be the new way to handle multi object editing as well as undo functionality etc
        SerializedProperty hasDrag;
        SerializedProperty hasLift;
        SerializedProperty hasRotationalDamping;
        SerializedProperty hasRotationalLift;
        SerializedProperty hasBuoyancy;
        SerializedProperty isKinematic;
        SerializedProperty camber;
        SerializedProperty rb;
        SerializedProperty dimensions;

        private void OnEnable()
        {
            ao = (AeroObject)target;
            ao.Initialise();
            serializedObject.Update();


            hasDrag = serializedObject.FindProperty("hasDrag");
            hasLift = serializedObject.FindProperty("hasLift");
            hasRotationalDamping = serializedObject.FindProperty("hasRotationalDamping");
            hasRotationalLift = serializedObject.FindProperty("hasRotationalLift");
            hasBuoyancy = serializedObject.FindProperty("hasBuoyancy");
            isKinematic = serializedObject.FindProperty("isKinematic");
            var AO = serializedObject.FindProperty("ao");
            camber = AO.FindPropertyRelative("BodyCamber");

            rb = serializedObject.FindProperty("rb");
            dimensions = serializedObject.FindProperty("localDimensions");
        }

        public override void OnInspectorGUI()
        {
            // ========= BANNER =================
            Texture banner = (Texture)AssetDatabase.LoadAssetAtPath("Assets/AO/Core/Resources/ao logo.png", typeof(Texture));
            GUILayout.Box(banner, GUILayout.ExpandWidth(true));
            // ==================================

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(hasDrag, new GUIContent("Has Drag"));
            EditorGUILayout.PropertyField(hasLift, new GUIContent("Has Lift"));
            EditorGUILayout.PropertyField(hasBuoyancy, new GUIContent("Has Buoyancy"));
            EditorGUILayout.PropertyField(hasRotationalDamping, new GUIContent("Has Rotational Damping"));
            EditorGUILayout.PropertyField(hasRotationalLift, new GUIContent("Has Rotational Lift"));

            if (EditorGUI.EndChangeCheck())
            {
                UpdateModels();
            }

            EditorGUILayout.PropertyField(isKinematic, new GUIContent("Is Kinematic"));
            EditorGUILayout.PropertyField(rb, new GUIContent("Rigid Body"));
            EditorGUILayout.PropertyField(dimensions, new GUIContent("Local Dimensions"));



            //// Debug Values - This is how we can make things visible in the editor
            //// A bit like the "Info" part of a RigidBody component

            //EditorGUILayout.LabelField("Number of Models", ao.ao.AerodynamicModels.Length.ToString());
            //EditorGUILayout.LabelField("AO Rotation", aoResult.ToString());
            //EditorGUILayout.LabelField("Unity Rotation", unityResult.ToString());
            //EditorGUILayout.LabelField("Inverse Body Rotation", ao.ao.BodyFrame.inverseBodyRotation.ToString());
            //EditorGUILayout.LabelField("External Fluid Velocity", ao.ao.FluidVelocity.ToString());
            //EditorGUILayout.LabelField("Global Angular Velocity", ao.ao.GlobalAngularVelocity.ToString());
            //EditorGUILayout.LabelField("Local Angular Velocity", ao.ao.LocalAngularVelocity.ToString());
            //EditorGUILayout.LabelField("Body Angular Velocity", ao.ao.BodyAngularVelocity.ToString());
            //EditorGUILayout.LabelField("Planform Area", ao.ao.PlanformArea.ToString());
            //EditorGUILayout.LabelField("Body Planform Area", ao.ao.BodyPlanformArea.ToString());
            //EditorGUILayout.LabelField("Area scale", ao.ao.AreaScale.ToString());
            //EditorGUILayout.LabelField("Drag Force", ao.ao.AerodynamicForces[ao.GetModelIndex<DragModel>()].vector.ToString());
            //EditorGUILayout.LabelField("Damping torque", ao.ao.AerodynamicLoads[ao.GetModelIndex<RotationalDampingModel>()].moment.ToString());
            //EditorGUILayout.LabelField("Buoyant Force", ao.ao.AerodynamicLoads[ao.GetModelIndex<BuoyancyModel>()].force.ToString());
            //EditorGUILayout.LabelField("Relative Velocity", ao.ao.RelativeVelocity.ToString());
            //EditorGUILayout.LabelField("Body Relative Velocity", ao.ao.BodyFrame.relativeVelocity.ToString());
            //EditorGUILayout.LabelField("Alpha", ao.ao.AngleOfAttack.ToString());
            //EditorGUILayout.LabelField("Alpha Degrees", (Mathf.Rad2Deg * ao.ao.AngleOfAttack).ToString());
            //EditorGUILayout.LabelField("Alpha Vector", ao.ao.AngleOfAttackRotationVector.ToString());
            //EditorGUILayout.LabelField("Beta", ao.ao.AngleOfSideslip.ToString());
            //EditorGUILayout.LabelField("Rho", ao.ao.FluidDensity.ToString());
            //EditorGUILayout.LabelField("CD", ((DragModel)ao.ao.AerodynamicModels[ao.GetModelIndex<DragModel>()]).CD.ToString());
            //EditorGUILayout.LabelField("test", ((RotationalDampingModel)ao.ao.AerodynamicModels[ao.GetModelIndex<RotationalDampingModel>()]).ToString());
            //EditorGUILayout.LabelField("Resultant Force", ao.ao.NetAerodynamicLoad.force.ToString());
            //EditorGUILayout.LabelField("Resultant Moment", ao.ao.NetAerodynamicLoad.moment.ToString());


            EditorGUILayout.PropertyField(camber, new GUIContent("Body Camber"));

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateModels()
        {
            // There MUST be a better way to do this...
            if (hasDrag.boolValue)
            {
                ao.AddModel<DragModel>();
            }
            else
            {
                ao.RemoveModel<DragModel>();
            }

            if (hasLift.boolValue)
            {
                ao.AddModel<LiftModel>();
            }
            else
            {
                ao.RemoveModel<LiftModel>();
            }

            if (hasRotationalDamping.boolValue)
            {
                ao.AddModel<RotationalDampingModel>();
            }
            else
            {
                ao.RemoveModel<RotationalDampingModel>();
            }

            if (hasRotationalLift.boolValue)
            {
                ao.AddModel<RotationalLiftModel>();
            }
            else
            {
                ao.RemoveModel<RotationalLiftModel>();
            }

            if (hasBuoyancy.boolValue)
            {
                ao.AddModel<BuoyancyModel>();
            }
            else
            {
                ao.RemoveModel<BuoyancyModel>();
            }
        }
    }
}