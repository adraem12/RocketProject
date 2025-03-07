using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using AerodynamicObjects;
using System.Linq;
using UnityEngine.UIElements;
using System;

public class SimulationConfig : MonoBehaviour
{

    public enum TestMode { FreeFlight, WindTunnel, ThrustTest, DropTest };
    //FreeFlight = Centre of gravity free, thrust enabled, nose cone free. Graph output set to rocket height
    //Use this mode to investigate the overall mission performance of the rocket
    //The angle of launch can be changed by changing

    //WindTunnel = Centre of gravity constrained on a spherical joint, thrust disabled, nosecone fixed. Graph output set to absolute angle of attack
    //Use this mode to investigate the effect of fin shape, orientation and position on the weathercock stability of the rocket
    //By default the wind speed is set to 1m/s in the positive x direction. This can be changed to any wind vector in the Simulation Configuration

    //ThrustTest = Centre of gravity fixed, thrust enabled, nosecone fixed. Graph output set to thrust magnitude
    //Use this mode to investigate the effect of water volume fraction on the overall specific impulse obtained from the propulsion system

    //DropTest = Centre of gravity free, thrust disabled, nose free, ground plane disabled
    //Use this mode to test out the parachute deployment physics
    public static TestMode testMode; //the chosen test mode
    [Header("Test Mode")]
    [Tooltip("Use this to change the type of experiment you want to run in the simulation.")]
    public TestMode _testMode;

    [HideInInspector]
    [Tooltip("Volume of bottle in litres")]
    [Range(0.5f, 3f)]
    public float bottleVolume = 2f;
    [HideInInspector]
    [Tooltip("Diameter of rocket nozzle in mm. Default value is 9mm. The maximum is 21mm which corresponds to the width of a standard bottle opening")]
    [Range(5f, 21f)]
    public float nozzleDiameter = 8.89f;
    [Header("Rocket design parameters")]
    [Tooltip("This parameter sets the precision with which the fins and nose cone are aligned with the axis of the rocket. A build quality of 1 means a professional job with perfect alignment. A build quality of zero means it will hold together but nothing is very well aligned. Student built rockets will typically be somewhere in the middle")]
    [Range(0f, 1f)]
    public float buildQuality = 0.46f;

    [Tooltip("Controls how tall the nose cone is.")]
    [Range(0.7f, 3f)]
    public float noseConeLengthToDiameterRatio = 1.77f;
    [Tooltip("The diameter of the parachute, m. Increasing this increases drag on descent but increases over all mass and larger parachutes take longer to deploy")]
    [Range(0.1f, 1f)]
    public float parachuteDiameter = 0.52f;
    [Tooltip("Panform area of one fin, m^2.")]
    [Range(0.0002f, 0.02f)]
    public float finArea; // planform area of one fin
    [Tooltip("Fin slenderness")]
    [Range(0.25f, 3f)]
    public float finAspectRatio;
    [Tooltip("Leading edge sweep angle, aft positive, degrees")]
    [Range(0f, 45f)]
    public float finSweepAngle;
    [Tooltip("Displacement of fins from bottom of rocket")]
    [Range(-0.2f, 0.4f)]
    public float hubVerticalOffset;

    [Header("Launch settings")]
    [Tooltip("Ratio of water volume to total volume of bottle at launch. 0 = no water in bottle, 1 = bottle is full of water.")]
    [Range(0f, 1f)]
    public float waterVolumeFraction = 0.48f;
    [Tooltip("The launch pressure in atmospheres. The default for all launches is 8. Anything much more than that and the bottles tend to explode.")]
    [Range(0f, 10f)]
    public float launchPressure = 5f;
    [Tooltip("Launch elevation angle, degrees. 90 degrees means straight up.")]
    [Range(15f, 90f)]
    public float launchElevationAngle = 50f;
    [Range(-180f, 180f)]
    [Tooltip("Launch azimuth angle, degrees. Use to change which way the rocket points in the horizontal plane. Makes no difference if the elevation angle is 90 degrees.")]
    public float launchAzimuthAngle = -7f;


