using System;
using System.Linq;
using Lidgren.Network;
using System.Collections.Generic;

/// <summary>
/// Replicates network object state and control data between the client and the server.
/// </summary>
public class NetworkObjectReplicator
{
    enum MessageType
    {
        ReplicateState,
        ReplicateControlData,
        Create
    }
    
    private NetPeer server;
    private bool isServer;
    private List<NetworkObject> networkObjects = new List<NetworkObject>();
    private List<NetworkObject> ownedObjects = new List<NetworkObject>();

    private List<Type> typeIndex = new List<Type>();

    private List<NetOutgoingMessage> pendingClientCreateRequests = new List<NetOutgoingMessage>();

    internal NetworkObjectReplicator(NetPeer server, bool isServer)
    {
        this.server = server;
        this.isServer = isServer;
    }
    
    /// <summary>
    /// Registers a NetworkObject type. This must be called in the same order on the client and the server.
    /// </summary>
    public void Register<T>() where T : NetworkObject, new()
    {
        typeIndex.Add(typeof(T));
    }

    NetOutgoingMessage CreateMessage(int objectIndex, MessageType messageType)
    {
        NetOutgoingMessage msg = server.CreateMessage();
        msg.Write((byte)objectIndex);
        msg.Write((byte)messageType);
        return msg;
    }

