using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsRagdollMovement : MonoBehaviour {
    public Rigidbody chest;

    public Rigidbody leftUpperLeg;
    public Rigidbody leftLeg;
    public Rigidbody leftFoot;
    private CharacterJoint leftHipJoint;
    private CharacterJoint leftKneeJoint;

    public Rigidbody rightUpperLeg;
    public Rigidbody rightLeg;
    public Rigidbody rightFoot;
    private CharacterJoint rightHipJoint;
    private CharacterJoint rightKneeJoint;

    private BoxCollider chestCollider;
    private Collider leftFootCollider;
    private Collider rightFootCollider;

    private Vector3 leftFootBottom;
    private Vector3 rightFootBottom;
    
    private Vector3 chestFront;

    public float totalMass;

    /* Default pose parameters */
    public float baseForceMagnitude;           //135f;

    public float baseForceCoeff = 1f;
    public float chestUpForceCoeff = 1f;        //135f;
    public float chestDownForceCoeff = 0.2f;   //(75f / 135f);
    public float chestFwdForceCoeff = 0f;
    
    public float feetPlantCoeff = 0.3f;

    /* Walk / Step cycle parameters */
    public float feetLiftCoeff = 0.2f;
    public float feetMoveCoeff = 0.2f;
    public float feetMoveSideCoeff = 0.05f;
    public float feetDropCoeff = 0.1f;
    public float hipTwistCoeff = -500f;
    public float kneeTwistCoeff = -250f;

    public Vector3 leftStepStart;
    public int leftStepPhase = 0;   /* 0 = dropped / planted, 1 = lifting, 2 = moving forward */
    
    public Vector3 rightStepStart;
    public int rightStepPhase = 0;

    public float stepLiftDist = 0.2f;       // units
    public float stepStrideDist = 0.5f;     // units
    private float currentStepStrideDist = 1.0f;

    public int walkCyclePhase = 0; /* 0 = stopped, 1 = half-stride, 2 = full-stride 1, 3 = full-stride 2 */
    public bool walkCyclePhaseStarted = false;

    public bool leftWalking = false;
    public bool rightWalking = false;
    public bool walking = false;

    /*
     * 0.1 chest downforce coeff + 0.1 feet plant coeff = char stands by default
     * 0.5 chest downforce coeff + 0.1 feet plant coeff = char falls but looks like they're trying to get back up
     */

    void Start () {
        chestCollider = chest.GetComponent<BoxCollider>();
        leftFootCollider = leftFoot.GetComponent<Collider>();
        rightFootCollider = rightFoot.GetComponent<Collider>();

        leftHipJoint = leftUpperLeg.gameObject.GetComponent<CharacterJoint>();
        leftKneeJoint = leftLeg.gameObject.GetComponent<CharacterJoint>();

        rightHipJoint = rightUpperLeg.gameObject.GetComponent<CharacterJoint>();
        rightKneeJoint = rightLeg.gameObject.GetComponent<CharacterJoint>();

        Rigidbody[] allRBs = GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in allRBs)
        {
            totalMass += rb.mass;
        }

        baseForceMagnitude = totalMass * Physics.gravity.magnitude * baseForceCoeff;
    }
	
    private void StepCycleUpdate(Rigidbody foot, Rigidbody leg, Rigidbody upperLeg, CharacterJoint hip, CharacterJoint knee, Vector3 footBottom, Vector3 walkDir, Vector3 sideMoveDir, ref int phase, ref Vector3 stepStart)
    {
        phase = Mathf.Clamp(phase, 0, 3);

        Vector3 curDisp = foot.position - stepStart;
        Vector2 horzDisp = new Vector2(curDisp.x, curDisp.z);

        Vector3 hipAxis = upperLeg.transform.TransformDirection(hip.axis);
        Vector3 kneeAxis = upperLeg.transform.TransformDirection(knee.axis);

        if (phase == 1)
        {
            /* Lift foot up */
            if (curDisp.y >= stepLiftDist)
                phase = 2;

            upperLeg.AddTorque(hipAxis * hipTwistCoeff * baseForceMagnitude);
            leg.AddTorque(kneeAxis * kneeTwistCoeff * baseForceMagnitude);

            foot.AddForce(Vector3.up * feetLiftCoeff * baseForceMagnitude);
        } else if(phase == 2)
        {
            /* Move foot forward */
            if (horzDisp.magnitude >= currentStepStrideDist)
                phase = 3; // then drop foot


            upperLeg.AddTorque(hipAxis * 2f * hipTwistCoeff * baseForceMagnitude);
            leg.AddTorque(kneeAxis * kneeTwistCoeff * baseForceMagnitude);

            leg.AddForce(Vector3.up * (leg.mass + foot.mass) * Physics.gravity.magnitude);
            foot.AddForce(walkDir * feetMoveCoeff * baseForceMagnitude);
            chest.AddForceAtPosition(walkDir * chestFwdForceCoeff * baseForceMagnitude, chestFront);
            foot.AddForce(sideMoveDir * feetMoveSideCoeff * baseForceMagnitude);
        } else if(phase == 3)
        {
            RaycastHit rcast;
            int lm = ~(1<<8);
            bool foundGround = Physics.Raycast(footBottom, Vector3.down, out rcast, Mathf.Infinity, lm);
            
            if (foundGround && rcast.distance <= 0.05)
                phase = 0;

            foot.AddForceAtPosition(Vector3.down * feetDropCoeff * baseForceMagnitude, footBottom);
            //leg.AddForce(Vector3.down * feetLiftCoeff * baseForceMagnitude);
        }
        else
        {
            /* Drop foot down or keep it planted */
            foot.AddForceAtPosition(Vector3.down * feetPlantCoeff * baseForceMagnitude, footBottom);
        }
    }

    void FixedUpdate() {
        leftFootBottom = leftFootCollider.bounds.center;
        leftFootBottom.y -= leftFootCollider.bounds.size.y / 4f;

        rightFootBottom = rightFootCollider.bounds.center;
        rightFootBottom.y -= rightFootCollider.bounds.size.y / 4f;

        chestFront = chestCollider.center;
        chestFront.y += chestCollider.size.y / 2f;
        chestFront = chest.transform.TransformPoint(chestFront);

        if (walking)
        {
            if(walkCyclePhase == 0)
            {
                leftStepPhase = 0;
                rightStepPhase = 0;
            } else if(walkCyclePhase == 1)
            {
                currentStepStrideDist = stepStrideDist / 2f;
                if (rightStepPhase == 0)
                {
                    if(!walkCyclePhaseStarted)
                    {
                        rightStepPhase = 1;
                        rightStepStart = rightFoot.position;
                        walkCyclePhaseStarted = true;
                    } else
                    {
                        walkCyclePhase = 2;
                        rightStepPhase = 0;
                        walkCyclePhaseStarted = false;
                    }
                }
            } else
            {
                currentStepStrideDist = stepStrideDist;
                if (!walkCyclePhaseStarted)
                {
                    walkCyclePhaseStarted = true;
                    
                    if (walkCyclePhase == 2) // left foot full stride
                    {
                        leftStepPhase = 1;
                        leftStepStart = leftFoot.position;
                    } else // walkCyclePhase == 3
                    {
                        rightStepPhase = 1;
                        rightStepStart = rightFoot.position;
                    }
                } else
                {
                    if(walkCyclePhase == 2 && leftStepPhase == 0)
                    {
                        walkCyclePhaseStarted = false;
                        leftStepPhase = 0;
                        walkCyclePhase = 3;
                    } else if(walkCyclePhase == 3 && rightStepPhase == 0)
                    {
                        walkCyclePhaseStarted = false;
                        rightStepPhase = 0;
                        walkCyclePhase = 2;
                    }
                }
            }
        } else
        {
            if (leftWalking && leftStepPhase == 0)
            {
                leftStepPhase = 1;
                leftStepStart = leftFoot.position;
            }
            leftWalking = false;

            if (rightWalking && rightStepPhase == 0)
            {
                rightStepPhase = 1;
                rightStepStart = rightFoot.position;
            }
            rightWalking = false;
        }

        StepCycleUpdate(leftFoot, leftLeg, leftUpperLeg, leftHipJoint, leftKneeJoint, leftFootBottom, transform.forward, -transform.right, ref leftStepPhase, ref leftStepStart);
        StepCycleUpdate(rightFoot, rightLeg, rightUpperLeg, rightHipJoint, rightKneeJoint, rightFootBottom, transform.forward, transform.right, ref rightStepPhase, ref rightStepStart);
    }
}
