using UnityEngine;

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
    private Weapon currentWeapon = null;

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

    public bool SendTransform(ref Vector3 pos, ref Vector3 vel)
    {
        if (playerObject == null)
            return false;

        pos = playerObject.transform.position;
        vel = playerObject.rigidbody.velocity;
        return true;
    }

    public void SetTransform(Vector3 position, Vector3 velocity)
    {
        if (playerObject == null)
            SpawnPlayer();

        playerObject.transform.position = Vector3.Lerp(playerObject.transform.position, position, 0.5f);
        playerObject.rigidbody.velocity = Vector3.Lerp(playerObject.rigidbody.velocity, velocity, 0.5f);
    }

    public bool SendMovement(ref Vector2 movement, ref bool doJump, ref bool fireWeapon)
    {
        if (playerObject == null)
            return false;

        movement = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1.0f);
        Vector3 move = (movement.x * head.right + movement.y * head.forward).normalized;
        movement = new Vector2(move.x, move.z);
        doJump = Input.GetButtonDown("Jump");
        fireWeapon = currentWeapon != null && Input.GetButtonDown("Fire1");

        playerMovement.SetInput(movement, doJump);

        return true;
    }

    public void SetWeapon(WeaponType type)
    {
        if (currentWeapon == null || currentWeapon.Type != type)
        {
            if (currentWeapon != null)
            {
                GameObject.Destroy(currentWeapon);
                currentWeapon = null;
            }

            currentWeapon = ((GameObject)GameObject.Instantiate(Resources.Load("Weapons/" + type), Vector3.zero, Quaternion.identity)).GetComponent<Weapon>();
            currentWeapon.transform.parent = head.transform;
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }

    public void FireWeapon()
    {
        DebugConsole.Log("FireWeapon");
        if (currentWeapon != null)
            currentWeapon.Fire(new Ray(head.position, head.forward));
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
