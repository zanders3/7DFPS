using System;
using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

public enum MessageTypes
{
    //Client -> Server
    SetPlayerInfo,
    SendPlayerInput,

    //Server -> Client
    SetPlayerTransform,
    KillPlayer,

    SendPlayerList
}

public class Client : ServerBase
{
    public List<PlayerInfo> PlayerList { get { return playerList; } }

    private List<PlayerInfo> playerList = new List<PlayerInfo>();
    private PlayerInfo me = null;
    private string playerName;

    public Client(string playerName) : base(true, string.Empty)
    {
        this.playerName = playerName;
    }

    protected override void HandleMessage(MessageTypes type, NetIncomingMessage msg)
    {
        switch (type)
        {
            case MessageTypes.SendPlayerList:
                playerList.Clear();
                int count = msg.ReadByte();
                for (int i = 0; i<count; i++)
                {
                    PlayerInfo playerInfo = new PlayerInfo(msg.ReadInt64(), this.PlayerID, msg.ReadString());
                    if (playerInfo.IsMe)
                        me = playerInfo;
                    playerList.Add(playerInfo);
                }

                DebugConsole.Log("Net.Client: Sent Player List: " + playerList.Count);
                break;

            case MessageTypes.SetPlayerTransform:
                long playerID = msg.ReadInt64();
                Vector3 pos = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());

                for (int i = 0; i<playerList.Count; i++)
                    if (playerList[i].ID == playerID)
                        playerList[i].SetTransform(pos);
                break;

            default:
                DebugConsole.LogError("Unknown client message type: " + type);
                break;
        }
    }

    public override void Poll()
    {
        base.Poll();

        if (me != null)
        {
            Vector3 movement = Vector3.zero;
            me.SendMovement(ref movement);

            var msg = CreateMessage(MessageTypes.SendPlayerInput);
            msg.Write(movement.x); msg.Write(movement.y); msg.Write(movement.z);
            SendMessage(msg, NetDeliveryMethod.ReliableSequenced);
        }
    }

    protected override void OnConnected(long playerID)
    {
        LevelManager.LoadLevel();

        var msg = CreateMessage(MessageTypes.SetPlayerInfo);
        msg.Write(playerName);
        SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
    }

    protected override void OnDisconnected(long playerID)
    {
        LevelManager.ClearLevel();
    }
}
