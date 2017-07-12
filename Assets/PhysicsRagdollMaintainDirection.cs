
using UnityEngine;
using System.Collections;

public class PhysicsRagdollMaintainDirection : MonoBehaviour
{
    public Vector3 chestDirection;
    public Vector3 feetDirection;
    public Vector3 legDirection;

    public Rigidbody chest;
    public Rigidbody leftFoot;
    public Rigidbody rightFoot;
    public Rigidbody leftLeg;
    public Rigidbody rightLeg;

    public float chestSpringStiffness = 275f;
    public float chestSpringDamper = -150f;

    public float legSpringStiffness = 300f;
    public float legSpringDamper = -250f;

    public float footSpringStiffness = 275f;
    public float footSpringDamper = -150f;

    public float chestDisp;
    public float chestVel;
    public float chestTorque;

    void Start()
    {
        // debug only
        chestDirection = -transform.right;
        legDirection = transform.right;
        feetDirection = transform.right;
    }

    void SpringUpdate(Rigidbody rb, Vector3 facingDir, Vector3 torqueDirection, float stiffness, float damper)
    {
        Quaternion lookAngle = Quaternion.LookRotation(facingDir, rb.transform.up);
        float targetAngle = lookAngle.eulerAngles.y; // the +90f here seems a bit hackish

        float displacement = targetAngle - rb.rotation.eulerAngles.y;
        float velocity = rb.angularVelocity.y * (180f / Mathf.PI);

        if (displacement > 180f)
            displacement -= 360f;

        float currentForce = (displacement * stiffness);
        currentForce += (velocity * damper);
        rb.AddTorque(torqueDirection * currentForce * rb.mass);
    }
    
    void FixedUpdate()
    {
        Quaternion lookAngle = Quaternion.LookRotation(chestDirection, chest.transform.up);
        float targetAngle = lookAngle.eulerAngles.y;

        chestDisp = chest.rotation.eulerAngles.y - targetAngle;
        if (chestDisp > 180f)
            chestDisp -= 360f;

        chestVel = chest.angularVelocity.y * (180f / Mathf.PI);
        chestTorque = (chestDisp * chestSpringStiffness) + (chestVel * chestSpringDamper);

        SpringUpdate(chest, chestDirection, Vector3.up, chestSpringStiffness, chestSpringDamper);
        SpringUpdate(leftLeg, legDirection, Vector3.up, legSpringStiffness, legSpringDamper);
        SpringUpdate(rightLeg, legDirection, Vector3.up, legSpringStiffness, legSpringDamper);
        SpringUpdate(leftFoot, feetDirection, Vector3.up, footSpringStiffness, footSpringDamper);
        SpringUpdate(rightFoot, feetDirection, Vector3.up, footSpringStiffness, footSpringDamper);
    }
}
