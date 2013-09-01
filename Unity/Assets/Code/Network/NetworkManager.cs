using System;
using Lidgren.Network;
using UnityEngine;
using System.Collections.Generic;

public interface INetMessage
{
    NetDeliveryMethod DeliveryMethod { get; }

    void ToNetwork(ref NetOutgoingMessage msg);
    void FromNetwork(NetIncomingMessage msg);
}

public struct GameServer
{
    public string GameName;
    public System.Net.IPEndPoint Endpoint;
}

/// <summary>
/// Abstracts away some of the client and server rubbish.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    public static bool IsServer { get; private set; }
    public static bool IsClient { get { return !IsServer; } }

    public static long MyID { get { return server.UniqueIdentifier; } }

    public static NetworkObjectReplicator Replicator { get { return replicator; } }

    private static NetPeer server = null;
    private static NetworkManager instance = null;
    private static NetworkObjectReplicator replicator = null;
    private static Action<long> onConnected, onDisconnected;

    private static string gameName;
    private static List<GameServer> discoveredClients = new List<GameServer>();

    private const int port = 14242;

    public static void Start(bool isServer, string gameName, Action<long> onConnected, Action<long> onDisconnected)
    {
        if (instance == null)
        {
            instance = new GameObject("NetworkManager").AddComponent<NetworkManager>();
        }

        NetworkManager.gameName = gameName;
        NetworkManager.onConnected = onConnected;
        NetworkManager.onDisconnected = onDisconnected;
        IsServer = isServer;

        NetPeerConfiguration config = new NetPeerConfiguration("7DFPSFTW");
        config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
        config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
        
        if (IsClient)
            server = new NetClient(config);
        else
        {
            config.Port = port;
            server = new NetServer(config);
        }
        
        server.Start();

        replicator = new NetworkObjectReplicator(server, IsServer);

        DebugConsole.Log("Started NetworkManager");
    }

    public static List<GameServer> DiscoverGames()
    {
        if (IsServer)
            DebugConsole.LogError("Net: DiscoverGames() called from Server");

        server.DiscoverLocalPeers(port);

        return discoveredClients;
    }

    public static void Join(GameServer host)
    {
        if (IsServer)
            DebugConsole.LogError("Net: Join() called from Server");

        discoveredClients.Clear();

        server.Connect(host.Endpoint);
    }

    public static void Stop()
    {
        if (server != null)
        {
            onDisconnected(MyID);

            if (server != null)
            {
                server.Shutdown("Bye");
                server = null;
            }
        }
    }

    void Update()
    {
        if (server == null)
            return;

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
                {
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                    
                    DebugConsole.Log("Net: Status " + status + ": " + msg.SenderConnection.RemoteUniqueIdentifier);
                    
                    switch (status)
                    {
                        case NetConnectionStatus.Connected:
                            onConnected(msg.SenderConnection.RemoteUniqueIdentifier);
                            break;
                            
                        case NetConnectionStatus.Disconnected:
                            onDisconnected(msg.SenderConnection.RemoteUniqueIdentifier);
                            break;
                    }
                }
                    break;

                case NetIncomingMessageType.Data:
                    replicator.HandleMessage(msg);
                    break;

                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.ErrorMessage:
                    DebugConsole.Log("Net: " + msg.ReadString());
                    break;
                    
                default:
                    DebugConsole.LogError("Net: Unhandled type " + msg.MessageType);
                    break;
            }

            if (server != null)
                server.Recycle(msg);
        }

        replicator.SendMessages();
    }
}
