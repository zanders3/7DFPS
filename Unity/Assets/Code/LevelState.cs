using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Player
{
    public string Guid { get; private set; }
    public string Name { get; private set; }
    public int Score { get; private set; }

    public Player(string guid, string playerName)
    {
        Guid = guid;
        Name = playerName;
        Score = 0;
    }
}

/// <summary>
/// Tracks the player list and score.
/// </summary>
public class LevelState : MonoBehaviour
{
    Dictionary<string, Player> guidToPlayer = new Dictionary<string, Player>();
    List<Player> players = new List<Player>();

    public static LevelState Instance = null;

    public List<Player> Players
    {
        get { return players; }
    }

    void Start()
    {
        Instance = this;
        Debug.Log("LevelState start");
        networkView.RPC("AddPlayer", RPCMode.AllBuffered, Network.player.guid, FrontEnd.Instance.mPlayerName);
    }

    [RPC]
    void AddPlayer(string guid, string playerName)
    {
        Debug.Log("Add player: " + guid + ", " + playerName);
        if (!guidToPlayer.ContainsKey(guid))
            guidToPlayer.Add(guid, new Player(guid, playerName));

        players = guidToPlayer.Values.ToList();
    }
    
    void OnPlayerDisconnected(NetworkPlayer zPlayer)
    {
        Debug.Log("Remove player: " + zPlayer.guid);

        guidToPlayer.Remove(zPlayer.guid);

        players = guidToPlayer.Values.ToList();

        Network.DestroyPlayerObjects(zPlayer);
    }
}
