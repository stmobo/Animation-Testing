using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPoseMatch : MonoBehaviour {
    private struct JointConstraintState
    {
        public SoftJointLimit lowAngularX;
        public SoftJointLimit highAngularX;
        public SoftJointLimit angularY;
        public SoftJointLimit angularZ;
        public SoftJointLimit linear;

        public void save(ConfigurableJoint joint)
        {
            this.lowAngularX = joint.lowAngularXLimit;
            this.highAngularX = joint.highAngularXLimit;
            this.angularY = joint.angularYLimit;
            this.angularZ = joint.angularZLimit;
            this.linear = joint.linearLimit;
        }

        public void save(CharacterJoint joint)
        {
            this.lowAngularX = joint.lowTwistLimit;
            this.highAngularX = joint.highTwistLimit;
            this.angularY = joint.swing1Limit;
            this.angularZ = joint.swing2Limit;
            this.linear = new SoftJointLimit();
        }

        public void restore(ConfigurableJoint joint)
        {
            joint.lowAngularXLimit = this.lowAngularX;
            joint.highAngularXLimit = this.highAngularX;
            joint.angularYLimit = this.angularY;
            joint.angularZLimit = this.angularZ;
            joint.linearLimit = this.linear;
        }
    }

    public GameObject anchor;
    public bool active;
    public bool disableGravity;

    // Each CharacterJoint is limited to (targetTransform axis rotation - jointLimLow) at the lowest, and (targetTransform axis rotation + jointLimHigh)
    // at the highest.
    private const float twistLimLow = 2.0f;
    private const float twistLimHigh = 2.0f;
    private const float swing1Lim = 2.0f;
    private const float swing2Lim = 2.0f;
    private const float interpSpeed = 10f;

    private Dictionary<ConfigurableJoint, Transform> jointMap;
    private Dictionary<ConfigurableJoint, Vector3> startAngles; // X = twist axis angle, Y = swing 1 axis angle, Z = swing 2 axis angle
    private Dictionary<ConfigurableJoint, JointConstraintState> startConstraints;
    private Dictionary<Transform, Transform> boneMap;

    // Take a direction in directionAxis and a value, and set the corresponding axis in toAxis to value.
    // for example, mapToAxis( (0,0,0), 90.0, (1,0,0) ) => (90.0, 0, 0).
    private Vector3 mapToAxis(Vector3 toAxis, float value, Vector3 directionAxis)
    {
        if (Mathf.Abs(directionAxis.x) > 0.1f) toAxis.x = value * -directionAxis.x;
        else if (Mathf.Abs(directionAxis.y) > 0.1f) toAxis.y = value * -directionAxis.y;
        else if (Mathf.Abs(directionAxis.z) > 0.1f) toAxis.z = value * -directionAxis.z;

        return toAxis;
    }

    private float getFromAxis(Vector3 from, Vector3 directionAxis)
    {
        if (Mathf.Abs(directionAxis.x) > 0.1) return from.x * -directionAxis.x;
        else if (Mathf.Abs(directionAxis.y) > 0.1) return from.y * -directionAxis.y;
        else if (Mathf.Abs(directionAxis.z) > 0.1) return from.z * -directionAxis.z;
        else throw new ArgumentException("Invalid axis passed to getFromAxis!");
    }

    private float getAngleWithAxis(Transform xfrm, Vector3 axis)
    {
        
        if (Mathf.Abs(axis.x) > 0.1) return xfrm.eulerAngles.x * -axis.x;
        else if (Mathf.Abs(axis.y) > 0.1) return xfrm.eulerAngles.y * -axis.y;
        else if (Mathf.Abs(axis.z) > 0.1) return xfrm.eulerAngles.z * -axis.z;
        else throw new ArgumentException("Invalid axis passed to getAngleWithAxis!");
        //return getFromAxis(xfrm.eulerAngles, axis);
    }

    private Vector3 getJointTargetAngles(CharacterJoint joint, Transform target)
    {
        float twistAngle = getAngleWithAxis(target, joint.axis);
        float swing1Angle = getAngleWithAxis(target, joint.swingAxis);
        float swing2Angle = getAngleWithAxis(target, Vector3.Cross(joint.axis, joint.swingAxis));

        return new Vector3(twistAngle, swing1Angle, swing2Angle);
    }

    private Vector3 getJointTargetAngles(ConfigurableJoint joint, Transform target)
    {
        float twistAngle = getAngleWithAxis(target, joint.axis);
        float swing1Angle = getAngleWithAxis(target, joint.secondaryAxis);
        float swing2Angle = getAngleWithAxis(target, Vector3.Cross(joint.axis, joint.secondaryAxis));

        return new Vector3(twistAngle, swing1Angle, swing2Angle);
    }

    private ConfigurableJoint copyCharacterJoint(CharacterJoint joint)
    {
        ConfigurableJoint cfj = joint.gameObject.AddComponent<ConfigurableJoint>();

        cfj.axis = joint.axis;
        cfj.secondaryAxis = joint.swingAxis;
        
        cfj.lowAngularXLimit = joint.lowTwistLimit;
        cfj.highAngularXLimit = joint.highTwistLimit;
        cfj.angularYLimit = joint.swing1Limit;
        cfj.angularZLimit = joint.swing2Limit;

        SoftJointLimitSpring twistLimitSpring = new SoftJointLimitSpring();
        twistLimitSpring.spring = 100f;
        twistLimitSpring.damper = 25f;

        SoftJointLimitSpring swingLimitSpring = new SoftJointLimitSpring();
        swingLimitSpring.spring = 50f;
        swingLimitSpring.damper = 50f;

        cfj.angularXLimitSpring = twistLimitSpring;  //joint.twistLimitSpring;
        cfj.angularYZLimitSpring = swingLimitSpring; //joint.swingLimitSpring;
        cfj.linearLimitSpring = twistLimitSpring;

        SoftJointLimit linLimit = new SoftJointLimit();
        linLimit.limit = 5f;
        cfj.linearLimit = linLimit;

        /*if(joint.enableProjection)
        {*/
            cfj.projectionMode = JointProjectionMode.PositionAndRotation;
            cfj.projectionAngle = 180f;
            cfj.projectionDistance = 1000f;
        //}

        cfj.anchor = joint.anchor;
        cfj.autoConfigureConnectedAnchor = joint.autoConfigureConnectedAnchor;
        cfj.breakForce = joint.breakForce;
        cfj.breakTorque = joint.breakTorque;
        cfj.connectedAnchor = joint.connectedAnchor;
        cfj.connectedBody = joint.connectedBody;
        cfj.enableCollision = joint.enableCollision;
        cfj.enablePreprocessing = joint.enablePreprocessing;

        cfj.angularXMotion = ConfigurableJointMotion.Limited;
        cfj.angularYMotion = ConfigurableJointMotion.Limited;
        cfj.angularZMotion = ConfigurableJointMotion.Limited;

        cfj.xMotion = ConfigurableJointMotion.Locked;
        cfj.yMotion = ConfigurableJointMotion.Locked;
        cfj.zMotion = ConfigurableJointMotion.Locked;

        JointDrive twistDriver = new JointDrive();
        twistDriver.positionDamper = 100f;
        twistDriver.positionSpring = 10f;
        twistDriver.maximumForce = Mathf.Infinity;

        JointDrive swingDriver = new JointDrive();
        swingDriver.positionDamper = 100f;
        swingDriver.positionSpring = 10f;
        twistDriver.maximumForce = Mathf.Infinity;

        JointDrive slerpDriver = new JointDrive();
        slerpDriver.positionDamper = 50f;
        slerpDriver.positionSpring = 250f;
        slerpDriver.maximumForce = Mathf.Infinity;

        JointDrive linDriver = new JointDrive();
        linDriver.positionDamper = 50f;
        linDriver.positionSpring = 250f;
        linDriver.maximumForce = Mathf.Infinity;

        cfj.angularXDrive = twistDriver;
        cfj.angularYZDrive = swingDriver;

        cfj.xDrive = linDriver;
        cfj.yDrive = linDriver;
        cfj.zDrive = linDriver;
        
        cfj.slerpDrive = slerpDriver;

        cfj.rotationDriveMode = RotationDriveMode.XYAndZ;

        return cfj;
    }

    void Awake()
    {
        jointMap = new Dictionary<ConfigurableJoint, Transform>();
        startAngles = new Dictionary<ConfigurableJoint, Vector3>();
        startConstraints = new Dictionary<ConfigurableJoint, JointConstraintState>();
        boneMap = new Dictionary<Transform, Transform>();
    }

	void Start () {
        /* Find CharacterJoints on this object and corresponding Transforms on the target */
        //CharacterJoint[] charJoints = GetComponentsInChildren<CharacterJoint>();
        //Transform[] targetXfrms = anchor.GetComponentsInChildren<Transform>();

        Animator localAnimator = GetComponent<Animator>();
        Animator targetAnimator = anchor.GetComponent<Animator>();

        if (localAnimator == null || targetAnimator == null)
            throw new InvalidOperationException("Both local and target GameObjects must have Animator components attached!");

        foreach(HumanBodyBones boneID in Enum.GetValues(typeof(HumanBodyBones)))
        {
            Transform localXfrm = localAnimator.GetBoneTransform(boneID);
            Transform targetXfrm = targetAnimator.GetBoneTransform(boneID);

            if(localXfrm != null && targetXfrm != null)
            {
                if(!boneMap.ContainsKey(localXfrm))
                    boneMap.Add(localXfrm, targetXfrm);

                CharacterJoint joint = localXfrm.gameObject.GetComponent<CharacterJoint>();
                if(joint != null)
                {
                    JointConstraintState initialConstraint = new JointConstraintState();
                    initialConstraint.save(joint);

                    /* Replace the joint with a ConfigurableJoint, copying all appropriate parameters */
                    ConfigurableJoint cfj = copyCharacterJoint(joint);

                    startAngles.Add(cfj, getJointTargetAngles(cfj, cfj.transform));
                    jointMap.Add(cfj, targetXfrm);
                    startConstraints.Add(cfj, initialConstraint);
                }
            }
        }

        /*
        foreach(CharacterJoint joint in charJoints) {
            Transform targetXfrm = Array.Find<Transform>(targetXfrms, x => x.gameObject.name.Equals(joint.gameObject.name));
            if (targetXfrm == null)
                throw new ArgumentNullException("Could not find target transform for joint on object " + joint.gameObject.name + "!");

            // Replace the joint with a ConfigurableJoint, copying all appropriate parameters
            ConfigurableJoint cfj = copyCharacterJoint(joint);

            startAngles.Add(cfj, getJointTargetAngles(cfj, cfj.transform));
            jointMap.Add(cfj, targetXfrm);

        }
        */

        /* Remove the old joints and wake up the rigidbodies if necessary */
        ConfigurableJoint[] cfJoints = GetComponentsInChildren<ConfigurableJoint>();
        foreach(ConfigurableJoint joint in cfJoints)
        {
            Rigidbody rb1 = joint.connectedBody;
            Rigidbody rb2 = joint.gameObject.GetComponent<Rigidbody>();

            CharacterJoint oldJoint = joint.gameObject.GetComponent<CharacterJoint>();
            CharacterJoint.Destroy(oldJoint);

            rb1.AddForce(Vector3.zero);
            rb2.AddForce(Vector3.zero);

            rb1.useGravity = !disableGravity;
            rb2.useGravity = !disableGravity;

            //rb1.isKinematic = true;
            //rb2.isKinematic = true;

        }
    }

    void UpdateJoint(ConfigurableJoint joint)
    {
        Transform target = jointMap[joint];
        Vector3 currentAngles = getJointTargetAngles(joint, joint.transform);
        Vector3 targetAngles = getJointTargetAngles(joint, target);

        //Debug.Log("[" + joint.gameObject.name + "] Current Target Angles: " + targetAngles.ToString());

        /*
        if (targetAngles.x > 180) targetAngles.x -= 360f;
        if (targetAngles.y > 180) targetAngles.y -= 360f;
        if (targetAngles.z > 180) targetAngles.z -= 360f;
        */

        targetAngles -= startAngles[joint]; // Character joint limits are relative to the starting position of the joint
        
        SoftJointLimit newLowLimit = joint.lowAngularXLimit;
        SoftJointLimit newHiLimit = joint.highAngularXLimit;

        newLowLimit.limit = targetAngles.x - twistLimLow;
        newHiLimit.limit = targetAngles.x + twistLimHigh;

        joint.lowAngularXLimit = newLowLimit;
        joint.highAngularXLimit = newHiLimit;

        SoftJointLimit yLimit = joint.angularYLimit;
        SoftJointLimit zLimit = joint.angularZLimit;

        yLimit.limit = targetAngles.y + swing1Lim;
        zLimit.limit = targetAngles.z + swing2Lim;

        joint.angularYLimit = yLimit;
        joint.angularZLimit = zLimit;


        JointDrive axDrive = joint.angularXDrive;
        JointDrive ayzDrive = joint.angularYZDrive;
        JointDrive lxDrive = joint.xDrive;
        JointDrive lyDrive = joint.yDrive;
        JointDrive lzDrive = joint.zDrive;

        axDrive.maximumForce = Mathf.Infinity;
        ayzDrive.maximumForce = Mathf.Infinity;
        lxDrive.maximumForce = Mathf.Infinity;
        lyDrive.maximumForce = Mathf.Infinity;
        lzDrive.maximumForce = Mathf.Infinity;

        joint.angularXDrive = axDrive;
        joint.angularYZDrive = ayzDrive;
        joint.xDrive = lxDrive;
        joint.yDrive = lyDrive;
        joint.zDrive = lzDrive;

        // Remap from world coords to joint local coords
        Vector3 targetRotation = new Vector3();
        Vector3 targetPosition = new Vector3();
        Vector3 tertiaryAxis = Vector3.Cross(joint.axis, joint.secondaryAxis);

        targetRotation = mapToAxis(targetRotation, targetAngles.x, joint.axis);
        targetRotation = mapToAxis(targetRotation, targetAngles.y, joint.secondaryAxis);
        targetRotation = mapToAxis(targetRotation, targetAngles.z, tertiaryAxis);

        targetPosition.x = getFromAxis(target.localPosition, joint.axis);
        targetPosition.y = getFromAxis(target.localPosition, joint.secondaryAxis);
        targetPosition.z = getFromAxis(target.localPosition, tertiaryAxis);

        joint.targetPosition = targetPosition; //target.position;

        //joint.targetRotation = target.localRotation;
        joint.targetRotation = Quaternion.Euler(targetAngles);
        //joint.targetRotation = Quaternion.Euler(targetRotation);

        //joint.transform.localRotation = target.localRotation;
        //joint.transform.localPosition = target.localPosition;


        SoftJointLimitSpring limitSpring = new SoftJointLimitSpring();
        limitSpring.spring = 100f;
        limitSpring.damper = 5f;

        joint.angularXLimitSpring = limitSpring;
        joint.angularYZLimitSpring = limitSpring;

        Rigidbody rb1 = joint.connectedBody;
        Rigidbody rb2 = joint.gameObject.GetComponent<Rigidbody>();

        //joint.transform.rotation = target.rotation; //Quaternion.Euler(targetRotation);
        /*
        joint.connectedBody.velocity = Vector3.zero;
        joint.connectedBody.angularVelocity = Vector3.zero;

        joint.GetComponent<Rigidbody>().velocity = Vector3.zero;
        joint.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        */
    }
    
    // Reset all constraints to initial values
    public void RestoreJoints()
    {
        foreach(ConfigurableJoint joint in startConstraints.Keys)
        {
            startConstraints[joint].restore(joint);

            JointDrive axDrive = joint.angularXDrive;
            JointDrive ayzDrive = joint.angularYZDrive;
            JointDrive lxDrive = joint.xDrive;
            JointDrive lyDrive = joint.yDrive;
            JointDrive lzDrive = joint.zDrive;

            axDrive.maximumForce = axDrive.positionDamper = axDrive.positionSpring = 0;
            ayzDrive.maximumForce = ayzDrive.positionDamper = ayzDrive.positionSpring = 0;
            lxDrive.maximumForce = lxDrive.positionDamper = lxDrive.positionSpring = 0;
            lyDrive.maximumForce = lyDrive.positionDamper = lyDrive.positionSpring = 0;
            lzDrive.maximumForce = lzDrive.positionDamper = lzDrive.positionSpring = 0;

            joint.angularXDrive = axDrive;
            joint.angularYZDrive = ayzDrive;
            joint.xDrive = lxDrive;
            joint.yDrive = lyDrive;
            joint.zDrive = lzDrive;

            SoftJointLimitSpring disabledSpring = new SoftJointLimitSpring();
            disabledSpring.spring = 0f;
            disabledSpring.damper = 50f;

            joint.angularXLimitSpring = disabledSpring;
            joint.angularYZLimitSpring = disabledSpring;
            joint.linearLimitSpring = disabledSpring;

            joint.targetPosition = Vector3.zero;
            joint.targetRotation = Quaternion.identity;

            /*
            joint.connectedBody.velocity = Vector3.zero;
            joint.connectedBody.angularVelocity = Vector3.zero;

            joint.GetComponent<Rigidbody>().velocity = Vector3.zero;
            joint.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            */
        }
    }

    // Update all joint constraints to match target animation
    public void UpdateJoints () {
		foreach(ConfigurableJoint joint in jointMap.Keys)
        {
            UpdateJoint(joint);
        }
	}

    void FixedUpdate() {
        if(active)
            UpdateJoints();
    }
}
