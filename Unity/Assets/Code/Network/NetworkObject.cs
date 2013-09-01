using System;
using Lidgren.Network;
using System.Collections.Generic;

/// <summary>
/// Represents an object synchronised between the client and the server.
/// All synched network state inherits from this object.
/// </summary>
public abstract class NetworkObject
{
    /// <summary>
    /// The type index determined by the order of Register() calls made to the owning NetworkObjectReplicator.
    /// </summary>
    public int TypeIndex { get; internal set; }

    /// <summary>
    /// Indicates if this machine created this object and will recieve SerializeControlData() calls.
    /// </summary>
    public bool IsMe { get; internal set; }

    public long OwningID { get; internal set; }
    public int ObjectIndex { get; internal set; }

    internal byte[] ConstructionData { get; set; }

    /// <summary>
    /// Called when the object is initially created. Called on both client and server.
    /// </summary>
    internal virtual void OnCreate(NetIncomingMessage msg)
    {
    }

    internal virtual bool ShouldSerializeState()
    {
        return false;
    }

    /// <summary>
    /// Serializes the current object state. Sent from server -> client into the DeserializeState function.
    /// Unreliably delivered.
    /// </summary>
    internal virtual void SerializeState(NetOutgoingMessage msg)
    {
    }

    /// <summary>
    /// Deserializes the current object state sent from the SerializeState function.
    /// </summary>
    internal virtual void DeserializeState(NetIncomingMessage msg)
    {
    }

    internal virtual bool ShouldSerializeControlData()
    {
        return false;
    }

    /// <summary>
    /// Serializes the client control data on the owning client. Sent from owner -> server into the DeserializeControlData function.
    /// Unreliably delivered.
    /// </summary>
    internal virtual void SerializeControlData(NetOutgoingMessage msg)
    {
    }

    /// <summary>
    /// Deserializes the control data on the server from the SerializeControlData function.
    /// </summary>
    internal virtual void DeserializeControlData(NetIncomingMessage msg)
    {
    }

    protected void SendMessageToServer(byte messageType, Func<NetOutgoingMessage, NetOutgoingMessage> msgSender)
    {
        throw new NotImplementedException();
    }

    protected void SendMessageToClient(byte messageType, Func<NetOutgoingMessage, NetOutgoingMessage> msgSender)
    {
        throw new NotImplementedException();
    }

    internal virtual void HandleMessage(byte messageType, NetIncomingMessage msg)
    {
    }
}