    [Header("Wind conditions")]
    public WindVelocity windVelocitySettings;
    [Tooltip("Tick this to include unsteady wind conditions. See the Wind Fluid Zone game object in the scene hierarchy for turbulence settings")]
    public bool enableTurbulence = false;

    WindFluidZone windFluidZone; //the wind space for the simulation

    [Header("Simulation parameters")]
    [Tooltip("Time stretch applied to simulation output. Values <1 are slow motion. Does not change the physics but does scale the time in the recorded results which will need correcting afterwards.")]
    [Range(0.01f, 1)]
    public float SlowMotionFactor = .55f;

    [Tooltip("The time step of the numerical solver for the physics in s. Smaller values increase simulation accuracy and stability but mean you have to run at greater slow motion to achieve smooth frame rate. Don't touch unless you want to use super slow motion")]
    public float TimeStep = 0.005f;
    [Tooltip("Disable this if you want to increase the simulation frame rate by not saving data during the simulation.")]
    public bool logData = false; // set to true to plot data to Grapher app. Reduces frame rate by around 50%. Set to false for high performance


    Transform body;
    Rigidbody egg;
    float eggOldSpeed = 0; // used for finding egg acceleration
    Queue<float> eggAccelerationBuffer; // array to find moving average of acceleration
    int accelerationFilterLength = 20; // Number of acceleration measurements in moving average filter
    float summedEggAcceleration = 0;
    Thruster thruster; //reference to the thruster script on the body
    ConfigurableJoint cg; // config joint at the cg
    CalculateCoG calculateCoG; // reference to cg calculator
    Transform maxHeight; // for onscreen display
    Transform flightTime;
    PerformanceMeasure performanceMeasure; // reference to script that records flight data
    AeroObject rocketBodyAeroObject;
    Transform flowFieldParticles;
    Transform flowFieldArrows;
    FinHub finHub;

