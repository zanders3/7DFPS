using System;
using Lidgren.Network;
using System.Collections.Generic;

/// <summary>
/// Replicates network object state and control data between the client and the server.
/// </summary>
public class NetworkObjectReplicator
{
    enum MessageType
    {
        Create,
        SetOwner,
        ReplicateState,
        ReplicateControlData
    }
    
    private NetPeer server;
    private bool isServer;
    private List<NetworkObject> networkObjects = new List<NetworkObject>();
    private List<NetworkObject> ownedObjects = new List<NetworkObject>();
    
    private List<Type> typeIndex = new List<Type>();

    void AddOwnedObject(NetworkObject obj)
    {
        if (ownedObjects.Contains(obj))
            return;

        obj.IsMe = true;
        obj.OnSetOwner();
        ownedObjects.Add(obj);
    }

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
    
    /// <summary>
    /// Creates an instance of the network object on the client and the server. 
    /// Assigns the callee as the owner of the object.
    /// </summary>
    /// <typeparam name="T">The NetworkObject implementing type to instantiate.</typeparam>
    public void Create<T>() where T : NetworkObject, new()
    {
        DebugConsole.Log("Rep.Create: " + typeof(T).Name + " " + isServer);

        int objectType = typeIndex.IndexOf(typeof(T));
        
        if (isServer)
            AddOwnedObject(GetOrCreateObject((byte)objectType, (byte)networkObjects.Count));
        else
        {
            NetOutgoingMessage msg = CreateMessage(objectType, 0, MessageType.Create);
            server.SendMessage(msg, server.Connections[0], NetDeliveryMethod.ReliableOrdered);
        }
    }
    
    NetworkObject GetOrCreateObject(int typeIndex, int objectIndex)
    {
        while (networkObjects.Count <= objectIndex)
            networkObjects.Add(null);
        
        NetworkObject obj = networkObjects[objectIndex];
        if (obj == null)
        {
            DebugConsole.Log("CreateObject: " + this.typeIndex[typeIndex].Name + " " + objectIndex);
            obj = (NetworkObject)this.typeIndex[typeIndex].GetConstructors()[0].Invoke(new object[] {});
            networkObjects[objectIndex] = obj;
            obj.TypeIndex = typeIndex;

            obj.OnCreate();
        }
        
        return obj;
    }
    
    public void HandleMessage(NetIncomingMessage msg)
    {
        int typeIndex, objectIndex;
        MessageType messageType;
        ReadMessage(msg, out typeIndex, out objectIndex, out messageType);
        
        switch (messageType)
        {
            case MessageType.Create:
                objectIndex = networkObjects.Count;
                GetOrCreateObject(typeIndex, objectIndex);
                
                if (msg.SenderConnection.RemoteUniqueIdentifier != server.UniqueIdentifier)
                {
                    NetOutgoingMessage outMsg = CreateMessage(typeIndex, objectIndex, MessageType.SetOwner);
                    server.SendMessage(outMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                }
                break;
            case MessageType.SetOwner:
                AddOwnedObject(GetOrCreateObject(typeIndex, objectIndex));
                break;
            case MessageType.ReplicateState:
                GetOrCreateObject(typeIndex, objectIndex).DeserializeState(msg);
                break;
            case MessageType.ReplicateControlData:
                GetOrCreateObject(typeIndex, objectIndex).DeserializeControlData(msg);
                break;
        }
    }
    
    NetOutgoingMessage CreateMessage(int typeIndex, int objectIndex, MessageType messageType)
    {
        NetOutgoingMessage msg = server.CreateMessage();
        msg.Write((byte)typeIndex);
        msg.Write((byte)objectIndex);
        msg.Write((byte)messageType);
        return msg;
    }
    
    void ReadMessage(NetIncomingMessage msg, out int typeIndex, out int objectIndex, out MessageType messageType)
    {
        typeIndex = (int)msg.ReadByte();
        objectIndex = (int)msg.ReadByte();
        messageType = (MessageType)msg.ReadByte();
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
                    server.Recycle(incomingMsg);
                    
                    server.Recycle(msg);
                }
            }
            
            //Send state data to clients
            if (server.Connections.Count > 0)
            {
                for (int i = 0; i<networkObjects.Count; i++)
                {
                    if (networkObjects[i].ShouldSerializeState())
                    {
                        NetOutgoingMessage msg = CreateMessage(networkObjects[i].TypeIndex, i, MessageType.ReplicateState);
                        
                        networkObjects[i].SerializeState(msg);
                        
                        server.SendMessage(msg, server.Connections, NetDeliveryMethod.UnreliableSequenced, 0);
                    }
                }
            }
        }
        else
        {
            //Send control data to server
            for (int i = 0; i<ownedObjects.Count; i++)
            {
                if (ownedObjects[i].ShouldSerializeControlData())
                {
                    NetOutgoingMessage msg = CreateMessage(networkObjects[i].TypeIndex, i, MessageType.ReplicateControlData);
                    
                    ownedObjects[i].SerializeControlData(msg);
                    
                    server.SendMessage(msg, server.Connections[0], NetDeliveryMethod.UnreliableSequenced, 0);
                }
            }
        }
    }
}
