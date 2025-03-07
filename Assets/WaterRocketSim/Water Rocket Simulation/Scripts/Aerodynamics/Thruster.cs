using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    public static float waterVolumeFraction;        // ratio of water to air, user supplied
    float waterVolume ;  
    //[HideInInspector]
    public float thrustForce;                  // The ideal thrust produced by the pressurised air pushing the water through the nozzle
    public Vector3 cgJointForce;                // The force measured on the joint reaction as a force sensor
    // Bottle properties, water and air inside the bottle, launch pressure
    public static float bottleVolume ;        // Standard is a 2L bottle so we have 0.002 m^3
    float airVolume ;    // Volume of air remaining in bottle, bottle vol - water vol
    public static float nozzleArea ;       // Area of nozzle with diameter 8.89mm, measured by Alex
    
    public static float bottlePressure ;     // Pressure in bottle - default: 6895Pa. 10 bar =1e6 Pa

    // Simulation values
    private float jetVelocity;                 // Velocity of water jet
    [HideInInspector]
    public float bottlePressure_t2;           // Pressure at time t+1
    private float waterVolume_t2;              // Volume of water at time t+1
    private float airVolume_t2;                // Volume of air at time t+1

    private float rhoWater = 1000;             // Density of water
    [HideInInspector]
    public float waterMass;                    //mass of water currently in bottle, kg

    // Rigid body used to find centre of mass of water in the bottle
    private Rigidbody rbWater, rbBody;
    ConfigurableJoint cg; //cg joint


    // Visual water stream from the rocket
    private ParticleSystem.MainModule waterJet;
    private ParticleSystem waterJetParticles;
    public bool ignition = false; //flag to determine state of engine ignition.

    
    
    bool empty = false;

    // Start is called before the first frame update
    void Start()
    {
        waterVolume = waterVolumeFraction * bottleVolume;
        airVolume = (1 - waterVolumeFraction) * bottleVolume;
        // Get the particle system for the water
        waterJetParticles = transform.Find("Thrust Particle System").GetComponent<ParticleSystem>();
        if (waterJetParticles != null)
        {
            waterJet = waterJetParticles.main;
        }
        waterJet.startSize = 0;
        waterJetParticles.Stop(); // shut off to start with

        ConfigurableJoint[] configurableJoints = gameObject.GetComponentsInChildren<ConfigurableJoint>();
        // there is an existing cj on body which attaches the nose cone. We need to find the other one which is attached to None, which should be the second one...
        // set the anchor point of the cj to the calculated cg position. This means that the rocket will pivot about the cg which is the desired behviour for 'wind tunnel mode'
        cg = configurableJoints[1];

        // Update the values for water and air volume + system mass
        UpdateConfig();
    }

    // This function updates the mass of the rigid body used to represent the water based on the user
    // input of the water volume. Also calculates the remaining volume of air in the bottle
    public void UpdateConfig()
    {
        waterVolume = waterVolumeFraction * bottleVolume;
        airVolume = (1 - waterVolumeFraction) * bottleVolume;
        // Get the rigid body for the water inside the rocket

        rbBody =GetComponent<Rigidbody>();// the body rigid body attached to this transform
        rbWater = transform.parent.Find("Water").GetComponent<Rigidbody>();

        // Clamp the water volume to the bottle's volume - we can't have more water than the bottle holds
        waterVolume = Mathf.Clamp(waterVolume, 0f, bottleVolume);

        // Update the mass of the water
        rbWater.mass = waterVolume * rhoWater;
        waterMass = rbWater.mass;

        // Get the remaining volume inside the bottle for air
        // use max so we don't get a negative volume from rounding errors
        airVolume = Mathf.Max(bottleVolume - waterVolume, 0f);
    }

    // FixedUpdate is called every time step in the simulation - the time step (dt) size can be changed in
    // Edit - Project Settings - Time - Fixed Timestep
    void FixedUpdate()
    {
        
        if (Time.time > 1f && empty!=true) // delay launch from start
        { 
            if (ignition == false)
            {
                waterJetParticles.Play();
                ignition = true;
            }
        
            if (empty != true)
            {
                // Calculate current velocity of jet
                jetVelocity = Mathf.Sqrt(2 * bottlePressure / rhoWater);

                // Volume of water that has been expelled this time step
                float changeInVolume = nozzleArea * jetVelocity * Time.fixedDeltaTime;

                // Update water volume
                waterVolume_t2 = waterVolume - changeInVolume;
                // Update the air volume
                airVolume_t2 = bottleVolume - waterVolume_t2;

                // Update the bottle pressure P1*V1 = P2*V2
                bottlePressure_t2 = bottlePressure * airVolume / airVolume_t2;

                // Calculate the remaining mass of water in the bottle
                rbWater.mass = waterVolume_t2 * rhoWater;
                waterMass = rbWater.mass;
                // Calculate thrust produced
                thrustForce =bottlePressure_t2 * nozzleArea;

                waterJet.startSize = thrustForce / 500; // scale size of particles to thrust level

                // Thrust is always applied out of the nozzle which is aligned with the Y axis of the bottle
                rbBody.AddRelativeForce(new Vector3(0, thrustForce));

                // Store next time steps in current values
                airVolume = airVolume_t2;
                waterVolume = waterVolume_t2;
                bottlePressure = bottlePressure_t2;

                if (waterVolume_t2 <= 0)
                {
                    empty = true;
                    thrustForce = 0;
                    bottlePressure_t2 = 0; // zero the output pressure
                    rbBody.AddRelativeForce(new Vector3(0, thrustForce)); // turn off thrust 
                                                                      // Stop the water particles
                    if (waterJetParticles != null)
                    {
                        waterJetParticles.Stop();
                        ignition = false;
                        
                    }
                    // Remove this script from the rocket
                    //Destroy(this); // dont do this if data logging thrust as it messes up the output file
                }


                // Update the speed of the particle system
                if (waterJetParticles != null)
                {
                    waterJet.startSpeed = jetVelocity/5;
                }


                // If the water volume is less than zero we have no more fuel and the pressure inside the bottle will
                // equal the ambient pressure so we no longer need to calculate thrust force

                  

            }

        }
        cgJointForce = cg.currentForce;
    }
}
