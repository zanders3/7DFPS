using Lidgren.Network;
using UnityEngine;
using System.Collections.Generic;

public class Server : ServerBase
{   
    public List<PlayerInfo> PlayerList { get { return PlayerList; } }

    private List<PlayerInfo> playerList = new List<PlayerInfo>();
    private List<KeyValuePair<PlayerInfo, float>> pendingSpawnList = new List<KeyValuePair<PlayerInfo, float>>();
    private PlayerInfo me = null;
    
    public Server(string playerName, string gameName) : base(false, gameName)
    {
        LevelManager.LoadLevel();

        me = new PlayerInfo(PlayerID, PlayerID, playerName);
        playerList.Add(me);
        pendingSpawnList.Add(new KeyValuePair<PlayerInfo, float>(me, 0.0f));
        UpdatePlayerList();
    }

    const float SpawnTimeSeconds = 3.0f;

    public override void Poll()
    {
        base.Poll();

        Vector2 move = Vector2.zero;
        bool doJump = false;
        me.SendMovement(ref move, ref doJump);

        Vector3 pos = Vector3.zero;
        for (int i = 0; i<playerList.Count; i++)
        {
            if (playerList[i].SendTransform(ref pos))
            {
                var msg = CreateMessage(MessageTypes.SetPlayerTransform);
                msg.Write(playerList[i].ID);
                msg.Write(pos.x); msg.Write(pos.y); msg.Write(pos.z);
                SendMessage(msg, NetDeliveryMethod.ReliableSequenced);
            }
        }

        for (int i = 0; i<pendingSpawnList.Count; i++)
        {
            pendingSpawnList[i] = new KeyValuePair<PlayerInfo, float>(pendingSpawnList[i].Key, pendingSpawnList[i].Value + Time.deltaTime);
            if (pendingSpawnList[i].Value > SpawnTimeSeconds)
            {
                DebugConsole.Log("Net.Server: Spawning player");
                pendingSpawnList[i].Key.SpawnPlayer();
                pendingSpawnList.RemoveAt(i);
                i--;
            }
        }

    }

    public void KillPlayer(PlayerInfo player)
    {
        if (!player.IsRespawning)
        {
            DebugConsole.Log("Net.Server: Kill player " + player.Name);
            var msg = CreateMessage(MessageTypes.KillPlayer);
            msg.Write(player.ID);
            SendMessage(msg, NetDeliveryMethod.ReliableSequenced);

            player.Remove();
        }
    }

    void UpdatePlayerList()
    {
        var msg = CreateMessage(MessageTypes.SendPlayerList);
        msg.Write((byte)playerList.Count);
        foreach (var player in playerList)
        {
            msg.Write(player.ID);
            msg.Write(player.Name);
        }
        SendMessage(msg, NetDeliveryMethod.ReliableSequenced);
    }
    
    protected override void HandleMessage(MessageTypes type, NetIncomingMessage msg)
    {
        switch (type)
        {
            case MessageTypes.SetPlayerInfo:
                string playerName = msg.ReadString();
                long playerID = msg.SenderConnection.RemoteUniqueIdentifier;

                DebugConsole.Log("SetPlayerInfo: " + playerName + " " + playerID);

                bool alreadyExists = playerList.Find(p => p.ID == playerID) != null;

                if (alreadyExists)
                {
                    DebugConsole.Log("Player already exists!");
                }
                else
                {
                    playerList.Add(new PlayerInfo(playerID, this.PlayerID, playerName));
                    pendingSpawnList.Add(new KeyValuePair<PlayerInfo, float>(playerList[playerList.Count - 1], 0.0f));

                    UpdatePlayerList();
                }
                break;

            case MessageTypes.SendPlayerInput:
                Vector3 move = new Vector2(msg.ReadFloat(), msg.ReadFloat());
                bool doJump = msg.ReadBoolean();
                for (int i = 0; i<playerList.Count; i++)
                    if (playerList[i].ID == msg.SenderConnection.RemoteUniqueIdentifier)
                    {
                        playerList[i].SetMovement(move, doJump);
                        break;
                    }
                break;
                
            default:
                DebugConsole.LogError("Unknown server message type: " + type);
                break;
        }
    }
    
    protected override void OnDisconnected(long playerID)
    {
        if (playerID == this.PlayerID)
        {
            foreach (var player in playerList)
                player.Remove();
            playerList.Clear();

            Frontend.SetState(FrontendState.Title);
        } 
        else
        {
            foreach (var player in playerList)
            {
                if (player.ID == playerID)
                {
                    playerList.Remove(player);
                    player.Remove();

                    UpdatePlayerList();
                
                    return;
                }
            }
        }
    }
}
