using System.Collections;
using UnityEngine;
using System.IO;

namespace AerodynamicObjects
{
    public class WindTunnel : MonoBehaviour
    {
        /*  I'm doing a big overhaul with this updated version!
         *  
         *  I think for now, we will only collect Cl, Cd and Cm
         *  
         *  This will be a "core" version which has no notion of control surfaces
         *  In another update we will include the aircraft controls version which will
         *  allow users to provide range values for the "user inputs" e.g. attitude controls: roll pitch yaw etc
         *  Those values will be included in the range of tests and the user can see how Cm varies with pitch input for example
         *  
         *  But for now, this wind tunnel test will just throw the object in a wind tunnel, rotate it round 360 degrees
         *  and spit out the variation in Cl, Cd and Cm with alpha
         *  
         *  Need to decide on how the wind direction is defined so that we can easily extract "lift" and "drag" from the force readings
         *  and so we can set the aircraft rotation properly!
         */

        [Header("Aircraft Components")]
        public Rigidbody aircraftRb;
        //public Transform leadingEdge;
        // This is the transform we'll position and rotate throughout the experiments
        Transform AircraftRoot;
        // This is the joint we will create and use to pin the aircraft at its centre of gravity
        // No need to create one yourself!
        ConfigurableJoint joint;


        [Header("Reference aircraft and fluid properties, used to calculate coefficients")]
        [Tooltip("Planform Area, S (m^2)")]
        public float wingArea = 0.72f;
        [Tooltip("Mean Aerodynamic Chord, MAC (m)")]
        public float meanAerodynamicChord = 0.233f;
        [Tooltip("Fluid Density, rho (kg/m^3)")]
        public float rho = 1.225f;
        // Dynamic pressure
        private float dynamicPressure;

        // Which way the wind will blow for zero alpha and zero beta
        public float windAzimuth;
        Vector3 windDirection = Vector3.forward;
        Quaternion windRotation;
        // How fast the wind will blow, doesn't really make a difference
        public float windSpeed = 10f;


        // Force readings
        public Vector3 measuredForceCoefficients, measuredTorqueCoefficients, measuredForce, measuredTorque;
        // Used to tare the force balance
        Vector3 zeroForce, zeroTorque;

        [Tooltip("If true, headers will be full words, e.g. Drag Coefficient. Otherwise, they will be shorthand, e.g. CD.")]
        public bool useVerboseHeaders = true;
        // Where to save the file
        public string path = "Assets\\Unity Wind Tunnel Data.txt";

        [Space(20), Tooltip("Increase this value to slow down the experiment visuals")]
        [Range(1f, 200f)]
        public float slowDownFactor = 1f;

        public int numberOfDataPoints = 100;

        // Properties of the aircraft which should be moved elsewhere    

        // Going to run through a range of angle of attack and angle of sideslip values - DEGREES!!!
        [Header("Independent variable ranges")]
        public float alphaMin = -90f;
        public float alphaMax = 90f;    // Degrees
        public bool performBetaSweep = true;
        public float betaMin = -90f;
        public float betaMax = 90f;      // Degrees
        public float alphaUsedInBetaSweep = 0f;


        public bool done;

        // Start is called before the first frame update
        void Start()
        {
            AircraftRoot = aircraftRb.transform.root;

            windDirection = new Vector3(Mathf.Sin(windAzimuth * Mathf.Deg2Rad), 0, Mathf.Cos(windAzimuth * Mathf.Deg2Rad));
            windRotation = new Quaternion(0, Mathf.Sin(windAzimuth * Mathf.Deg2Rad / 2f), 0, Mathf.Cos(windAzimuth * Mathf.Deg2Rad / 2f));

            windDirection.Normalize();
            dynamicPressure = 0.5f * rho * windSpeed * windSpeed;

            GlobalFluid.FluidVelocity = windSpeed * windDirection;

            AddJoint();
            Tare();

            if (!done)
                StartCoroutine(GetAircraftAlphaData());
        }

        private void OnValidate()
        {

        }

        private void FixedUpdate()
        {
            if (done)
                MeasureForces();
        }

        #region Force Readings

        public void Tare()
        {
            if (joint == null)
            {
                Debug.LogWarning("Could not tare force balance, no joint was found.");
                return;
            }
            // Run a physics update to get the forces on the joint
            Physics.simulationMode = SimulationMode.Script;
            Physics.Simulate(Time.fixedDeltaTime);

            // Get the current forces on the joint so we can offset
            zeroForce = -joint.currentForce;
            zeroTorque = -joint.currentTorque;
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }

