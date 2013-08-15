using Lidgren.Network;
using UnityEngine;
using System.Collections.Generic;

public class Server : ServerBase
{   
    public List<PlayerInfo> PlayerList { get { return PlayerList; } }

    private List<PlayerInfo> playerList = new List<PlayerInfo>();
    private PlayerInfo me = null;
    
    public Server(string playerName, string gameName) : base(false, gameName)
    {
        LevelManager.LoadLevel();

        me = new PlayerInfo(PlayerID, PlayerID, playerName);
        playerList.Add(me);
        UpdatePlayerList();
    }
    
    public override void Poll()
    {
        base.Poll();

        Vector2 move = Vector2.zero;
        bool doJump = false;
        me.SendMovement(ref move, ref doJump);

        Vector3 pos = Vector3.zero;
        for (int i = 0; i<playerList.Count; i++)
        {
            playerList[i].SendTransform(ref pos);
            {
                var msg = CreateMessage(MessageTypes.SetPlayerTransform);
                msg.Write(playerList[i].ID);
                msg.Write(pos.x); msg.Write(pos.y); msg.Write(pos.z);
                SendMessage(msg, NetDeliveryMethod.ReliableSequenced);
            }
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
