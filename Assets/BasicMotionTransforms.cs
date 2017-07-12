using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMotionTransforms : MonoBehaviour {
    public Rigidbody rb;
    private Quaternion velRotateTarget;
    private GameObject mainCamera;

    const float rotateSpeed = 10.0f;

    const float maxAccelTiltDegrees = 5.0f;
    const float accelTiltCoeff = 1.0f; // derived from (fwdSpeed + strSpeed) in KeyboardCtrl script

    private Vector3 accel;
    private Vector3 lastVel;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        mainCamera = GameObject.FindWithTag("MainCamera");

        Transform CoM_transform = transform.Find("CenterMass");
        rb.centerOfMass = CoM_transform.localPosition;

        lastVel = rb.velocity;
        velRotateTarget = Quaternion.Euler(0, 0, 0);
    }
    
	
	void FixedUpdate() {
        accel = (rb.velocity - lastVel) / Time.fixedDeltaTime;
        accel.y = 0f;

        lastVel = rb.velocity;

        // Rotate character towards velocity
        Quaternion currentTarget;
        float velAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * (180.0f / Mathf.PI);
        if (rb.velocity.magnitude > 0.5f && Mathf.Abs(rb.rotation.eulerAngles.y - velAngle) > 1.0f && (Mathf.Abs(Input.GetAxis("Horizontal")) > 0f || Mathf.Abs(Input.GetAxis("Vertical")) > 0f))
        {
            velRotateTarget = Quaternion.Euler(0.0f, velAngle, 0.0f);
            Debug.DrawRay(rb.centerOfMass, velRotateTarget * Vector3.forward * 10.0f, Color.cyan);

            currentTarget = velRotateTarget;
        } else
        {
            currentTarget = Quaternion.identity;
        }

        // Tilt char in direction of acceleration:
        float tiltDegrees = Mathf.Clamp(accel.magnitude * accelTiltCoeff, 0.0f, maxAccelTiltDegrees);
        if(accel.magnitude > 1.0f)
        {
            Vector3 tiltAxis = Vector3.Cross(Vector3.up, accel).normalized;
            Debug.DrawRay(rb.position + Vector3.up, tiltAxis * 5.0f, Color.red);
            Quaternion tiltRotation = Quaternion.AngleAxis(tiltDegrees, tiltAxis);

            currentTarget = Quaternion.Slerp(rb.rotation, tiltRotation * currentTarget, Time.fixedDeltaTime * rotateSpeed);
        }

        //Quaternion rotToTarget = Quaternion.Slerp(rb.rotation, currentTarget, Time.fixedDeltaTime * rotateSpeed);
        rb.rotation = currentTarget;// rotToTarget;
    }
}
