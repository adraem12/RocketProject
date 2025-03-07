using AerodynamicObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParachuteManager : MonoBehaviour
    // This script controls the behaviour of the physical parachute and the rendering of a parachute graphic to match the size, position and orientation of the parachute
    // physical parachute. The physical parachute is modelled as a sphere that increases with size the further it moves away from its original 
    // anchor point. The sphere is attached to the body of the rocket via 'string' joint which allows displacement in any direction upto the limit o
    // of the string length. 
{
    Transform body, nose, parachuteSphereGraphic; // references to body, nose and parachute transforms.


    /*     CHANGE NOTES
     *     I have removed the intermediate rigidbody and attached one configurable joint to the
     *     parachute rigidbody game object. This lead to more issues, mainly that the collider for the parachute wasn't moving
     *     as the chute deployed. I've fixed this by swapping the collider when the chute is
     *     fully deployed. The parachute doesn't have a sliding joint now, the main collider moves because
     *     it is offset from the origin of the scaled parachute object, so as the parachute grows the collider
     *     moves up and grows too.
     */
    public AeroObject parachuteAeroObject;
    // This is the collider object aligned with the top of the parachute
    Transform deployedGraphic;     // - ParachuteGraphic/ParachuteActualGraphic/DeployedCollider
    public float initialScale = 0.05f;     // the size of the parachute ball as packed to start with, m.
    public float parachuteMass = 0.05f;
    bool deployed = false;          // variable to record whether the parachute has been deployed or not. The parachute is locked in
    ConfigurableJoint cjs;
    bool parachuteEnabled=false;
    [Range(0.1f,2)]
    public float parachuteSize; // This sets the string joint linear limit length
    [Range(0.05f,0.2f)]
    public float stageSeparationForDeployment=0.01f; // user defined value that sets when parachute is released for deployment based on the distance 
    //between the nosecone and the body. In practice the parachute will deploy differently depending on how its is packed into the nose cone
    //so this is effectively a tuning variable. A value of between 5 to 15cm seems about right. The max value cant be larger than than the string length 
    //holding the nose cone on, which as a default is set to 20cm (0.2m).
    Vector3 noseOffsetFromBody; // initial location of nose relative to top of bottle. Used to find stage separation distance
    float stageSeparation;
    Transform parachuteActualGraphic;

    // Keeping track of the drag model so we can swap the collider when fully deployed
    
    // The parachute rigidbody
    Rigidbody rb;
    // How full the parachute is
    float fillAmount;
    // Volume of chute at full deployment
    float maxFillAmount;
    Vector3 localVelocity;
    // Value between 0 and 1 to limit how much air flowing into the parachute
    // actually stays and fills the parachute
    float fillRate = .2f;

    // Start is called before the first frame update
    void Start()
    {      
        // Get the relevant components
        body = transform.parent.parent.Find("Body"); // the rocket body
        parachuteActualGraphic = transform.Find("ParachuteGraphic/ParachuteActualGraphic");
        deployedGraphic = transform.Find("ParachuteGraphic/ParachuteActualGraphic/DeployedCollider");
        parachuteSphereGraphic = transform.Find("ParachuteSphereGraphic"); // Sphere graphic we are using as a placeholder for the rendered parachute
        nose = transform.parent.parent.Find("Nose"); // the nosecone
        rb = GetComponent<Rigidbody>();
        
        rb.mass = parachuteMass;
        rb.inertiaTensor = parachuteMass * 0.02f * 0.02f * Vector3.one;
        
        // Total volume of parachute fully opened
        maxFillAmount = (2f / 3f) * Mathf.PI * Mathf.Pow(initialScale + parachuteSize, 3);
        parachuteSphereGraphic.localScale = new Vector3(initialScale, initialScale, initialScale);

        // Turn off the parachute aerodynamics to start with        
        parachuteAeroObject.enabled = false;

        noseOffsetFromBody = body.transform.InverseTransformDirection(nose.transform.position - body.transform.position);// position vector of nose in body frame

        // Get the sphereical joint acting as the parachute connector and fix for launch
        cjs = GetComponent<ConfigurableJoint>();
        cjs.xMotion = ConfigurableJointMotion.Locked;
        cjs.yMotion = ConfigurableJointMotion.Locked;
        cjs.zMotion = ConfigurableJointMotion.Locked;
        cjs.angularXMotion = ConfigurableJointMotion.Locked;
        cjs.angularYMotion = ConfigurableJointMotion.Locked;
        cjs.angularZMotion = ConfigurableJointMotion.Locked;

        // Hide the deployed parachute for launch
        parachuteActualGraphic.gameObject.SetActive(false);
        parachuteSphereGraphic.GetComponent<MeshCollider>().enabled = false;
    }


    void FixedUpdate()
    {
        if (deployed == false)
        {
            // How far the nose has separated from the body. 
            stageSeparation = Vector3.Magnitude(nose.transform.position - (body.transform.TransformPoint(noseOffsetFromBody)));

            // True if the parachute has not yet deployed and the stage separation has just exceeded the deployment threshold
            // parachuteEnabled means the nose cone can land on the rocket and the parachute continues to function after coming out
            if (parachuteEnabled || stageSeparation > stageSeparationForDeployment)
            {
                // Initialise the parachute by freeing up the joint and enabling the aerodynamic drag script. 
                if (parachuteEnabled == false)
                {
                    InitialiseParachute();
                }

                // Fill and scale up the parachute based on air flow into the parachute
                FillParachute();
            }
        }
    }

    void InitialiseParachute()
    {
        //Do just the first time this code block is entered
        parachuteAeroObject.enabled = true;
        parachuteSphereGraphic.gameObject.SetActive(false); // Disable sphere parachute
        parachuteActualGraphic.gameObject.SetActive(true);  // Enable the real parachute

        // Only using a spherical joint
        cjs.xMotion = ConfigurableJointMotion.Locked;
        cjs.yMotion = ConfigurableJointMotion.Locked;
        cjs.zMotion = ConfigurableJointMotion.Locked;
        cjs.angularXMotion = ConfigurableJointMotion.Free;
        cjs.angularYMotion = ConfigurableJointMotion.Free;
        cjs.angularZMotion = ConfigurableJointMotion.Free;

        parachuteEnabled = true;
    }

    void FillParachute()
    {
        // Get the local velocity of the parachute
        if (parachuteAeroObject.ao is not null && parachuteAeroObject.ao.LocalRelativeVelocity is not null)
            localVelocity = parachuteAeroObject.ao.LocalRelativeVelocity.ToUnity();
        // Using velocity perpendicular to the parachute, i.e. flowing directly up into the chute
        float fillVelocity = Mathf.Clamp(-localVelocity.y,0,10);  // Only air moving up into the parachute should fill it
        // Use area of circle
        float currentArea = Mathf.PI * parachuteActualGraphic.localScale.x * parachuteActualGraphic.localScale.x;   // Pi*r^2

        // Volume flow rate into the parachute, using fill rate to limit the total capture of air
        // dV = dv*A
        fillAmount += fillRate * fillVelocity * currentArea * Time.fixedDeltaTime;
        if (fillAmount < 0)
        {
            // The parachute can't implode
            fillAmount = 0;
        }

        float extensionFraction = fillAmount / maxFillAmount;

        if (extensionFraction >= 1f) // parachute is all the way out
        {
            FinishDeployment();
            // Stop the extension from exceeding 1 and producing an enourmous chute
            extensionFraction = 1f;
        }
        // Scale up the parachute size based on the fill volume
        float scale = initialScale + parachuteSize * Mathf.Pow(extensionFraction, 1f / 3f);
        parachuteActualGraphic.localScale = new Vector3(scale, scale, scale);
    }

    void FinishDeployment()
    {
        // Nothing much to do here, there's no extra joint to destroy/replace
        deployed = true;
        print("Parachute fully deployed");
    }
}
