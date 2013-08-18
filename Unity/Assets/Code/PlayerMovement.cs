using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public PlayerInfo Info { get; set; }
    
    public float MaxSpeed = 6.0f, MaxForce = 1.0f;
    
    public void SetInput(Vector2 move, bool doJump)
    {
        Vector3 desiredVelocity = new Vector3(move.x, 0.0f, move.y) * MaxSpeed - rigidbody.velocity;
        desiredVelocity.y = 0.0f;
        
        if (Physics.Raycast(new Ray(transform.position, -transform.up), 1.5f))
            rigidbody.AddForce(Vector3.ClampMagnitude(desiredVelocity * MaxForce, MaxForce), ForceMode.VelocityChange);
        
        //Jumping logic
        if (doJump && Physics.Raycast(new Ray(transform.position, -transform.up), 2.0f))
            rigidbody.AddForce(0.0f, 5.0f - rigidbody.velocity.y, 0.0f, ForceMode.VelocityChange);
    }
}
