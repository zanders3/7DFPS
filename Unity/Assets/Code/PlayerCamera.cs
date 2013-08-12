using UnityEngine;
using System.Collections;

/// <summary>
/// Implements the first person player camera and player movement.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerCamera : MonoBehaviour 
{
	private float upAngle, rotateAngle;

	public float MaxSpeed = 6.0f, MaxForce = 100.0f, JumpVel = 1.0f;
	public Transform Head;

	private PlayerState state = PlayerState.InAir;
	private float jumpTimer = 0.0f;

	private enum PlayerState
	{
		TouchingGround,
		Jumping,
		InAir
	};

	void Start()
	{
		Screen.lockCursor = true;
	}

	void Update()
	{
		upAngle = Mathf.Clamp(upAngle - Input.GetAxis("Mouse Y"), -90.0f, 90.0f);
		rotateAngle += Input.GetAxis("Mouse X");
		Head.rotation = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Quaternion.AngleAxis(upAngle, Vector3.right);

		RaycastHit hitInfo;
		bool isTouchingGround = Physics.Raycast (new Ray (transform.position, Vector3.down), out hitInfo, 2.0f);

		//Player jumping logic
		switch (state)
		{
		case PlayerState.InAir:
			if (isTouchingGround && !Input.GetButtonDown("Jump"))
			{
				Debug.Log("TouchingGround");
				state = PlayerState.TouchingGround;
			}
			break;

		case PlayerState.TouchingGround:
			if (!isTouchingGround)
			{
				state = PlayerState.InAir;
			}
			else if (Input.GetButtonDown("Jump") && jumpTimer <= 0.0f)
			{
				Debug.Log("Jump");
				rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.Impulse);
				state = PlayerState.InAir;
				jumpTimer = 1.0f;
			}
			break;
		}

		jumpTimer -= Time.deltaTime;

		//Player movement logic
		switch (state)
		{
		case PlayerState.Jumping:
		case PlayerState.TouchingGround:
			Vector2 move = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1.0f);
			
			Vector3 desiredVelocity = (move.x * Head.right + move.y * Head.forward) * MaxSpeed - rigidbody.velocity;
			Vector3 velocityRelativeToFloor = Vector3.up * Vector3.Dot(desiredVelocity, Vector3.up);
			
			rigidbody.AddForce(Vector3.ClampMagnitude((desiredVelocity - velocityRelativeToFloor) * MaxForce, MaxForce));
			break;
		}
	}
}
