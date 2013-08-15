using UnityEngine;
using System.Collections.Generic;
/*
[RequireComponent(typeof(NetworkView))]
public class PlayerClient : MonoBehaviour
{
    public static List<string> PlayerNames = new List<string>();

    private NetworkPlayer owner;
    private string name;

    void Update()
    {
        if (owner == Network.player)
        {
            //TODO: send player movement commands
        }

        if (Network.isServer)
        {
            //TODO: apply player movement
        }
    }

    void OnDestroy()
    {
        PlayerNames.Remove(name);
    }

    public NetworkPlayer GetOwner()
    {
        return owner;
    }

    public void SetOwner(NetworkPlayer player, string name)
    {
        networkView.RPC("RPC_SetOwner", RPCMode.AllBuffered, player, name);
    }

    [RPC]
    void RPC_SetOwner(NetworkPlayer player, string name)
    {
        this.owner = player;
        this.name = name;
        PlayerNames.Add(name);
    }
}

public abstract class NetworkManager
{
    private NetworkView networkView;

    public NetworkManager(NetworkView networkView)
    {
        this.networkView = networkView;
    }

    public virtual void SpawnPlayer(NetworkPlayer player, string playerName)
    {
    }

    public virtual void OnPlayerConnected(NetworkPlayer player)
    {
    }

    public virtual void OnPlayerDisconnected(NetworkPlayer player)
    {
    }

    public virtual void OnDisconnectedFromServer(NetworkDisconnection info)
    {
    }

    public virtual void OnGUI() 
    {
    }

    protected void RPC(string methodName, RPCMode mode, params object[] args)
    {
        networkView.RPC(methodName, mode, args);
    }
}

public class ServerNetworkManager : ClientNetworkManager
{
    private List<PlayerClient> players = new List<PlayerClient>();

    public ServerNetworkManager(NetworkView view) : base(view)
    {
    }

    public override void SpawnPlayer(NetworkPlayer player, string playerName)
    {
        base.SpawnPlayer(player, playerName);

        GameObject playerObject = (GameObject)Network.Instantiate(Resources.Load("Player"), Vector3.zero, Quaternion.identity, 0);
        playerObject.GetComponent<PlayerClient>().SetOwner(player, playerName);
        players.Add(playerObject.GetComponent<PlayerClient>());
    }

    public override void OnPlayerDisconnected(NetworkPlayer player)
    {
        base.OnPlayerDisconnected(player);

        Network.RemoveRPCs(player);

        PlayerClient clientToRemove = players.Find(p => p.GetOwner().guid == player.guid);
        if (clientToRemove == null)
            DebugConsoleError("Failed to find player: " + player.guid + ", " + players.Count);

        Network.RemoveRPCs(clientToRemove.networkView.viewID);
        Network.Destroy(clientToRemove.gameObject);
    }
}

public class ClientNetworkManager : NetworkManager
{
    public ClientNetworkManager(NetworkView view) : base(view)
    {
        RPC("SpawnPlayer", RPCMode.AllBuffered, Network.player, Frontend.PlayerName);
    }
}

[RequireComponent(typeof(NetworkView))]
public class NetworkScript : MonoBehaviour
{
    private NetworkManager NetworkManager = null;
    private bool requestedHostList = false;

    public void HostGame(string gameName)
    {
        Network.InitializeServer(8, 3000, true);
        MasterServer.RegisterHost("7DFPSFTW", gameName);
        NetworkManager = new ServerNetworkManager(networkView);
    }

    public void JoinGame(HostData host)
    {
        Network.Connect(host);
        NetworkManager = new ClientNetworkManager(networkView);
    }

    public HostData[] PollGames(bool refresh)
    {
        if (!requestedHostList || refresh)
        {
            MasterServer.RequestHostList("7DFPSFTW");
            requestedHostList = true;
        }

        return MasterServer.PollHostList();
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        NetworkManager.OnPlayerConnected(player);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        NetworkManager.OnPlayerDisconnected(player);
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        DebugConsole.Log("Disconnected from server: " + info);
        NetworkManager.OnDisconnectedFromServer(info);
        NetworkManager = null;
    }

    [RPC]
    void SpawnPlayer(NetworkPlayer player, string playerName)
    {
        DebugConsole.Log("Spawn player: " + player + ", " + playerName);
        NetworkManager.SpawnPlayer(player, playerName);
    }
}*/