        // We need a transformation between Unity and Aircraft axes - rotate and mirror image
        void MeasureForces()
        {
            // Earth and wind axes coincide - the joint reads in Unity's global frame
            measuredForce = joint.currentForce + zeroForce;
            measuredTorque = joint.currentTorque + zeroTorque;

            measuredForceCoefficients = measuredForce / (dynamicPressure * wingArea);

            measuredForceCoefficients = windRotation * measuredForceCoefficients;

            // Drag could be acting in either X or Z or a combination of both depending on the azimuth
            measuredForceCoefficients.z *= -1;
            measuredForceCoefficients.x *= -1;
            measuredForceCoefficients.y *= -1;

            // Now we want to align the torques into the wind direction so we have pitch, yaw, roll 

            measuredTorqueCoefficients = measuredTorque / (dynamicPressure * wingArea * meanAerodynamicChord);
            measuredTorqueCoefficients = windRotation * measuredTorqueCoefficients;

            measuredTorqueCoefficients = UnityToAircraftMoment(measuredTorqueCoefficients);
        }

        #endregion

        public static Vector3 UnityToAircraftDirection(Vector3 unity)
        {
            return new Vector3(unity.x, -unity.y, unity.z);
        }

        public static Vector3 UnityToAircraftMoment(Vector3 unity)
        {
            return new Vector3(-unity.x, unity.y, -unity.z);
        }

        #region Joint

        public void AddJoint()
        {
            joint = aircraftRb.gameObject.GetComponent<ConfigurableJoint>();
            if (joint == null)
            {
                joint = aircraftRb.gameObject.AddComponent<ConfigurableJoint>();
            }

            // Join the centre of mass to the world at the centre of mass location
            // This seems a bit weird but it's correct!
            joint.anchor = aircraftRb.centerOfMass;
            joint.connectedAnchor = aircraftRb.worldCenterOfMass;

            // Fixed in translation
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            // Fixed in rotation
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
        }

        public void RemoveJoint()
        {
            joint = aircraftRb.gameObject.GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                DestroyImmediate(joint);
            }
        }

        #endregion


        //void SetCgPosition(float macPercentage)
        //{
        //    RemoveJoint();

        //    Vector3 cgLocalToLeadingEdge = new Vector3(0, 0, -macPercentage * meanAerodynamicChord);
        //    Vector3 cgGlobal = leadingEdge.TransformPoint(cgLocalToLeadingEdge);
        //    aircraftRb.centerOfMass = aircraftRb.transform.InverseTransformPoint(cgGlobal);

        //    AddJoint();
        //}

        string GenerateAlphaFileHeader()
        {
            // File needs to go
            // alpha, Cd, Cl, Cm

            // Start with alpha
            string header = useVerboseHeaders ? "Angle of Attack\tDrag Coefficient\tLift Coefficient\tLift to Drag Ratio\tPitching Moment Coefficient\tRolling Moment Coefficient\tYawing Moment Coefficient" : "alpha\tCD\tCL\tCpitch\tCroll\tCyaw";

            return header;
        }

        string GenerateBetaFileHeader()
        {
            // File needs to go
            // alpha, Cd, Cl, Cm
            // Start with alpha
            string header = useVerboseHeaders ? "Angle of Sideslip\tDrag Coefficient\tLift Coefficient\tLift to Drag Ratio\tPitching Moment Coefficient\tRolling Moment Coefficient\tYawing Moment Coefficient" : "alpha\tCD\tCL\tCpitch\tCroll\tCyaw";
            return header;
        }

