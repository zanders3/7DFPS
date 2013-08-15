using UnityEngine;
using System.Collections;

/// <summary>
/// Implements the first person player camera and player movement.
/// Sends position to the server.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerCamera : MonoBehaviour 
{
	private float upAngle, rotateAngle;
    public Transform Head { get; private set; }

	void Start()
	{
        gameObject.AddComponent<Camera>();

        Head = transform.FindChild("Head");
		Screen.lockCursor = true;
	}

	void Update()
	{
		upAngle = Mathf.Clamp(upAngle - Input.GetAxis("Mouse Y"), -90.0f, 90.0f);
		rotateAngle += Input.GetAxis("Mouse X");
		Head.rotation = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Quaternion.AngleAxis(upAngle, Vector3.right);
	}
}
