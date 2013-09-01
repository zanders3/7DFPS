using UnityEngine;
using Lidgren.Network;

public class SendPlayerInput : INetMessage
{
    public NetDeliveryMethod DeliveryMethod { get { return NetDeliveryMethod.UnreliableSequenced; } }

    public long ID;
    public Vector2 Move;
    public bool Jump;
    public bool FireWeapon;
    
    public void ToNetwork(ref NetOutgoingMessage msg)
    {
        msg.Write(ID);
        msg.Write(Move);
        msg.Write(Jump);
        msg.Write(FireWeapon);
    }
    
    public void FromNetwork(NetIncomingMessage msg)
    {
        ID = msg.ReadInt64();
        Move = msg.ReadVector2();
        Jump = msg.ReadBoolean();
        FireWeapon = msg.ReadBoolean();
    }
}

/*[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public PlayerClient Client { get; set; }
    
    public float MaxSpeed = 6.0f, MaxForce = 1.0f;

    void Start()
    {
    }

    void Update()
    {
        if (Client.ID == NetworkManager.MyID)
        {
            var msg = NetworkManager.CreateMessage<SendPlayerInput>();
            msg.ID = Client.ID;
            msg.Move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            msg.FireWeapon = Input.GetButtonDown("Fire1");
            msg.Jump = Input.GetButtonDown("Jump");
            NetworkManager.SendMessageToServer(msg);
        }
    }

    public void SetInput(SendPlayerInput input)
    {
        Vector3 desiredVelocity = new Vector3(move.x, 0.0f, move.y) * MaxSpeed - rigidbody.velocity;
        desiredVelocity.y = 0.0f;
        
        if (Physics.Raycast(new Ray(transform.position, -transform.up), 1.5f))
            rigidbody.AddForce(Vector3.ClampMagnitude(desiredVelocity * MaxForce, MaxForce), ForceMode.VelocityChange);
        
        //Jumping logic
        if (doJump && Physics.Raycast(new Ray(transform.position, -transform.up), 2.0f))
            rigidbody.AddForce(0.0f, 5.0f - rigidbody.velocity.y, 0.0f, ForceMode.VelocityChange);
    }
}*/
