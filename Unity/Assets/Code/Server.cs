using Lidgren.Network;
using UnityEngine;
using System.Collections.Generic;

public abstract class ServerBase
{
    protected NetPeer server;
    protected const int port = 14242;

    public ServerBase(bool isClient)
    {
        NetPeerConfiguration config = new NetPeerConfiguration("7DFPSFTW");
        config.EnableMessageType(isClient ? NetIncomingMessageType.DiscoveryResponse : NetIncomingMessageType.DiscoveryRequest);

        if (isClient)
            server = new NetClient(config);
        else
        {
            config.Port = port;
            server = new NetServer(config);
        }

        server.Start();

        DebugConsole.Log("Net: " + (isClient ? "client" : "server") + " started");
    }

    public void Close()
    {
        DebugConsole.Log("Net: Closing");
        server.Shutdown("Bye");
    }

    public void Poll()
    {
        NetIncomingMessage msg;
        while ((msg = server.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.ErrorMessage:
                    DebugConsole.Log("Net: " + msg.ReadString());
                    break;
                    
                default:
                    if (!HandleMessage(msg))
                        DebugConsole.LogError("Net unhandled type: " + msg.MessageType);
                    break;
            }
            server.Recycle(msg);
        }
    }

    protected abstract bool HandleMessage(NetIncomingMessage msg);
}

public struct GameServer
{
    public string GameName;
    public System.Net.IPEndPoint Endpoint;
}

public class Client : ServerBase
{
    private List<GameServer> discoveredClients = new List<GameServer>();

    public Client() : base(true)
    {
    }

    public List<GameServer> DiscoverClients(bool refresh)
    {
        if (refresh)
            discoveredClients.Clear();

        server.DiscoverLocalPeers(port);

        return discoveredClients;
    }

    public void Join(GameServer host, string playerName)
    {
        DebugConsole.Log("Net.Client: Connect to " + host.Endpoint.ToString() + " as " + playerName);
        discoveredClients.Clear();

        NetOutgoingMessage msg = server.CreateMessage();
        msg.Write(playerName);
        server.Connect(host.Endpoint, msg);
    }

    protected override bool HandleMessage(NetIncomingMessage msg)
    {
        switch (msg.MessageType)
        {
        case NetIncomingMessageType.DiscoveryResponse:
            {
                foreach (GameServer server in discoveredClients)
                    if (server.Endpoint.ToString() == msg.SenderEndPoint.ToString())
                        return true;

                discoveredClients.Add(new GameServer()
                {
                    Endpoint = msg.SenderEndPoint,
                    GameName = msg.ReadString()
                });
            }
            return true;
        }

        return false;
    }
}

public class Server : ServerBase
{
    string gameName;

    public Server(string gameName) : base(false)
    {
        this.gameName = gameName;
    }

    protected override bool HandleMessage(NetIncomingMessage msg)
    {
        switch (msg.MessageType)
        {
            case NetIncomingMessageType.DiscoveryRequest:
                {
                    NetOutgoingMessage response = server.CreateMessage();
                    response.Write(gameName);
                    server.SendDiscoveryResponse(response, msg.SenderEndPoint);
                }
                return true;

            case NetIncomingMessageType.StatusChanged:
                {
                    
                }
                return true;
        }

        return false;
    }
}