        public IEnumerator GetAircraftAlphaData()
        {
            Debug.Log("Starting Alpha Sweep");

            string alphaPath = path.Split('.')[0] + " alpha.txt";

            // Create the data file and put the header in
            FileStream f = File.Create(alphaPath);
            f.Close();

            string header = GenerateAlphaFileHeader();
            header += '\n';
            File.WriteAllText(alphaPath, header);

            float oldDt = Time.fixedDeltaTime;
            Time.fixedDeltaTime = 0.001f * slowDownFactor;

            // Calculate the step size for alpha given the range and number of points
            float alphaIncrement = (alphaMax - alphaMin) / (numberOfDataPoints - 1);
            float alpha = alphaMin;

            // Wait for the physics to simulate
            yield return new WaitForFixedUpdate();

            // Iterate over the angle of attack range
            for (int i = 0; i < numberOfDataPoints; i++)
            {
                string data = alpha.ToString() + "\t";

                // Set the angle of attack by rotating the aircraft - note this isn't rotating about the CG
                SetAircraftRotation(alpha);

                // Turn off the wind to tare the force balance
                GlobalFluid.FluidVelocity = Vector3.zero;
                yield return new WaitForFixedUpdate();

                // Re-tare the force balance
                Tare();

                // Reset the wind
                GlobalFluid.FluidVelocity = windSpeed * windDirection;
                yield return new WaitForFixedUpdate();

                // Measure the force acting on the joint
                MeasureForces();

                // Get the coefficients
                float Cd = measuredForceCoefficients.z;
                data += Cd.ToString() + "\t";

                float Cl = measuredForceCoefficients.y;
                data += Cl.ToString() + "\t";

                data += (Cl / Cd).ToString() + "\t";

                float Cpitch = measuredTorqueCoefficients.x;
                data += Cpitch.ToString() + "\t";

                float Croll = measuredTorqueCoefficients.z;
                data += Croll.ToString() + "\t";

                float Cyaw = measuredTorqueCoefficients.y;
                data += Cyaw.ToString() + "\t";

                data += "\n";

                File.AppendAllText(alphaPath, data);
                // Increment the angle of attack for the next run
                alpha += alphaIncrement;
            }

            done = true;
            Time.fixedDeltaTime = oldDt;

            Debug.Log("Alpha Sweep Done.");

            if (performBetaSweep)
            {
                StartCoroutine(GetAircraftBetaData());
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
        }


        public IEnumerator GetAircraftBetaData()
        {
            Debug.Log("Starting Beta Sweep");

            string betaPath = path.Split('.')[0] + " beta.txt";

            // Create the data file and put the header in
            FileStream f = File.Create(betaPath);
            f.Close();

            string header = GenerateBetaFileHeader();
            header += '\n';
            File.WriteAllText(betaPath, header);

            // Set the time scale so the user can see each position in the sweep
            float oldDt = Time.fixedDeltaTime;
            Time.fixedDeltaTime = 0.001f * slowDownFactor;

            // Calculate the step size for beta given the range and number of points
            float betaIncrement = (betaMax - betaMin) / (numberOfDataPoints - 1);
            float beta = betaMin;

            // Wait for the physics to simulate
            yield return new WaitForFixedUpdate();

            // Iterate over the angle of attack range
            for (int i = 0; i < numberOfDataPoints; i++)
            {
                string data = beta.ToString() + "\t";

                // Set angles by rotating the aircraft - note this isn't rotating about the CG
                SetAircraftRotation(alphaUsedInBetaSweep, beta);

                // Turn off the wind to tare the force balance
                GlobalFluid.FluidVelocity = Vector3.zero;
                yield return new WaitForFixedUpdate();

                // Re-tare the force balance
                Tare();

                // Reset the wind
                GlobalFluid.FluidVelocity = windSpeed * windDirection;
                yield return new WaitForFixedUpdate();

                // Measure the force acting on the joint
                MeasureForces();

                // Get the coefficients
                float Cd = measuredForceCoefficients.z;
                data += Cd.ToString() + "\t";

                float Cl = measuredForceCoefficients.y;
                data += Cl.ToString() + "\t";

                data += (Cl / Cd).ToString() + "\t";

                float Cpitch = measuredTorqueCoefficients.x;
                data += Cpitch.ToString() + "\t";

                float Croll = measuredTorqueCoefficients.z;
                data += Croll.ToString() + "\t";

                float Cyaw = measuredTorqueCoefficients.y;
                data += Cyaw.ToString() + "\t";

                data += "\n";

                File.AppendAllText(betaPath, data);
                // Increment the angle of attack for the next run
                beta += betaIncrement;
            }

            done = true;
            Time.fixedDeltaTime = oldDt;

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            Debug.Log("Beta Sweep Done.");
        }

        public void SetAircraftRotation(Quaternion rotation)
        {
            RemoveJoint();
            AircraftRoot.rotation = rotation;
            AddJoint();
        }

        // Only set angle of attack
        public void SetAircraftRotation(float _alpha)
        {
            RemoveJoint();
            AircraftRoot.rotation = Quaternion.Euler(-_alpha, 0, 0);
            AddJoint();
        }

        // Set angle of attack and angle of sideslip
        public void SetAircraftRotation(float _alpha, float _beta)
        {
            RemoveJoint();
            AircraftRoot.rotation = Quaternion.Euler(-_alpha, -_beta, 0);
            AddJoint();
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}