    public void Start()
    {
        //call set up to refresh
        Setup();

    }
    public void Setup() // Also called via the Sim config editor script
    {
        windFluidZone = transform.parent.Find("Wind Fluid Zone").GetComponent<WindFluidZone>();

        //Get references to relevant objects
        body = transform.parent.Find("Launch Pad/Rocket/Body");
        //centreOfGravity = transform.parent.Find("Launch Pad/Rocket/CentreOfGravity").GetComponent<Rigidbody>();
        egg = transform.parent.Find("Launch Pad/Rocket/Egg").GetComponent<Rigidbody>();
        thruster = body.GetComponent<Thruster>();
        Transform launcherInterface = transform.parent.Find("Launch Pad/Launcher interface");
        Transform launchPlate = transform.parent.Find("Lighting and Scenery/Launch Plate");
        Transform simulatorMode = transform.parent.Find("User Interface/Text Overlay/Simulator Mode"); // screen area for displaying text
        maxHeight = transform.parent.Find("User Interface/Text Overlay/Max Height"); // screen area for flight data
        flightTime = transform.parent.Find("User Interface/Text Overlay/Flight Time"); // screen area for flight data
        Transform groundPlane = transform.parent.Find("Lighting and Scenery/Ground Plane");
        Transform groundPlaneUnderneath = transform.parent.Find("Lighting and Scenery/Ground Plane underneath");
        Transform backPlane = transform.parent.Find("Lighting and Scenery/Back Plane");
        // Note both cameras track the centre of gravity by default
        Transform perspectiveCamera = transform.parent.Find("Cameras/PerspectiveCamera");
        Transform orthographicCamera = transform.parent.Find("Cameras/OrthographicCamera");
        Transform orbitCamera = transform.parent.Find("Cameras/Orbit Cam");
        Transform water = transform.parent.Find("Launch Pad/Rocket/Water");
        Transform thrustStand = transform.parent.Find("Lighting and Scenery/Thrust Stand");
        ParachuteManager pm = transform.parent.Find("Launch Pad/Rocket/Parachute/ParachuteRigidBody").GetComponent<ParachuteManager>();
        performanceMeasure = transform.parent.Find("Launch Pad/Rocket").GetComponent<PerformanceMeasure>();
        rocketBodyAeroObject = transform.parent.Find("Launch Pad/Rocket/Body/BottleGraphic/AeroBody").GetComponent<AeroObject>();

        testMode = _testMode; // store public version as static version
        Time.fixedDeltaTime = TimeStep;
        Time.timeScale = SlowMotionFactor;

        //transfer public variables to other scripts as needed
        Thruster.bottleVolume = bottleVolume / 1000f;// bottle volume in m^3
        Thruster.bottlePressure = launchPressure * 101325f; // convert atmospheres to N/m^2 (Pa)
        Thruster.waterVolumeFraction = waterVolumeFraction;
        Thruster.nozzleArea = Mathf.PI * Mathf.Pow(nozzleDiameter / 1000f, 2) / 4;
        thruster.UpdateConfig();    // Update the water mass so CG is appropriate before playmode
                                    //transform.parent.Find("Launch Pad").rotation = Quaternion.Euler(0, launchAzimuthAngle, launchElevationAngle-90);

        calculateCoG = transform.parent.Find("Launch Pad/Rocket").GetComponent<CalculateCoG>(); // 
        //transform.parent.Find("Launch Pad/Rocket/Body/FinHub").localPosition = new Vector3(0, finVerticalPosition, 0);
        transform.parent.Find("Launch Pad/Rocket/Nose/NoseConeGraphic").localScale = new Vector3(0.95f, noseConeLengthToDiameterRatio, 0.95f);

        water.localPosition =
            new Vector3(UnityEngine.Random.Range(-1f, 1f) * (1 - buildQuality) * 0.02f, water.localPosition.y, UnityEngine.Random.Range(-1f, 1f) * (1 - buildQuality) * 0.01f);

        //Randomise the location of the egg in the lateral plane based on build quality
        float y = egg.transform.localPosition.y;
        egg.transform.localPosition = new Vector3(UnityEngine.Random.Range(-0.03f, 0.03f) * (1 - buildQuality), y, UnityEngine.Random.Range(-0.03f, 0.03f) * (1 - buildQuality));

        //Set up the filter list for egg accerlation
        eggAccelerationBuffer = new Queue<float>(accelerationFilterLength);
        //pack out initial queue with zeros. 
        for (int i = 0; i < accelerationFilterLength; i++)
        {
            eggAccelerationBuffer.Enqueue(0);
        }


        //adjust fin orientation according to build quality
        Transform[] fins = transform.parent.Find("Launch Pad/Rocket/Body/FinHub").
            GetComponentsInChildren<Transform>().Where(t => t.name == "FinPrefab").ToArray();
        foreach (Transform t in fins)
        {
            t.localEulerAngles = new Vector3(UnityEngine.Random.Range(-1f, 1f) * (1 - buildQuality) * 15, t.localEulerAngles.y, 0);
        }

        //Set up the test mode
        pm.parachuteSize = parachuteDiameter;
        //Scaling of size and mass cause stability issues at either end of parachute size scale so disabled for now. Mass is a dynamics thing, scale is a coding issue
        pm.initialScale = 0.05f;// * Mathf.Sqrt(parachuteDiameter); // a parchute 1m in diameter packs to a 5cm dia ball
        pm.parachuteMass = 0.1f * Mathf.Pow(parachuteDiameter, 2); // as above, weighs 50g. Should be square scaling but dynamics blow up for small parachutes so leave as linear

        //Setup build quality using static variable in aerodynamics script
        //SimplifiedFlatPlateAerodynamics.aeroBuildQuality = buildQuality;

        //get and set cg config joint on body
        //update the cg position


        calculateCoG.GetRigidBodies();
        calculateCoG.FindCoG();
        calculateCoG.UpdateCoGMarker();

        ConfigurableJoint[] configurableJoints = body.GetComponentsInChildren<ConfigurableJoint>();
        // there is an existing cj on body which attaches the nose cone. We need to find the other one which is attached to None, which should be the second one...
        // set the anchor point of the cj to the calculated cg position. This means that the rocket will pivot about the cg which is the desired behviour for 'wind tunnel mode'
        cg = configurableJoints[1];
        cg.anchor = body.InverseTransformPoint(calculateCoG.cg);
        print("Total mass = " + calculateCoG.massSum + " kg, CoG 3d position in body axes = " + transform.parent.Find("Launch Pad/Rocket/Body").InverseTransformPoint(calculateCoG.cg).ToString("F3") + " m."); // Note that the ToString allows you show more dp in console output

        //print("Dry mass = " + (CalculateCoG.massSum-thruster.waterMass).ToString("F3") + " kg"); // Note that the ToString allows you show more dp in console output

        simulatorMode.GetComponent<TextMeshProUGUI>().text = testMode.ToString() + " Mode";

        // do launch pad rotation
        //undo the launch pad fixed joint if it exists
        if (transform.parent.Find("Launch Pad/Launcher interface").TryGetComponent<FixedJoint>(out var fj)) DestroyImmediate(fj);
        //rotate launch pad
        transform.parent.Find("Launch Pad").rotation = Quaternion.Euler(0, launchAzimuthAngle, launchElevationAngle - 90);
        // fix launcher interface to the ground
        transform.parent.Find("Launch Pad/Launcher interface").gameObject.AddComponent<FixedJoint>();

        flowFieldArrows = transform.parent.Find("Wind Fluid Zone/Flow Field Arrows");
        flowFieldParticles = transform.parent.Find("Wind Fluid Zone/Flow Field Particles");

        finHub = transform.parent.GetComponentInChildren<FinHub>();

        finHub.finArea = finArea;
        finHub.finSweepAngle = finSweepAngle;
        finHub.finAspectRatio = finAspectRatio;
        finHub.hubVerticalOffset = hubVerticalOffset;
        finHub.UpdateFins();

        switch (testMode)
        {
            case TestMode.FreeFlight:
                {

                    ChangeCGJointToFree();
                    launcherInterface.gameObject.SetActive(true); // enable launcher
                    body.GetComponent<NoseRelease>().fixNoseconeToBody = false; // Set the nose cone to free
                    launchPlate.gameObject.SetActive(true);
                    thruster.enabled = true;
                    water.gameObject.SetActive(true);
                    groundPlane.gameObject.SetActive(true);
                    groundPlaneUnderneath.gameObject.SetActive(true);
                    backPlane.gameObject.SetActive(false);
                    perspectiveCamera.gameObject.SetActive(false);
                    orthographicCamera.gameObject.SetActive(false);
                    orbitCamera.gameObject.SetActive(true);
                    thrustStand.gameObject.SetActive(false);
                    flowFieldArrows.gameObject.SetActive(true);
                    flowFieldParticles.gameObject.SetActive(false);
                    windFluidZone.useBoundaryLayerProfile = true;
                    windFluidZone.windVelocity = windVelocitySettings;
                    windFluidZone.enableTurbulence = enableTurbulence;

                    break;
                }
            case TestMode.WindTunnel:
                {
                    ChangeCGJointToPivot();
                    launcherInterface.gameObject.SetActive(false); // disable the launcher
                    body.GetComponent<NoseRelease>().fixNoseconeToBody = true;//Set the nose cone to fixed on
                    launchPlate.gameObject.SetActive(false);
                    thruster.enabled = false;
                    water.gameObject.SetActive(false);
                    groundPlane.gameObject.SetActive(false);
                    groundPlaneUnderneath.gameObject.SetActive(false);
                    backPlane.gameObject.SetActive(true);
                    perspectiveCamera.gameObject.SetActive(false);
                    orthographicCamera.GetComponent<TrackCamera>().enabled = false;
                    orthographicCamera.gameObject.SetActive(true);
                    orbitCamera.gameObject.SetActive(false);
                    windVelocitySettings.speed = 5;
                    windVelocitySettings.azimuth = 90;
                    thrustStand.gameObject.SetActive(false);
                    flowFieldArrows.gameObject.SetActive(false);
                    flowFieldParticles.gameObject.SetActive(false);
                    windFluidZone.useBoundaryLayerProfile = false;
                    windVelocitySettings.azimuth = 90;
                    windVelocitySettings.elevation = 0;
                    windFluidZone.windVelocity = windVelocitySettings;
                    windFluidZone.enableTurbulence = enableTurbulence;

                    break;
                }
            case TestMode.ThrustTest:
                {
                    ChangeCGJointToFixed();
                    launcherInterface.gameObject.SetActive(false); // disable the launcher
                    body.GetComponent<NoseRelease>().fixNoseconeToBody = true;//Set the nose cone to fixed on
                    launchPlate.gameObject.SetActive(true);
                    thruster.enabled = true;
                    water.gameObject.SetActive(true);
                    groundPlane.gameObject.SetActive(false);
                    groundPlaneUnderneath.gameObject.SetActive(false);
                    backPlane.gameObject.SetActive(true);
                    perspectiveCamera.gameObject.SetActive(false);
                    orthographicCamera.gameObject.SetActive(true);
                    orbitCamera.gameObject.SetActive(false);
                    orthographicCamera.GetComponent<TrackCamera>().enabled = false;

                    //reduce timescale to .1 to allow sufficient time resolution of thrust
                    SlowMotionFactor = .5f;
                    Time.timeScale = SlowMotionFactor;
                    thrustStand.gameObject.SetActive(true);
                    launchElevationAngle = 90f;
                    flowFieldArrows.gameObject.SetActive(false);
                    flowFieldParticles.gameObject.SetActive(false);

                    windVelocitySettings.speed = 0;
                    windFluidZone.windVelocity = windVelocitySettings;
                    windFluidZone.enableTurbulence = false;
                    break;
                }
            case TestMode.DropTest:
                {
                    ChangeCGJointToFree();
                    launcherInterface.gameObject.SetActive(false); // disable the launcher
                    body.GetComponent<NoseRelease>().fixNoseconeToBody = false;//Set the nose cone to free
                    launchPlate.gameObject.SetActive(false);
                    thruster.enabled = false;
                    water.gameObject.SetActive(false);
                    groundPlane.gameObject.SetActive(false);
                    groundPlaneUnderneath.gameObject.SetActive(false);
                    backPlane.gameObject.SetActive(false);
                    perspectiveCamera.gameObject.SetActive(true);
                    orthographicCamera.gameObject.SetActive(false);
                    orbitCamera.gameObject.SetActive(false);

                    thrustStand.gameObject.SetActive(false);
                    flowFieldArrows.gameObject.SetActive(true);
                    flowFieldParticles.gameObject.SetActive(false);
                    windVelocitySettings.speed = 0;
                    windFluidZone.windVelocity = windVelocitySettings;
                    windFluidZone.enableTurbulence = false;

                    break;
                }

        }

    }

