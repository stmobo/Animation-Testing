using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkCycleWheel : MonoBehaviour {
    public Rigidbody rb;
    public Animator animator;
    public KeyboardCtrl kbdCtrl;
    
    public const float maxSpeed = 7.5f;

    public const float walkStrideDist = 7f; // unity units
    public const float runStrideDist = 3f;

    private Vector3 lastPos;

    private float curCrouchTarget = 0f;
    private float curCrouchStage = 0f;
    private float curCrouchVelocity = 0f;
    private const float crouchSpringStiffness = 0.1f;
    private const float crouchSpringDamping = 0.75f;

    private float curJumpTarget = 0f;
    private float curJumpStage = 0f;
    private float curJumpVelocity = 0f;
    private const float jumpSpringStiffness = 0.1f;
    private const float jumpSpringDamping = 0.75f;

    private float currentDist = 0.0f;

    private int currentCyclePos = 0;
    private int cycleLen = 2;

    void Start () {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        kbdCtrl = GetComponent<KeyboardCtrl>();

        lastPos = rb.position;
    }

    void drawWheel()
    {
        float currentAngle = (currentDist / walkStrideDist) * 90.0f;

        Vector3 center = transform.position;
        center.y += 1.0f;

        Debug.DrawLine(center + (transform.right * 5), center + (transform.right * -5), Color.white);

        Vector3 markerOffset1 = Quaternion.AngleAxis(currentAngle, transform.right) * transform.up;
        Vector3 halfwayMarker1 = Quaternion.AngleAxis(currentAngle + 45.0f, transform.right) * transform.up;
        Vector3 markerOffset2 = Quaternion.AngleAxis(currentAngle + 90.0f, transform.right) * transform.up;
        Vector3 halfwayMarker2 = Quaternion.AngleAxis(currentAngle + 135.0f, transform.right) * transform.up;
        Vector3 markerOffset3 = Quaternion.AngleAxis(currentAngle + 180.0f, transform.right) * transform.up;

        Color mainColor = Color.red;
        if (currentCyclePos == 1)
            mainColor = Color.green;

        Debug.DrawLine(center, center + (markerOffset1 * 5), mainColor);
        Debug.DrawLine(center, center + (halfwayMarker1 * 5), Color.blue);
        Debug.DrawLine(center, center + (markerOffset2 * 5), mainColor);
        Debug.DrawLine(center, center + (halfwayMarker2 * 5), Color.blue);
        Debug.DrawLine(center, center + (markerOffset3 * 5), mainColor);

        // reflect lines
        Debug.DrawLine(center, center - (markerOffset1 * 5), mainColor);
        Debug.DrawLine(center, center - (halfwayMarker1 * 5), Color.blue);
        Debug.DrawLine(center, center - (markerOffset2 * 5), mainColor);
        Debug.DrawLine(center, center - (halfwayMarker2 * 5), Color.blue);
        Debug.DrawLine(center, center - (markerOffset3 * 5), mainColor);
    }

    void OnWalkTick()
    {
        currentCyclePos = (currentCyclePos + 1) % cycleLen;
    }

    void Update() {
        if(!kbdCtrl.onGround || kbdCtrl.jumped)
        {
            animator.Play("Jump");
        }
        else if (rb.velocity.magnitude >= 0.5f)
        {
            animator.Play("MoveBlend");

            float dist = Vector3.Distance(lastPos, rb.position);
            lastPos = rb.position;

            float speedBlend = (rb.velocity.magnitude / maxSpeed);
            speedBlend = Mathf.Clamp(speedBlend, 0.0f, 1.0f);
            float currentStrideDist = (speedBlend * runStrideDist) + ((1.0f - speedBlend) * walkStrideDist);

            currentDist += dist;

            if (currentDist >= currentStrideDist)
            {
                currentDist -= currentStrideDist;
            }

            float currentStride = (currentDist / currentStrideDist);

            animator.SetFloat("SpeedBlend", speedBlend);
            animator.SetFloat("RunStage", currentStride);
        }
        else
        {
            if (Input.GetButton("Crouch"))
                curCrouchTarget = 1.0f;
            else
                curCrouchTarget = 0.0f;

            float crouchAccel = -((curCrouchStage - curCrouchTarget) * crouchSpringStiffness) - (curCrouchVelocity * crouchSpringDamping);
            curCrouchVelocity += crouchAccel;
            curCrouchStage += curCrouchVelocity;

            curCrouchStage = Mathf.Clamp(curCrouchStage, 0.0f, 1.0f);

            if(Mathf.Abs(curCrouchVelocity) > 0.05f || Mathf.Abs(curCrouchStage) > 0.0005f)
            {
                animator.Play("Crouch");
                animator.SetFloat("RunStage", curCrouchStage);
            } else
            {
                animator.Play("Idle");
            }
        }
	}
}
