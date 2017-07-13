using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    [System.Serializable]
    public class LinearSpring
    {
        public Rigidbody actingBody;
        public Vector3 springAxis;
        public float targetPosition;
        public float stiffness;
        public float dampener;

        public LinearSpring(Rigidbody rb, Vector3 axis)
        {
            actingBody = rb;
            springAxis = axis;
        }

        public void FixedUpdate()
        {
            float displacement = 0f;
            float velocity = 0f;

            if (Mathf.Abs(springAxis.x) > 0.1f) displacement = actingBody.position.x;
            else if (Mathf.Abs(springAxis.y) > 0.1f) displacement = actingBody.position.y;
            else if (Mathf.Abs(springAxis.z) > 0.1f) displacement = actingBody.position.z;

            if (Mathf.Abs(springAxis.x) > 0.1f) velocity = actingBody.velocity.x;
            else if (Mathf.Abs(springAxis.y) > 0.1f) velocity = actingBody.velocity.y;
            else if (Mathf.Abs(springAxis.z) > 0.1f) velocity = actingBody.velocity.z;

            displacement = targetPosition - displacement;

            float forceMagnitude = (displacement * stiffness) + (velocity * dampener);
            actingBody.AddForce(springAxis * forceMagnitude);
        }
    }

    [System.Serializable]
    public class AngularSpring
    {
        public Rigidbody actingBody;
        public Vector3 springAxis;
        public float targetPosition;
        public float stiffness;
        public float dampener;

        public AngularSpring(Rigidbody rb, Vector3 axis)
        {
            actingBody = rb;
            springAxis = axis;
        }

        public void FixedUpdate()
        {
            float displacement = 0f;
            float velocity = 0f;

            if (Mathf.Abs(springAxis.x) > 0.1f) displacement = actingBody.rotation.x;
            else if (Mathf.Abs(springAxis.y) > 0.1f) displacement = actingBody.rotation.y;
            else if (Mathf.Abs(springAxis.z) > 0.1f) displacement = actingBody.rotation.z;

            if (Mathf.Abs(springAxis.x) > 0.1f) velocity = actingBody.angularVelocity.x;
            else if (Mathf.Abs(springAxis.y) > 0.1f) velocity = actingBody.angularVelocity.y;
            else if (Mathf.Abs(springAxis.z) > 0.1f) velocity = actingBody.angularVelocity.z;

            displacement = targetPosition - displacement;

            float forceMagnitude = (displacement * stiffness) + (velocity * dampener);
            actingBody.AddTorque(springAxis * forceMagnitude);
        }
    }
}
