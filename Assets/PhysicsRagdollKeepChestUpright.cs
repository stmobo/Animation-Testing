using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsRagdollKeepChestUpright : MonoBehaviour {
    public Rigidbody chest;
    public Rigidbody pelvis;

    private BoxCollider chestCollider;

    private Vector3 chestTop;
    private Vector3 chestBottom;
    private Vector3 chestFront;

    public float baseForceMagnitude;

    public float baseForceCoeff = 1f;
    public float chestUpForceCoeff = 1f;        //135f;
    public float chestDownForceCoeff = 0.2f;   //(75f / 135f);

    public Vector3 chestStraightenAxis;
    public float chestStraightenStiffness = 0.003f;
    public float chestStraightenDamper = -0f;
    public float chestStraightenLinDamper = 0f;

    private float startingChestAngle;
    public float chestStraightenMagnitude;

    public float currentChestAngleDisplacement;
    public float currentChestAngularVelocity;

    // Use this for initialization
    void Start () {
        chestCollider = chest.GetComponent<BoxCollider>();

        float totalMass = 0;
        Rigidbody[] allRBs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in allRBs)
        {
            totalMass += rb.mass;
        }

        if (Mathf.Abs(chestStraightenAxis.x) > 0.1f) startingChestAngle = chest.rotation.eulerAngles.x;
        if (Mathf.Abs(chestStraightenAxis.y) > 0.1f) startingChestAngle = chest.rotation.eulerAngles.y;
        if (Mathf.Abs(chestStraightenAxis.z) > 0.1f) startingChestAngle = chest.rotation.eulerAngles.z;

        baseForceMagnitude = totalMass * Physics.gravity.magnitude * baseForceCoeff;
    }
	
	void FixedUpdate () {
        chestBottom = chestCollider.center;
        chestTop = chestCollider.center;
        chestFront = chestCollider.center;

        chestTop.x -= chestCollider.size.x / 2f;
        chestBottom.x += chestCollider.size.x / 2f;
        chestFront.y += chestCollider.size.y / 2f;

        chestTop = chest.transform.TransformPoint(chestTop);
        chestBottom = chest.transform.TransformPoint(chestBottom);
        chestFront = chest.transform.TransformPoint(chestFront);

        Debug.DrawRay(chestTop, Vector3.up, Color.white);
        Debug.DrawRay(chestBottom, Vector3.down, Color.red);

        chest.AddForceAtPosition(Vector3.up * baseForceMagnitude * chestUpForceCoeff, chestTop);
        chest.AddForceAtPosition(Vector3.down * baseForceMagnitude * chestDownForceCoeff, chestBottom);

        float chestDisp = 0f;
        float chestAVel = 0f;

        if (Mathf.Abs(chestStraightenAxis.x) > 0.1f) chestDisp = chest.rotation.eulerAngles.x;
        else if (Mathf.Abs(chestStraightenAxis.y) > 0.1f) chestDisp = chest.rotation.eulerAngles.y;
        else if (Mathf.Abs(chestStraightenAxis.z) > 0.1f) chestDisp = chest.rotation.eulerAngles.z;

        if (Mathf.Abs(chestStraightenAxis.x) > 0.1f) chestAVel = chest.angularVelocity.x;
        else if (Mathf.Abs(chestStraightenAxis.y) > 0.1f) chestAVel = chest.angularVelocity.y;
        else if (Mathf.Abs(chestStraightenAxis.z) > 0.1f) chestAVel = chest.angularVelocity.z;

        chestAVel *= (180f / Mathf.PI);

        chestDisp = startingChestAngle - chestDisp;

        if (chestDisp > 180f)
            chestDisp -= 360f;
        else if (chestDisp < -180f)
            chestDisp += 360f;

        chestDisp = Mathf.Abs(chestDisp);

        currentChestAngleDisplacement = chestDisp;
        currentChestAngularVelocity = chestAVel;

        chestStraightenMagnitude = (chestDisp * chestStraightenStiffness) + (chestAVel * chestStraightenDamper);
        chestStraightenMagnitude *= baseForceMagnitude;

        Vector3 bottomForce = Vector3.down * (Mathf.Clamp(chest.velocity.y, 0, Mathf.Infinity) * chestStraightenLinDamper);

        Debug.DrawRay(chestBottom, bottomForce, Color.magenta);
        Debug.DrawRay(chestTop, chestStraightenAxis, Color.blue);

        Vector3 topForce = Vector3.up * chestStraightenMagnitude;

        Debug.DrawRay(chestTop, topForce, Color.cyan);

        chest.AddForceAtPosition(topForce, chestTop);
        chest.AddForceAtPosition(bottomForce, chestBottom);
        //chest.AddForceAtPosition(botForce, chestBottom);
        //chest.AddTorque(currentTorqueAxis * chestTorqueMagnitude * chest.mass);
    }
}
