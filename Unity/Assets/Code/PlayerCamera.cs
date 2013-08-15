using UnityEngine;
using System.Collections;

/// <summary>
/// Implements the first person player camera movement.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerCamera : MonoBehaviour 
{
	private float upAngle, rotateAngle;
    public Transform Head { get; private set; }

	void Start()
	{
        Head = transform.FindChild("Head");
        Head.camera.enabled = true;
		Screen.lockCursor = true;
	}

	void Update()
	{
		upAngle = Mathf.Clamp(upAngle - Input.GetAxis("Mouse Y"), -90.0f, 90.0f);
		rotateAngle += Input.GetAxis("Mouse X");
		Head.rotation = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Quaternion.AngleAxis(upAngle, Vector3.right);
	}
}
