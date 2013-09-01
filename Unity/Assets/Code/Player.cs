using UnityEngine;
using Lidgren.Network;
using System.Collections.Generic;

public class PlayerList : NetworkObject
{
    private static string playerName;

    List<string> playerList = new List<string>();
    bool hasListChanged = false;

    public static void SetPlayerName(string playerName)
    {
        DebugConsole.Log("SetPlayerName: " + playerName);
        PlayerList.playerName = playerName;
    }
    
    internal override void OnCreate()
    {
        if (!string.IsNullOrEmpty(playerName) && !playerList.Contains(playerName))
        {
            DebugConsole.Log("Create Player: " + playerName);
            SendMessageToServer(0, msg =>
            {
                msg.Write(playerName);
                return msg;
            });
            NetworkManager.Replicator.Create<Player>();
        }
    }

    internal override bool ShouldSerializeState()
    {
        bool listChanged = hasListChanged;
        hasListChanged = false;
        return listChanged;
    }

    internal override void SerializeState(NetOutgoingMessage msg)
    {
        msg.Write(playerList.Count);
        foreach (string player in playerList)
            msg.Write(player);
    }

    internal override void DeserializeState(NetIncomingMessage msg)
    {
        playerList.Clear();
        int count = msg.ReadInt32();
        for (int i = 0; i<count; i++)
            playerList.Add(msg.ReadString());
    }

    internal override void HandleMessage(byte messageType, NetIncomingMessage msg)
    {
        if (messageType == 0)
        {
            string playerName = msg.ReadString();

            if (!playerList.Contains(playerName))
            {
                playerList.Add(playerName);
                hasListChanged = true;
            }
        }
    }
}

public class Player : NetworkObject
{
    GameObject playerObject = null;
    Transform head = null;

    internal override void OnCreate()
    {
        if (playerObject == null)
            playerObject = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Player"), SpawnPoint.GetSpawnPoint(), Quaternion.identity);

        head = playerObject.transform.FindChild("Head");
        head.camera.enabled = false;
        head.GetComponent<AudioListener>().enabled = false;

        DebugConsole.Log("CreatePlayer");
    }

    internal override void OnSetOwner()
    {
        playerObject.AddComponent<PlayerCamera>();
        head.camera.enabled = true;
        head.GetComponent<AudioListener>().enabled = true;
        DebugConsole.Log("CreatePlayerOwner");
    }

    internal override bool ShouldSerializeControlData()
    {
        return true;
    }

    internal override void SerializeControlData(NetOutgoingMessage msg)
    {
        Vector2 movement = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1.0f);
        Vector3 move = (movement.x * head.right + movement.y * head.forward).normalized;

        msg.Write(new Vector2(move.x, move.z));
        msg.Write(Input.GetButtonDown("Jump"));
        //msg.Write(Input.GetButtonDown("Fire"));
        msg.Write(head.rotation);
    }

    const float MaxSpeed = 6.0f, MaxForce = 1.0f;

    internal override void DeserializeControlData(NetIncomingMessage msg)
    {
        Rigidbody rigidbody = playerObject.rigidbody;
        Transform transform = playerObject.transform;

        Vector2 move = msg.ReadVector2();
        bool doJump = msg.ReadBoolean();
        //bool doFire = msg.ReadBoolean();

        if (!IsMe)
            head.rotation = msg.ReadQuaternion();

        Vector3 desiredVelocity = new Vector3(move.x, 0.0f, move.y) * MaxSpeed - rigidbody.velocity;
        desiredVelocity.y = 0.0f;
        
        if (Physics.Raycast(new Ray(transform.position, -transform.up), 1.5f))
            rigidbody.AddForce(Vector3.ClampMagnitude(desiredVelocity * MaxForce, MaxForce), ForceMode.VelocityChange);
        
        //Jumping logic
        if (doJump && Physics.Raycast(new Ray(transform.position, -transform.up), 2.0f))
            rigidbody.AddForce(0.0f, 5.0f - rigidbody.velocity.y, 0.0f, ForceMode.VelocityChange);
    }

    internal override bool ShouldSerializeState()
    {
        return true;
    }

    internal override void SerializeState(NetOutgoingMessage msg)
    {
        Rigidbody rigidbody = playerObject.rigidbody;

        msg.Write(rigidbody.position);
        msg.Write(rigidbody.velocity);
        msg.Write(head.rotation);
    }

    internal override void DeserializeState(NetIncomingMessage msg)
    {
        Rigidbody rigidbody = playerObject.rigidbody;

        rigidbody.position = msg.ReadVector3();
        rigidbody.velocity = msg.ReadVector3();
        if (!IsMe)
            head.rotation = msg.ReadQuaternion();
    }
}

/*public class PlayerInfo
{
    public PlayerInfo(long ID, long myID, string name, WeaponType type)
    {
        this.ID = ID;
        this.Name = name;
        this.IsMe = ID == myID;
        this.currentWeaponType = type;

        DebugConsole.Log("Create Player: " + ID + ", " + Name + " " + (IsMe ? "IsMe" : ""));
    }

    public long ID { get; private set; }
    public string Name { get; set; }
    public bool IsMe { get; private set; }
    public bool IsRespawning { get { return playerObject == null; } }

    public WeaponType WeaponType { get { return currentWeapon != null ? currentWeapon.Type : WeaponType.None; } }

    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private Transform head;

    private Weapon currentWeapon = null;
    private WeaponType currentWeaponType = WeaponType.None;

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

        SetWeapon(currentWeaponType);
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
            this.currentWeaponType = type;

            if (currentWeapon != null)
            {
                GameObject.Destroy(currentWeapon);
                currentWeapon = null;
            }

            if (type != WeaponType.None && head != null)
            {
                currentWeapon = ((GameObject)GameObject.Instantiate(Resources.Load("Weapons/" + type), Vector3.zero, Quaternion.identity)).GetComponent<Weapon>();
                currentWeapon.transform.parent = head.transform;
                currentWeapon.transform.localPosition = Vector3.zero;
                currentWeapon.transform.localRotation = Quaternion.identity;
            }
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
}*/
