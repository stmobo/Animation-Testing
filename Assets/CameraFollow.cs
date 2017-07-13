using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public GameObject player;
    //private Rigidbody playerRB;

    private Vector3 cameraOffset;
    private Quaternion cameraStartRotation;
    private Quaternion target;

    private float rotateSpeed = 2.0f;

    private const float fovX = 360.0f;
    private const float fovY = 60.0f;

    private float mouseSensX = 10.0f;
    private float mouseSensY = 10.0f;



    float rotX = 0.0f;
    float rotY = 0.0f;

	// Use this for initialization
	void Start () {
        //player = GameObject.FindWithTag("Player");
        //playerRB = player.GetComponent<Rigidbody>();

        cameraOffset = transform.position - player.transform.position;
        cameraStartRotation = transform.rotation;

        target = Quaternion.Euler(Vector3.zero);

	}

    public static float clampAngle(float angle, float min, float max)
    {
        if (angle > 360.0f)
            angle -= 360.0f;

        if (angle < -360.0f)
            angle += 360.0f;

        return Mathf.Clamp(angle, min, max);
    }
	
	void LateUpdate () {
        transform.position = player.transform.position + cameraOffset;
        //transform.RotateAround(player.transform.position, Vector3.up, player.transform.rotation.eulerAngles.y);

        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        Vector3 lookPoint = player.transform.position;
        lookPoint.y += 0.5f;

        if (Mathf.Abs(scrollWheel) > 0)
        {
            Vector3 diff = transform.position - lookPoint;
            cameraOffset -= (diff * scrollWheel);
        }

        if (Input.GetButton("MoveCamera"))
        {
            rotX = clampAngle(rotX + (Input.GetAxis("Mouse X") * mouseSensX), -fovX, fovX);
            rotY = clampAngle(rotY + (Input.GetAxis("Mouse Y") * mouseSensY), -fovY, fovY);
        }

        transform.RotateAround(lookPoint, Vector3.up, rotX);

        Vector3 yRotAxis = Vector3.Cross(lookPoint - transform.position, Vector3.up);
        transform.RotateAround(lookPoint, yRotAxis, rotY);

        Quaternion lookRot = Quaternion.LookRotation(lookPoint - transform.position);
        transform.rotation = lookRot;

        
    }
}
