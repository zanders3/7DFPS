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

public class PlayerInfo
{
    public PlayerInfo(long ID, long myID, string name)
    {
        this.ID = ID;
        this.Name = name;
        this.IsMe = ID == myID;

        DebugConsole.Log("Create Player: " + ID + ", " + Name + " " + (IsMe ? "IsMe" : ""));
    }

    public long ID { get; private set; }
    public string Name { get; set; }
    public bool IsMe { get; private set; }
    public bool IsRespawning { get { return playerObject == null; } }

    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private Transform head;

    public void SpawnPlayer()
    {
        playerObject = (GameObject)GameObject.Instantiate(Resources.Load("Player"), SpawnPoint.GetSpawnPoint(), Quaternion.identity);
        
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
        playerMovement.Info = this;
    }

    public bool SendTransform(ref Vector3 pos)
    {
        if (playerObject == null)
            return false;

        pos = playerObject.transform.position;
        return true;
    }

    public void SetTransform(Vector3 position)
    {
        if (playerObject == null)
            SpawnPlayer();

        playerObject.transform.position = position;
    }

    public bool SendMovement(ref Vector2 movement, ref bool doJump)
    {
        if (playerObject == null)
            return false;

        movement = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1.0f);
        Vector3 move = (movement.x * head.right + movement.y * head.forward).normalized;
        movement = new Vector2(move.x, move.z);
        doJump = Input.GetButtonDown("Jump");

        playerMovement.SetInput(movement, doJump);
        return true;
    }

    public void SetMovement(Vector2 movement, bool doJump)
    {
        if (playerObject == null)
            return;

        playerMovement.SetInput(movement, doJump);
    }

    public void Remove()
    {
        if (playerObject == null)
            return;

        DebugConsole.Log("Remove Player: " + ID + ", " + Name + " " + (IsMe ? "IsMe" : ""));

        GameObject.Destroy(playerObject);
        playerObject = null;
        head = null;
        playerMovement = null;
    }
}
