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
    }

    public long ID { get; private set; }
    public string Name { get; set; }
    public bool IsMe { get; private set; }

    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private Transform head;

    void SpawnPlayerObject()
    {
        if (playerObject == null)
        {
            playerObject = (GameObject)GameObject.Instantiate(Resources.Load("Player"), Vector3.zero, Quaternion.identity);

            //The player camera allows the player to look around, but not move directly.
            if (IsMe)
            {
                playerObject.AddComponent<PlayerCamera>();
                head = playerObject.transform.FindChild("Head");
            }

            playerMovement = playerObject.AddComponent<PlayerMovement>();
        }
    }

    private Vector3 lastPos = Vector3.zero;

    public bool SendTransform(ref Vector3 pos)
    {
        SpawnPlayerObject();
        if ((lastPos - pos).sqrMagnitude > 0.2f)
        {
            pos = playerObject.transform.position;
            lastPos = pos;
            return true;
        }
        else return false;
    }

    public void SetTransform(Vector3 position)
    {
        SpawnPlayerObject();
        playerObject.transform.position = position;
    }

    public void SendMovement(ref Vector3 movement)
    {
        SpawnPlayerObject();

        Vector2 move = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1.0f);
        movement = (move.x * head.right + move.y * head.forward);
        playerMovement.SetInput(movement);
    }

    public void SetMovement(Vector3 movement)
    {
        SpawnPlayerObject();
        playerMovement.SetInput(movement);
    }

    public void Kill()
    {
        if (playerObject == null)
            return;

        GameObject.Destroy(playerObject);
        playerObject = null;
    }
}
