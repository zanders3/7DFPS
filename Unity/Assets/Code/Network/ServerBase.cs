using System;
using Lidgren.Network;
using System.Collections.Generic;

public struct GameServer
{
    public string GameName;
    public System.Net.IPEndPoint Endpoint;
}

public abstract class ServerBase
{
    public static bool IsClient { get { return isClient; } }

    private List<GameServer> discoveredClients = new List<GameServer>();
    private string gameName;
    private static bool isClient;

    private NetPeer server;
    protected const int port = 14242;

    protected long PlayerID
    {
        get { return server.UniqueIdentifier; }
    }

    public ServerBase(bool isClient, string gameName)
    {
        this.gameName = gameName;
        ServerBase.isClient = isClient;

        NetPeerConfiguration config = new NetPeerConfiguration("7DFPSFTW");
        config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
        config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
        
        if (isClient)
            server = new NetClient(config);
        else
        {
            config.Port = port;
            server = new NetServer(config);
        }
        
        server.Start();

        DebugConsole.Log("Net: " + (isClient ? "Client" : "Server") + " started");
    }

    public List<GameServer> DiscoverClients()
    {
        if (!isClient)
            DebugConsole.LogError("Net: DiscoverClients called from server");

        server.DiscoverLocalPeers(port);
        
        return discoveredClients;
    }

    public void Join(GameServer host)
    {
        if (!isClient)
            DebugConsole.LogError("Net: Join called from server");

        DebugConsole.Log("Net.Client: Connect to " + host.Endpoint.ToString());
        discoveredClients.Clear();

        server.Connect(host.Endpoint);
    }
    
    public void Close()
    {
        if (server != null)
        {
            DebugConsole.Log("Net: Closing");
            OnDisconnected(PlayerID);

            if (server != null)
            {
                server.Shutdown("Bye");
                server = null;
            }
        }
    }
    
    public virtual void Poll()
    {
        NetIncomingMessage msg;
        while ((msg = server.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.DiscoveryRequest:
                    {
                        NetOutgoingMessage response = server.CreateMessage();
                        response.Write(gameName);
                        server.SendDiscoveryResponse(response, msg.SenderEndPoint);
                    }
                    break;

                case NetIncomingMessageType.DiscoveryResponse:
                    {
                        bool foundEndpoint = false;
                        foreach (GameServer s in discoveredClients)
                            if (s.Endpoint.ToString() == msg.SenderEndPoint.ToString())
                            {
                                foundEndpoint = true;
                                break;
                            }

                        if (foundEndpoint)
                            break;
                        
                        discoveredClients.Add(new GameServer()
                        {
                            Endpoint = msg.SenderEndPoint,
                            GameName = msg.ReadString()
                        });
                    }
                    break;

                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                    DebugConsole.Log("Net: Status " + status + ": " + msg.SenderConnection.RemoteUniqueIdentifier);

                    switch (status)
                    {
                        case NetConnectionStatus.Connected:
                            OnConnected(msg.SenderConnection.RemoteUniqueIdentifier);
                            break;

                        case NetConnectionStatus.Disconnected:
                            OnDisconnected(msg.SenderConnection.RemoteUniqueIdentifier);
                            break;
                    }
                    break;

                case NetIncomingMessageType.Data:
                    MessageTypes type = (MessageTypes)msg.ReadByte();
                    HandleMessage(type, msg);
                    break;
                    
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.ErrorMessage:
                    DebugConsole.Log("Net: " + msg.ReadString());
                    break;
                    
                default:
                    DebugConsole.LogError("Net unhandled type: " + msg.MessageType);
                    break;
            }

            if (server != null)
                server.Recycle(msg);
        }
    }

    protected NetOutgoingMessage CreateMessage(MessageTypes message)
    {
        NetOutgoingMessage msg = server.CreateMessage();
        msg.Write((byte)message);
        return msg;
    }

    protected void SendMessage(NetOutgoingMessage msg, NetDeliveryMethod method)
    {
        if (server.Connections.Count > 0)
            server.SendMessage(msg, server.Connections, method, 0);
    }

    protected abstract void HandleMessage(MessageTypes type, NetIncomingMessage msg);

    protected virtual void OnConnected(long playerID) {}
    protected virtual void OnDisconnected(long playerID) {}
}
