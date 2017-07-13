using UnityEngine;
using System.Collections;
using Assets;

public class PhysicsRagdollKeepHeadUpright : MonoBehaviour
{
    public Rigidbody head;
    public AngularSpring headAngularXSpring;
    public AngularSpring headAngularZSpring;
    public float headUpForce = 30f;

    private Collider headCollider;
    private Vector3 headTop;

    public bool drawDebugRays = false;

    // Use this for initialization
    void Start()
    {
        headCollider = head.GetComponent<Collider>();
        headAngularXSpring.actingBody = headAngularZSpring.actingBody = head;
    }
    
    void FixedUpdate()
    {
        headAngularXSpring.springAxis = Vector3.right;
        headAngularZSpring.springAxis = Vector3.forward;

        headTop = headCollider.bounds.center;
        headTop.y += headCollider.bounds.size.y / 2f;

        if (drawDebugRays) Debug.DrawRay(headTop, Vector3.up, Color.gray);

        headAngularXSpring.FixedUpdate();
        headAngularZSpring.FixedUpdate();
        head.AddForceAtPosition(Vector3.up * headUpForce, headTop);
    }
}
