using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AerodynamicObjects
{
    public class CreateObjectsTools
    {
        static readonly Vector3 objectStartScale = new Vector3(10f, 10f, 10f);

        //[MenuItem("GameObject/Aerodynamic Objects/Aerodynamic Object")]
        //static void CreateAeroObject()
        //{
        //    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    go.name = "Aero Object";
        //    go.AddComponent<AeroObject>();
        //    Selection.activeObject = go;
        //}

        //[MenuItem("GameObject/Aerodynamic Objects/Aerodynamic Object (with rigidbody)")]
        //static void CreateRigidAeroObject()
        //{
        //    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    go.name = "Aero Object";
        //    Rigidbody rb = go.AddComponent<Rigidbody>();
        //    rb.angularDrag = 0;
        //    go.AddComponent<AeroObject>().rb = rb;

        //    Selection.activeObject = go;
        //}

        [MenuItem("GameObject/Aerodynamic Objects/Fluid Zone")]
        static void CreateFluidZone()
        {
            GameObject go = new GameObject("Fluid Zone");
            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            //collider.size = objectStartScale;
            go.transform.localScale = objectStartScale;
            go.AddComponent<DefaultFluidZone>();

            SetGameObjectParent(go);
            Undo.RegisterCreatedObjectUndo(go, "Create Fluid Zone");
        }

        [MenuItem("GameObject/Aerodynamic Objects/Global Fluid Settings")]
        static void CreateGlobalFluidSettings()
        {
            GameObject go = new GameObject("Global Fluid Settings");
            go.AddComponent<GlobalFluid>();

            SetGameObjectParent(go);
            EditorUtility.SetDirty(go);
        }

        [MenuItem("GameObject/Aerodynamic Objects/Flow Point Particles")]
        static void CreateFlowPointParticles()
        {
            GameObject go = new GameObject("Flow Point Particles");
            go.AddComponent<FlowPointParticles>();

            // Set up the particle system
            ParticleSystem particles = go.AddComponent<ParticleSystem>();
            ParticleSystem.EmissionModule particleEmission = particles.emission;
            particleEmission.rateOverDistance = 0;
            particleEmission.rateOverTime = 100;
            ParticleSystem.MainModule main = particles.main;
            main.startSpeed = 0;
            main.startSize = 0.02f;
            main.startLifetime = 2;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 10000;

            // Remove the shape module
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = false;

            // Add a size over lifetime curve
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) }));

            // Add trails
            ParticleSystem.TrailModule trailModule = particles.trails;
            trailModule.enabled = true;
            trailModule.mode = ParticleSystemTrailMode.Ribbon;
            trailModule.colorOverLifetime = new ParticleSystem.MinMaxGradient { color = new Color(1, 1, 1, 0.2f) };
            trailModule.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.9f, 1), new Keyframe(1, 0) }));

            // Set the materials
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            particles.GetComponent<ParticleSystemRenderer>().trailMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");

            // Add the collider, and rigid body so the system responds to fluid zones
            go.AddComponent<SphereCollider>().isTrigger = true;
            go.AddComponent<Rigidbody>().isKinematic = true;

            SetGameObjectParent(go);
            Undo.RegisterCreatedObjectUndo(go, "Create Flow Point Particles");
        }

        [MenuItem("GameObject/Aerodynamic Objects/Flow Field Particles")]
        static void CreateFlowFieldParticles()
        {
            GameObject go = new GameObject("Flow Field Particles");

            // Need to add width over trail curve so the trails aren't the same size

            go.AddComponent<FlowFieldParticles>();

            ParticleSystem particles = go.AddComponent<ParticleSystem>();
            ParticleSystem.EmissionModule particleEmission = particles.emission;
            particleEmission.rateOverDistance = 0;
            particleEmission.rateOverTime = 100;
            ParticleSystem.MainModule main = particles.main;
            main.startSpeed = 0;
            main.startSize = 0.1f;
            main.maxParticles = 10000;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            particles.GetComponent<ParticleSystemRenderer>().trailMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;

            // Create the walls for the particle system
            GameObject east = new GameObject("East");
            east.transform.position = go.transform.position + new Vector3(0.5f, 0, 0);
            east.transform.up = Vector3.left;
            east.transform.parent = go.transform;

            GameObject west = new GameObject("West");
            west.transform.position = go.transform.position + new Vector3(-0.5f, 0, 0); ;
            west.transform.up = Vector3.right;
            west.transform.parent = go.transform;

            GameObject north = new GameObject("North");
            north.transform.position = go.transform.position + new Vector3(0, 0, 0.5f);
            north.transform.up = Vector3.back;
            north.transform.parent = go.transform;

            GameObject south = new GameObject("South");
            south.transform.position = go.transform.position + new Vector3(0, 0, -0.5f);
            south.transform.up = Vector3.forward;
            south.transform.parent = go.transform;

            GameObject top = new GameObject("Top");
            top.transform.position = go.transform.position + new Vector3(0, 0.5f, 0);
            top.transform.up = Vector3.down;
            top.transform.parent = go.transform;

            GameObject bottom = new GameObject("Bottom");
            bottom.transform.position = go.transform.position + new Vector3(0, -0.5f, 0);
            bottom.transform.up = Vector3.up;
            bottom.transform.parent = go.transform;

            ParticleSystem.CollisionModule collisionModule = particles.collision;
            collisionModule.enabled = true;
            collisionModule.dampen = 1;
            collisionModule.bounce = 0;
            collisionModule.lifetimeLoss = 0;
            collisionModule.AddPlane(east.transform);
            collisionModule.AddPlane(west.transform);
            collisionModule.AddPlane(north.transform);
            collisionModule.AddPlane(south.transform);
            collisionModule.AddPlane(top.transform);
            collisionModule.AddPlane(bottom.transform);


            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.15f, 1), new Keyframe(1, 0) }));

            // Add trails
            ParticleSystem.TrailModule trailModule = particles.trails;
            trailModule.enabled = true;
            trailModule.mode = ParticleSystemTrailMode.PerParticle;
            trailModule.colorOverLifetime = new ParticleSystem.MinMaxGradient { color = new Color(1, 1, 1, 0.2f) };
            trailModule.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0) }));

            go.transform.localScale = objectStartScale;

            SetGameObjectParent(go);
            Undo.RegisterCreatedObjectUndo(go, "Create Flow Field Particles");
        }

        [MenuItem("GameObject/Aerodynamic Objects/Flow Field Arrows")]
        static void CreateFlowArrows()
        {
            GameObject go = new GameObject("Flow Field Arrows");
            go.transform.localScale = objectStartScale;
            go.AddComponent<FlowFieldArrows>();

            SetGameObjectParent(go);
            Undo.RegisterCreatedObjectUndo(go, "Create Flow Field Arrows");
        }

        /// <summary>
        /// Sets the parent of the game object to the current selection. If there is no selection then it will be
        /// create in the root of the scene. If the context is a prefab scene then it is created as a child of the
        /// prefab root. Also sets the current selection to the game object in question.
        /// </summary>
        /// <param name="go"></param>
        public static void SetGameObjectParent(GameObject go)
        {
            // Create the object as a child if the context is appropriate
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                // If we're in a prefab scene we either child the new object to the selection in the scene
                if (Selection.activeGameObject != null)
                {
                    go.transform.parent = Selection.activeGameObject.transform;
                }
                else
                {
                    // Or we use the root of the prefab as the parent
                    go.transform.parent = prefabStage.prefabContentsRoot.transform;
                }
            }
            else if (Selection.activeGameObject != null)
            {
                go.transform.parent = Selection.activeGameObject.transform;
            }

            go.transform.position = SceneView.lastActiveSceneView.pivot;
            Selection.activeObject = go;
        }
    }
}