    void CreateObject(int objectIndex, MessageType messageType, NetIncomingMessage msg)
    {
        int objectType = (int)msg.ReadByte();
        long owningID = msg.ReadInt64();

        DebugConsole.Log("Construction: " + objectIndex + ", " + objectType + ", " + owningID);
        DebugConsole.Log(string.Join(" ", msg.Data.Select(b => b.ToString()).ToArray()));

        //The server allocates the object index only. An object index of 0 indicates that the object has not had an ID allocated.
        if (isServer && objectIndex == 255)
        {
            objectIndex = networkObjects.Count + 1;
        }

        //Ignore if this object has already been created
        if (networkObjects.Count > objectIndex && networkObjects[objectIndex] == null)
            return;

        //The object index is the first byte of the packet. This might have been changed by the object index allocation on the server above.
        //This is a pretty dirty hack.. D:
        byte[] constructionData = new byte[msg.Data.Length];
        msg.Data.CopyTo(constructionData, 0);
        constructionData[0] = (byte)objectIndex;

        DebugConsole.Log(string.Join(" ", constructionData.Select(b => b.ToString()).ToArray()));

        //Broadcast the create message to all clients if this is the server
        if (isServer && server.Connections.Count > 0)
        {
            NetOutgoingMessage outMsg = server.CreateMessage();
            outMsg.Write(constructionData);
            server.SendMessage(outMsg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
        }

        //Create the actual object
        while (networkObjects.Count <= objectIndex)
            networkObjects.Add(null);

        NetworkObject obj = (NetworkObject)typeIndex[objectType].GetConstructors()[0].Invoke(new object[] {});
        obj.IsMe = NetworkManager.MyID == owningID;
        obj.OwningID = owningID;
        obj.ObjectIndex = objectIndex;
        obj.ConstructionData = constructionData;
        obj.OnCreate(msg);

        //Assign the objects to the replicator
        DebugConsole.Log("Rep.Create: " + objectIndex + " " + typeIndex[objectType].Name + " " + owningID + " (" + obj.IsMe + ")");
        if (obj.IsMe)
            ownedObjects.Add(obj);

        networkObjects[objectIndex] = obj;
    }

    public void OnConnected(long playerID)
    {
        if (isServer)
        {
            //Broadcast not yet created objects to the new client
            NetConnection client = null;

            for (int i = 0; i<server.Connections.Count; i++)
                if (server.Connections[i].RemoteUniqueIdentifier == playerID)
                {
                    client = server.Connections[i];
                    break;
                }

            for (int i = 0; i<networkObjects.Count; i++)
            {
                if (networkObjects[i] != null)
                {
                    NetOutgoingMessage msg = server.CreateMessage();
                    msg.Write(networkObjects[i].ConstructionData);
                    server.SendMessage(msg, client, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }
    }

    public void OnDisconnected(long playerID)
    {
        //TODO: handle disconnects by destroying owned objects
    }

    /// <summary>
    /// Creates an instance of the network object on the client and the server. 
    /// Assigns the callee as the owner of the object.
    /// </summary>
    /// <typeparam name="T">The NetworkObject implementing type to instantiate.</typeparam>
    public void Create<T>(Func<NetOutgoingMessage, NetOutgoingMessage> createMessage) where T : NetworkObject, new()
    {
        int objectType = typeIndex.IndexOf(typeof(T));

        NetOutgoingMessage msg = CreateMessage(255, MessageType.Create);
        msg.Write((byte)objectType);
        msg.Write(NetworkManager.MyID);
        msg = createMessage(msg);
        
        if (isServer)
        {
            NetIncomingMessage incomingMsg = server.CreateIncomingMessage(NetIncomingMessageType.Data, msg.Data);
            HandleMessage(incomingMsg);

            server.Recycle(msg);
        }
        else
        {
            pendingClientCreateRequests.Add(msg);
        }
    }
    
    public void HandleMessage(NetIncomingMessage msg)
    {
        int objectIndex = (int)msg.ReadByte();
        MessageType messageType = (MessageType)msg.ReadByte();

        //We cannot sync this object yet because we are waiting for the create message for it.
        if (messageType != MessageType.Create && (objectIndex >= networkObjects.Count || networkObjects[objectIndex] == null))
        {
            DebugConsole.Log(messageType + " from null object " + objectIndex);
            return;
        }

        switch (messageType)
        {
            case MessageType.Create:
                CreateObject(objectIndex, messageType, msg);
                break;
            case MessageType.ReplicateState:
                networkObjects[objectIndex].DeserializeState(msg);
                break;
            case MessageType.ReplicateControlData:
                networkObjects[objectIndex].DeserializeControlData(msg);
                break;
        }
    }
    
    public void SendMessages()
    {
        if (isServer)
        {
            //Additional bits eventually needed:
            //* A flow control layer abstracting the server message calls.
            //* A prioritization layer that picks which messages to send (implemented in NetworkObject? or a priority queue?)
            
            //Handle local control data messages locally to server
            for (int i = 0; i<ownedObjects.Count; i++)
            {
                if (ownedObjects[i].ShouldSerializeControlData())
                {
                    NetOutgoingMessage msg = server.CreateMessage();
                    ownedObjects[i].SerializeControlData(msg);
                    
                    NetIncomingMessage incomingMsg = server.CreateIncomingMessage(NetIncomingMessageType.Data, msg.Data);
                    ownedObjects[i].DeserializeControlData(incomingMsg);
                    
                    server.Recycle(msg);
                }
            }
            
            //Send state data to clients
            if (server.Connections.Count > 0)
            {
                for (int i = 0; i<networkObjects.Count; i++)
                {
                    if (networkObjects[i] != null && networkObjects[i].ShouldSerializeState())
                    {
                        NetOutgoingMessage msg = CreateMessage(i, MessageType.ReplicateState);
                        
                        networkObjects[i].SerializeState(msg);
                        
                        server.SendMessage(msg, server.Connections, NetDeliveryMethod.UnreliableSequenced, 0);
                    }
                }
            }
        }
        else if (server.Connections.Count > 0)
        {
            while (pendingClientCreateRequests.Count > 0)
            {
                NetOutgoingMessage msg = pendingClientCreateRequests[pendingClientCreateRequests.Count-1];

                server.SendMessage(msg, server.Connections[0], NetDeliveryMethod.ReliableOrdered);
                pendingClientCreateRequests.RemoveAt(pendingClientCreateRequests.Count - 1);
            }

            //Send control data to server
            for (int i = 0; i<ownedObjects.Count; i++)
            {
                if (ownedObjects[i].ShouldSerializeControlData())
                {
                    NetOutgoingMessage msg = CreateMessage(ownedObjects[i].ObjectIndex, MessageType.ReplicateControlData);
                    
                    ownedObjects[i].SerializeControlData(msg);
                    
                    server.SendMessage(msg, server.Connections[0], NetDeliveryMethod.UnreliableSequenced, 0);
                }
            }
        }
    }
}
