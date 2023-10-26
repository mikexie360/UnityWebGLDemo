using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using Game.Events;

public class AudioHandler : NetworkBehaviour
{
    protected virtual byte MessageType()
    {
        return 1;
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
        reader.ReadValueSafe(out int samples);
        reader.ReadValueSafe(out int channels);
        reader.ReadValueSafe(out int frequency);
        reader.ReadValueSafe(out float[] data);
        //reader.ReadValueSafe(out string message);
        if (IsServer)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                //Debug.Log($"Server received unnamed message of type ({MessageType()}) from client " +
                //    $"({clientId}) that contained the string: \"{id}\"");
                SendUnnamedMessage(samples, channels, frequency, data);
                AudioEvents.OnAudioReceived.Invoke(samples, channels, frequency, data);
            }
            // As an example, we can also broadcast the client message to everyone
            //SendUnnamedMessage($"Newly connected client sent this greeting: \"{stringMessage}\"");
        }
        else
        {
            //AddMessage(id, message);
            AudioEvents.OnAudioReceived.Invoke(samples, channels, frequency, data);
            //Debug.Log(id);
        }
    }

    /// <summary>
    /// When recieving a unnamed message, make sure that the message is directed to us
    /// </summary>
    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        var messageType = (byte)1;
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
    /// Send the id and message to the server, so it can send to all
    /// </summary>
    /// <param name="dataToSend"></param>
    public virtual void SendUnnamedMessage(int samples, int channels, int frequency, float[] data)
    {
        var writer = new FastBufferWriter((data.Length * 4) + 100, Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;
        // Tip: Placing the writer within a using scope assures it will
        // be disposed upon leaving the using scope
        using (writer)
        {
            // Write our message type
            writer.WriteValueSafe(MessageType());

            // Write our string message
            writer.WriteValueSafe(samples);
            writer.WriteValueSafe(channels);
            writer.WriteValueSafe(frequency);
            writer.WriteValueSafe(data);
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
