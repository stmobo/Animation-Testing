using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchAnimScript : MonoBehaviour {

    public float curCrouchTarget = 0f;
    public float curCrouchStage = 0f;
    public float curCrouchVelocity = 0f;
    public const float crouchSpringStiffness = 0.1f;
    public const float crouchSpringDamping = 0.75f;
    public Animator animator;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Crouch"))
            curCrouchTarget = 1.0f;
        else
            curCrouchTarget = 0.0f;

        float crouchAccel = -((curCrouchStage - curCrouchTarget) * crouchSpringStiffness) - (curCrouchVelocity * crouchSpringDamping);
        curCrouchVelocity += crouchAccel;
        curCrouchStage += curCrouchVelocity;

        curCrouchStage = Mathf.Clamp(curCrouchStage, 0.0f, 1.0f);

        if (Mathf.Abs(curCrouchVelocity) > 0.05f || Mathf.Abs(curCrouchStage) > 0.0005f)
        {
            animator.Play("Crouch");
            animator.SetFloat("RunStage", curCrouchStage);
        }
    }
}
