using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardCtrl : MonoBehaviour {
    public Rigidbody rb;
    public GameObject mainCamera;

    const float strSpeed = 5.0f;
    const float fwdSpeed = 5.0f;

    const float jumpSpeed = 5.0f;
    float maxSpeed = 7.5f;
    const float decelSpeed = 0.5f;

    const float airMulti = 0.1f;

    public bool onGround = false;
    public bool jumped = false;

	// Use this for initialization
	void Start () {
        mainCamera = GameObject.FindWithTag("MainCamera");
        rb = GetComponent<Rigidbody>();
	}

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            onGround = true;
            if(jumped)
            {
                jumped = false;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            onGround = false;
        }
    }
	
	void FixedUpdate()
    {
        float str = Input.GetAxis("Horizontal");
        float fwd = Input.GetAxis("Vertical");

        // Velocity axes for forward/strafe movement
        Vector3 fwdAxis = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up);
        Vector3 strAxis = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up);

        Vector3 accel = (fwdAxis * fwdSpeed * fwd) + (strAxis * strSpeed * str);

        if(Input.GetButton("Crouch"))
        {
            maxSpeed = 2.5f;
        } else
        {
            maxSpeed = 7.5f;
        }

        if(!onGround)
        {
            accel *= airMulti;
        } else if (Mathf.Abs(str) < 0.1 && Mathf.Abs(fwd) < 0.1) {
            accel = rb.velocity.normalized * -decelSpeed;
        }

        Vector3 nextVel = rb.velocity + accel;

        if (onGround && Input.GetButtonDown("Jump") && !jumped)
        {
            jumped = true;
            rb.velocity += (Vector3.up * jumpSpeed);
        }

        if(onGround)
        {
            if (nextVel.magnitude < maxSpeed)
            {
                rb.AddForce(accel * rb.mass * 10f);
            }
            else
            {
                rb.velocity = nextVel.normalized * maxSpeed;
            }

            if (rb.velocity.magnitude < 0.5f)
                rb.velocity = Vector3.zero;
        }
    }
}
