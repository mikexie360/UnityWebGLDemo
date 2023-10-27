using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using Game.Events;

public class TextMessageHandler : NetworkBehaviour
{
    protected virtual byte MessageType()
    {
        // The default unnamed message type
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
                Debug.Log($"Server received unnamed message of type ({MessageType()}) from client " +
                    $"({clientId}) that contained the string: \"{id}\"");
                SendUnnamedMessage(id, message);
                MessageEvents.OnMessageReceived.Invoke(id, message);
            }
            // As an example, we can also broadcast the client message to everyone
            //SendUnnamedMessage($"Newly connected client sent this greeting: \"{stringMessage}\"");
        }
        else
        {
            //AddMessage(id, message);
            MessageEvents.OnMessageReceived.Invoke(id, message);
            Debug.Log(id);
        }
    }

    /// <summary>
    /// For this unnamed message example, we always read the message type
    /// value to determine if it should be handled by this instance in the
    ///  event it's a child of the CustomUnnamedMessageHandler class.
    /// </summary>
    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        var messageType = (byte)0;
        // Read the message type value that is written first when we send
        // this unnamed message.
        reader.ReadValueSafe(out messageType);
        // Example purposes only, you might handle this in a more optimal way
        if (messageType == MessageType())
        {
            OnReceivedUnnamedMessage(clientId, reader);
        }
    }

    /// <summary>
    /// For simplicity, the default does nothing
    /// </summary>
    /// <param name="dataToSend"></param>
    public virtual void SendUnnamedMessage(string id, string message)
    {
        var writer = new FastBufferWriter(1100, Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;
        // Tip: Placing the writer within a using scope assures it will
        // be disposed upon leaving the using scope
        using (writer)
        {
            // Write our message type
            writer.WriteValueSafe(MessageType());

            // Write our string message
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(message);
            if (IsServer)
            {
                // This is a server-only method that will broadcast the unnamed message.
                // Caution: Invoking this method on a client will throw an exception!
                customMessagingManager.SendUnnamedMessageToAll(writer);
            }
            else
            {
                // This method can be used by a client or server (client to server or server to client)
                customMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer);
            }
        }
    }
}