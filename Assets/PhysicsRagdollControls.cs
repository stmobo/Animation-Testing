using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsRagdollControls : MonoBehaviour {
    PhysicsRagdollMaintainDirection directionMaint;
    PhysicsRagdollMovement mover;

    public Rigidbody head;
    public GameObject mainCamera;

    private Collider headCollider;
    private Vector3 headTop;

    public float headHoldForceMagnitude = 10f;
    public float facingAngle = 0;
    float facingChangeRate = 90f;

    public bool drawDebugRays = false;

	// Use this for initialization
	void Start () {
        directionMaint = GetComponent<PhysicsRagdollMaintainDirection>();
        mover = GetComponent<PhysicsRagdollMovement>();

        headCollider = head.gameObject.GetComponent<Collider>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float strafe = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");

        facingAngle += strafe * (facingChangeRate * Time.fixedDeltaTime);

        if (facingAngle > 360f)
            facingAngle -= 360f;

        if (facingAngle < -360f)
            facingAngle += 360f;

        Vector3 facingDir = new Vector3(Mathf.Sin(facingAngle * (Mathf.PI / 180f)), 0f, Mathf.Cos(facingAngle * (Mathf.PI / 180f)));
        Vector3 localFacingDir = Vector3.Cross(facingDir, transform.up);

        if(drawDebugRays)
        {
            Debug.DrawRay(directionMaint.chest.transform.position, facingDir.normalized, Color.green);
            Debug.DrawRay(directionMaint.chest.transform.position, localFacingDir.normalized, Color.magenta);
        }

        if (forward > 0f)
        {
            mover.walking = true;
            if (mover.walkCyclePhase == 0)
                mover.walkCyclePhase = 1;
        } else
        {
            mover.walking = false;
            //mover.walkCyclePhase = 0;
        }

        mover.walkingDir = facingDir;
        directionMaint.chestDirection = localFacingDir;
        directionMaint.legDirection = directionMaint.feetDirection = -localFacingDir;

        
	}
}
