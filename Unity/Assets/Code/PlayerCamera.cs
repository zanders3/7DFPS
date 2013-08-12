using UnityEngine;
using System.Collections;

/// <summary>
/// Implements the first person player camera
/// </summary>
public class PlayerCamera : MonoBehaviour 
{
	private float upAngle, rotateAngle;

	void Start()
	{
		Screen.lockCursor = true;
	}

	void Update()
	{
		upAngle = Mathf.Clamp(upAngle + Input.GetAxis("Vertical"), 0.1f, Mathf.PI - 0.1f);
		rotateAngle = Mathf.DeltaAngle(rotateAngle, Input.GetAxis("Horizontal"));

		transform.rotation = Quaternion.AngleAxis(upAngle, Vector3.right) * Quaternion.AngleAxis(rotateAngle, Vector3.up);
	}
}
