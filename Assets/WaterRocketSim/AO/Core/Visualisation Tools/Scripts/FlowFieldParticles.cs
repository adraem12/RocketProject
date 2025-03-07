using UnityEngine;

namespace AerodynamicObjects
{
    public class FlowFieldParticles : MonoBehaviour
    {
        public bool enableParticleTrails = true;
        public float particleSpawnRate = 100;
        public float particleSize = 0.1f;
        public float particleLife = 3f;

        public FluidZone[] fluidZones;

        ParticleSystem m_ParticleSystem;
        ParticleSystem.TrailModule trailModule;
        ParticleSystem.MainModule mainModule;
        ParticleSystem.EmissionModule emissionModule;
        ParticleSystem.CollisionModule collisionModule;
        ParticleSystemRenderer particleSystemRenderer;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1];
        int nParticles;

        private void OnValidate()
        {
            particleSpawnRate = Mathf.Max(0, particleSpawnRate);
            particleSize = Mathf.Max(0, particleSize);
            particleLife = Mathf.Max(0, particleLife);
        }


        // Start is called before the first frame update
        void Start()
        {
            m_ParticleSystem = GetComponent<ParticleSystem>();
            trailModule = m_ParticleSystem.trails;
            emissionModule = m_ParticleSystem.emission;
            collisionModule = m_ParticleSystem.collision;
            particleSystemRenderer = m_ParticleSystem.GetComponent<ParticleSystemRenderer>();

            // Make sure the particle system is using world space instead of local!
            mainModule = m_ParticleSystem.main;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

            UpadteParticleSystemSettings();
        }

        private void OnEnable()
        {
            if (m_ParticleSystem != null)
            {
                emissionModule.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (m_ParticleSystem != null)
            {
                emissionModule.enabled = false;
            }
        }

        void UpadteParticleSystemSettings()
        {
            mainModule.startSize = particleSize / transform.lossyScale.magnitude;
            mainModule.startLifetime = particleLife;

            emissionModule.rateOverTime = particleSpawnRate;

            trailModule.enabled = enableParticleTrails;
            if (enableParticleTrails)
            {
                particleSystemRenderer.renderMode = ParticleSystemRenderMode.None;
                collisionModule.lifetimeLoss = 0;
            }
            else
            {
                particleSystemRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                collisionModule.lifetimeLoss = 1;
            }
        }

        void InitParticles()
        {
            // This might be better as a != rather than a < but for now I'm just going with
            // what they have on the Unity Documentation
            if (particles.Length < mainModule.maxParticles)
                particles = new ParticleSystem.Particle[mainModule.maxParticles];

        }

        private void LateUpdate()
        {
            UpadteParticleSystemSettings();

            // Will only update the array if we have fewer particles than we need
            InitParticles();

            // Get all the particles currently in the system
            nParticles = m_ParticleSystem.GetParticles(particles);

            for (int i = 0; i < nParticles; i++)
            {
                // Setting the velocity according to our chosen function
                particles[i].velocity = Vector3.zero;

                for (int j = 0; j < fluidZones.Length; j++)
                {
                    if (fluidZones[j].IsPositionInsideZone(particles[i].position))
                    {
                        particles[i].velocity += fluidZones[j].VelocityFunction(particles[i].position);
                    }
                }

            }

            // Apply the changes to the particles
            m_ParticleSystem.SetParticles(particles, nParticles);

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }

    }
}