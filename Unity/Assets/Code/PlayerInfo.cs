using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float MaxSpeed = 6.0f, MaxForce = 100.0f;

    public void SetInput(Vector3 move)
    {
        Vector3 desiredVelocity = move * MaxSpeed - rigidbody.velocity;
        Vector3 velocityRelativeToFloor = Vector3.up * Vector3.Dot(desiredVelocity, Vector3.up);
        
        rigidbody.AddForce(Vector3.ClampMagnitude((desiredVelocity - velocityRelativeToFloor) * MaxForce, MaxForce));
    }
}

public class PlayerInfo
{
    public PlayerInfo(long ID, long myID, string name)
    {
        this.ID = ID;
        this.Name = name;
        this.IsMe = ID == myID;

        DebugConsole.Log("Create Player: " + ID + ", " + Name + " " + (IsMe ? "IsMe" : ""));

        playerObject = (GameObject)GameObject.Instantiate(Resources.Load("Player"), Vector3.zero, Quaternion.identity);
        
        //The player camera allows the player to look around, but not move directly.
        if (IsMe)
        {
            playerObject.AddComponent<PlayerCamera>();
            head = playerObject.transform.FindChild("Head");
        }
        else
        {
            GameObject.Destroy(playerObject.transform.FindChild("Head").gameObject);
        }
        
        playerMovement = playerObject.AddComponent<PlayerMovement>();
    }

    public long ID { get; private set; }
    public string Name { get; set; }
    public bool IsMe { get; private set; }

    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private Transform head;

    public void SendTransform(ref Vector3 pos)
    {
        pos = playerObject.transform.position;
    }

    public void SetTransform(Vector3 position)
    {
        playerObject.transform.position = position;
    }

    public void SendMovement(ref Vector3 movement)
    {
        Vector2 move = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1.0f);
        movement = (move.x * head.right + move.y * head.forward);
        playerMovement.SetInput(movement);
    }

    public void SetMovement(Vector3 movement)
    {
        playerMovement.SetInput(movement);
    }

    public void Kill()
    {
        if (playerObject == null)
            return;

        DebugConsole.Log("Remove Player: " + ID + ", " + Name + " " + (IsMe ? "IsMe" : ""));

        GameObject.Destroy(playerObject);
        playerObject = null;
    }
}
