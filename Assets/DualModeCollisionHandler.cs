using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualModeCollisionHandler : MonoBehaviour {
    public DualAnimationController animCtrl;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision, magnitude " + collision.relativeVelocity.magnitude.ToString());

        animCtrl.matchedPoseMode();
        if (collision.relativeVelocity.magnitude > 1.5f)
        {
            animCtrl.Invoke("physicsMode", 1f);
        }
        animCtrl.Invoke("animatedMode", 5f);
    }

	// Use this for initialization
	void Start () {
        animCtrl = GetComponent<DualAnimationController>();
	}
}
