using UnityEngine;
using Lidgren.Network;
using System.Collections.Generic;

public class Player : NetworkObject
{
    public static List<Player> Players = new List<Player>();

    GameObject playerObject = null;
    Transform head = null;
    string playerName;
    float spawnTimer = 3.0f;

    public int SpawnTimer { get { return (int)spawnTimer; } }
    public string Name { get { return playerName; } }

    public static void Create(string playerName)
    {
        NetworkManager.Replicator.Create<Player>(msg =>
        {
            msg.Write(playerName);
            return msg;
        });
    }

    internal override void OnCreate(NetIncomingMessage msg)
    {
        playerName = msg.ReadString();

        DebugConsole.Log("CreatePlayer: " + playerName + "(" + IsMe + ")");
        Players.Add(this);
    }

    internal override void OnDestroy()
    {
        if (playerObject != null)
        {
            GameObject.Destroy(playerObject);
            playerObject = null;
        }

        Players.Remove(this);
    }

    internal override bool ShouldSerializeControlData()
    {
        return playerObject != null;
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
        if (spawnTimer > 0.0f)
        {
            msg.Write(true);
            msg.Write(spawnTimer);
        }
        else
        {
            Rigidbody rigidbody = playerObject.rigidbody;
            msg.Write(false);
            msg.Write(rigidbody.position);
            msg.Write(rigidbody.velocity);
            msg.Write(head.rotation);
        }
    }

    internal override void DeserializeState(NetIncomingMessage msg)
    {
        bool waitingForSpawn = msg.ReadBoolean();
        if (waitingForSpawn)
        {
            KillPlayer();

            this.spawnTimer = msg.ReadSingle();
        }
        else
        {
            CreatePlayer();

            Rigidbody rigidbody = playerObject.rigidbody;

            rigidbody.position = msg.ReadVector3();
            rigidbody.velocity = msg.ReadVector3();
            if (!IsMe)
                head.rotation = msg.ReadQuaternion();
        }
    }

    internal override void Update()
    {
        DebugConsole.Log("Player Update: " + spawnTimer);
        if (NetworkManager.IsServer)
        {
            if (spawnTimer > 0.0f)
                spawnTimer -= Time.deltaTime;

            if (spawnTimer > 0.0f)
                KillPlayer();
            else
                CreatePlayer();
        }
    }

    void CreatePlayer()
    {
        if (playerObject != null)
            return;

        playerObject = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("Player"), SpawnPoint.GetSpawnPoint(), Quaternion.identity);

        playerObject.AddComponent<PlayerScript>().Player = this;

        head = playerObject.transform.FindChild("Head");
        head.camera.enabled = false;
        head.GetComponent<AudioListener>().enabled = false;
        
        if (IsMe)
        {
            playerObject.AddComponent<PlayerCamera>();
            head.camera.enabled = true;
            head.GetComponent<AudioListener>().enabled = true;
        }
    }

    void KillPlayer()
    {
        if (playerObject == null)
            return;

        GameObject.Destroy(playerObject);
        playerObject = null;
    }

    public void Kill()
    {
        if (NetworkManager.IsServer)
        {
            spawnTimer = 3.0f;
        }
    }
}
