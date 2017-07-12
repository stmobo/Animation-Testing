using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualAnimationController : MonoBehaviour {
    public GameObject animatedObject;       // Object controlling animations
    public GameObject physicsObject;        // Physical object (handles collisions, ragdoll stuff, etc.)
    public RagdollPoseMatch poseMatcher;    // Pose matching script on physicsObject
    public Transform physicsRootBone;       // Root bone for the ragdoll position
    private Transform animRootBone;         // Matching Transform for PhysicsRootBone on the animated object

    // debugging only
    public bool goToFullAnim = false;
    public bool goToFullRagdoll = false;
    public bool goToPoseMatch = false;

    public enum Mode
    {
        FullAnimation,
        PoseMatching,
        FullRagdoll
    }

    public Mode currentMode;

    // Dictionaries mapping bones that are the same across both objects.
    private Dictionary<Transform, Transform> physicsToAnimated;
    private Dictionary<Transform, Transform> animatedToPhysics;

    private Animator physAnimator;
    private Animator animAnimator;

    private Vector3 physRootStart;
    private Vector3 startPos;

    private void matchTransformPair(KeyValuePair<Transform, Transform> xfrmPair)
    {
        Transform toXfrm = xfrmPair.Key;
        Transform fromXfrm = xfrmPair.Value;
        toXfrm.SetPositionAndRotation(fromXfrm.position, fromXfrm.rotation);
    }

    private void matchPhysicsToAnimated() {
        foreach (KeyValuePair<Transform, Transform> xfrmPair in physicsToAnimated) matchTransformPair(xfrmPair);
        physicsObject.transform.SetPositionAndRotation(animatedObject.transform.position, animatedObject.transform.rotation);
    }

    private void matchAnimatedToPhysics() {
        foreach (KeyValuePair<Transform, Transform> xfrmPair in animatedToPhysics) matchTransformPair(xfrmPair);
        animatedObject.transform.SetPositionAndRotation(physicsObject.transform.position, physicsObject.transform.rotation);
        Vector3 currentPhysicsOffset = physicsRootBone.transform.position - physRootStart;
        currentPhysicsOffset.y = 0f;

        transform.position += currentPhysicsOffset;
        physRootStart = physicsRootBone.transform.position;
    }

    private void setPhysicsRendered(bool enabled) { foreach (Renderer r in physicsObject.GetComponentsInChildren<Renderer>()) r.enabled = enabled; }
    private void setAnimatedRendered(bool enabled) { foreach (Renderer r in animatedObject.GetComponentsInChildren<Renderer>()) r.enabled = enabled; }

    private void setKinematics(bool enabled) { foreach (Rigidbody r in physicsObject.GetComponentsInChildren<Rigidbody>()) r.isKinematic = enabled; }

    // Use this for initialization
    void Awake () {
        physAnimator = physicsObject.GetComponent<Animator>();
        animAnimator = animatedObject.GetComponent<Animator>();

        physicsToAnimated = new Dictionary<Transform, Transform>();
        animatedToPhysics = new Dictionary<Transform, Transform>();

        physRootStart = physicsRootBone.position;
        startPos = transform.position;

        if (physAnimator == null || animAnimator == null)
            throw new InvalidOperationException("Both animated and physical GameObjects must have Animator components attached!");

        Transform[] animTransforms = animatedObject.GetComponentsInChildren<Transform>();
        Transform[] physTransforms = physicsObject.GetComponentsInChildren<Transform>();

        animRootBone = Array.Find<Transform>(animTransforms, xfrm => xfrm.gameObject.name.Equals(physicsRootBone.gameObject.name));

        foreach (Transform animXfrm in animTransforms)
        {
            Transform physXfrm = Array.Find<Transform>(physTransforms, xfrm => xfrm.gameObject.name.Equals(animXfrm.gameObject.name));
            if(physXfrm != null)
            {
                if (!animatedToPhysics.ContainsKey(animXfrm) && !physicsToAnimated.ContainsKey(physXfrm))
                {
                    physicsToAnimated.Add(physXfrm, animXfrm);
                    animatedToPhysics.Add(animXfrm, physXfrm);
                }
            }
        }

        Debug.Log("Found " + physicsToAnimated.Count.ToString() + " matched transforms.");

        /*
        foreach (HumanBodyBones boneID in Enum.GetValues(typeof(HumanBodyBones)))
        {
            Transform xfrm1 = animAnimator.GetBoneTransform(boneID);
            Transform xfrm2 = physAnimator.GetBoneTransform(boneID);

            if (xfrm1 != null && xfrm2 != null)
            {
                if(!animatedToPhysics.ContainsKey(xfrm1) && !physicsToAnimated.ContainsKey(xfrm2))
                {
                    physicsToAnimated.Add(xfrm2, xfrm1);
                    animatedToPhysics.Add(xfrm1, xfrm2);
                }
            }
        }
        */

        physAnimator.enabled = false;

        poseMatcher = physicsObject.AddComponent<RagdollPoseMatch>();
        poseMatcher.anchor = animatedObject;
        poseMatcher.active = false;

        currentMode = Mode.FullAnimation;
    }

    void Start()
    {
        animatedMode();
    }

    // Ragdoll while attempting to match animations
    public void matchedPoseMode()
    {
        if (currentMode == Mode.FullAnimation)
        {
            //matchPhysicsToAnimated();

            setAnimatedRendered(false);
            setPhysicsRendered(true);
        }

        poseMatcher.active = true;
        animAnimator.enabled = true;

        setKinematics(false);
        currentMode = Mode.PoseMatching;
    }

    // Go to full animation
    public void animatedMode()
    {
        matchAnimatedToPhysics();

        setAnimatedRendered(true);
        setPhysicsRendered(false);

        poseMatcher.active = false;
        animAnimator.enabled = true;

        setKinematics(false);
        currentMode = Mode.FullAnimation;
    }

    // Go to full ragdoll
    public void physicsMode()
    {
        if(currentMode == Mode.FullAnimation)
        {
            matchPhysicsToAnimated();

            setAnimatedRendered(false);
            setPhysicsRendered(true);
        }

        poseMatcher.RestoreJoints();

        poseMatcher.active = false;
        animAnimator.enabled = false;

        setKinematics(false);
        currentMode = Mode.FullRagdoll;
    }

    void FixedUpdate()
    {
        if(currentMode == Mode.FullAnimation)
        {
            matchPhysicsToAnimated();
        }

        if(goToFullAnim && currentMode != Mode.FullAnimation)
        {
            animatedMode();
        } else if(goToFullRagdoll && currentMode != Mode.FullRagdoll)
        {
            physicsMode();
        } else if(goToPoseMatch && currentMode != Mode.PoseMatching)
        {
            matchedPoseMode();
        }

        goToPoseMatch = false;
        goToFullRagdoll = false;
        goToFullAnim = false;
    }
}