    void FixedUpdate()
    {
        // update cg position. To DO: Need to only update during thrust on! 
        if (thruster.ignition == true) // only update the cg position when the thrust is on
        {
            calculateCoG.FindCoG();
            calculateCoG.UpdateCoGMarker();
        }

        Time.timeScale = SlowMotionFactor; // dynamically update slowmotion input

        // show the flight test data if in free flight mode
        if (testMode == TestMode.FreeFlight)
        {
            maxHeight.GetComponent<TextMeshProUGUI>().text = "Max Height = " + performanceMeasure.maxHeight.ToString("F2") + " m";
            flightTime.GetComponent<TextMeshProUGUI>().text = "Flight Time = " + performanceMeasure.flightTime.ToString("F2") + " s";
        }

        if (logData == true) // only log data if needed as slows simulation down
        {
            //Set up the Grapher logging app according to which test mode has been selected
            switch (testMode)
            {
                case TestMode.FreeFlight:
                    {
                        float eggSpeed = egg.velocity.magnitude;
                        float currentEggAcceleration = (eggSpeed - eggOldSpeed) / Time.fixedDeltaTime;  // CHANGED to fixedDeltaTime
                        eggOldSpeed = eggSpeed;
                        eggAccelerationBuffer.Enqueue(currentEggAcceleration); // add new acceleration value
                        var oldestAcceleration = eggAccelerationBuffer.Dequeue(); // removed oldest value
                        summedEggAcceleration += currentEggAcceleration; //add current value
                        summedEggAcceleration -= oldestAcceleration;// take away oldest value

                        float averageEggAcceleration = summedEggAcceleration / accelerationFilterLength;

                        Grapher.Log(egg.position.y, "Egg height above ground, m");
                        Grapher.Log(egg.velocity.magnitude, "Egg velocity magnitude, m/s");
                        Grapher.Log(averageEggAcceleration / 9.81f, "Eggceleration magnitude, 'g'");
                        break;
                    }
                case TestMode.WindTunnel:
                    {
                        float absAngleOfAttack = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(rocketBodyAeroObject.ao.BodyRelativeVelocity.ToUnity().normalized, -Vector3.right));
                        Grapher.Log(absAngleOfAttack, "Absolute angle of attack (angle between the wind vector and the rocket axis of axial symmetry(, degrees");
                        break;
                    }
                case TestMode.ThrustTest:
                    {
                        float thrust = thruster.thrustForce;
                        Grapher.Log(thrust, "Thrust, N", Color.red);
                        break;
                    }
                case TestMode.DropTest:
                    {
                        Grapher.Log(egg.velocity.y, "Vertical Velocity, m/s", Color.red);
                        break;
                    }
            }
        }
    }
    public void ChangeCGJointToFree() // free flight mode
    {
        //change the translation mode of this joint from the default of free to fixed
        cg.xMotion = ConfigurableJointMotion.Free;
        cg.yMotion = ConfigurableJointMotion.Free;
        cg.zMotion = ConfigurableJointMotion.Free;
        cg.angularXMotion = ConfigurableJointMotion.Free;
        cg.angularYMotion = ConfigurableJointMotion.Free;
        cg.angularZMotion = ConfigurableJointMotion.Free;
    }

    public void ChangeCGJointToPivot() // wind tunnel mode
    {
        // change the translation mode of this joint from the default of free to fixed
        cg.xMotion = ConfigurableJointMotion.Locked;
        cg.yMotion = ConfigurableJointMotion.Locked;
        cg.zMotion = ConfigurableJointMotion.Locked;
        cg.angularXMotion = ConfigurableJointMotion.Free;
        cg.angularYMotion = ConfigurableJointMotion.Free;
        cg.angularZMotion = ConfigurableJointMotion.Free;
    }

    public void ChangeCGJointToFixed() //used for thrust test mode
    {
        // change the translation mode of this joint from the default of free to fixed
        cg.xMotion = ConfigurableJointMotion.Locked;
        cg.yMotion = ConfigurableJointMotion.Locked;
        cg.zMotion = ConfigurableJointMotion.Locked;
        cg.angularXMotion = ConfigurableJointMotion.Locked;
        cg.angularYMotion = ConfigurableJointMotion.Locked;
        cg.angularZMotion = ConfigurableJointMotion.Locked;
    }
}
