using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using Game.Events;

public class TextMessageHandler : NetworkBehaviour
{
    protected virtual byte MessageType()
    {
        return 0;
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.CustomMessagingManager.OnUnnamedMessage += ReceiveMessage;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.CustomMessagingManager.OnUnnamedMessage -= ReceiveMessage;
    }

    protected virtual void OnReceivedUnnamedMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string id);
        reader.ReadValueSafe(out string message);

        if (IsServer)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                SendUnnamedMessage(id, message);
                MessageEvents.OnMessageReceived.Invoke(id, message);
            }
        }
        else
        {
            MessageEvents.OnMessageReceived.Invoke(id, message);
        }
    }

    /// <summary>
    /// When recieving a unnamed message, make sure that the message is directed to us
    /// </summary>
    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        var messageType = (byte)0;
        reader.ReadValueSafe(out messageType);
        if (messageType == MessageType())
        {
            OnReceivedUnnamedMessage(clientId, reader);
        }
    }

    /// <summary>
    /// Send the id and message to the server, so it can send to all
    /// </summary>
    /// <param name="id">users id</param>
    /// <param name="message">message to send</param>
    public virtual void SendUnnamedMessage(string id, string message)
    {
        var writer = new FastBufferWriter(1100, Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;
        using (writer)
        {
            // Write our message type
            writer.WriteValueSafe(MessageType());

            // Write our string message
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(message);

            if (IsServer)
            {
                customMessagingManager.SendUnnamedMessageToAll(writer);
            }
            else
            {
                customMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer);
            }
        }
    }
}