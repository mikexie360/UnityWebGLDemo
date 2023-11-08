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
        //NetworkManager.CustomMessagingManager.OnUnnamedMessage += ReceiveMessage;
        NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler("voice", OnReceiveNamedMessage);
    }

    public override void OnNetworkDespawn()
    {
        //NetworkManager.CustomMessagingManager.OnUnnamedMessage -= ReceiveMessage;
        NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler("voice");
    }

    protected virtual void OnReceivedUnnamedMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out int samples);
        reader.ReadValueSafe(out int channels);
        reader.ReadValueSafe(out int frequency);
        reader.ReadValueSafe(out float[] data);
        Debug.Log("recieved message");
        if (IsServer)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                Debug.Log("server got client message");
                SendUnnamedMessage(samples, channels, frequency, data);
                AudioEvents.OnAudioReceived.Invoke(samples, channels, frequency, data);
            }
        }
        else
        {
            //AddMessage(id, message);
            Debug.Log("client got message");
            AudioEvents.OnAudioReceived.Invoke(samples, channels, frequency, data);
            //Debug.Log(id);
        }
    }

    protected virtual void OnReceiveNamedMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out int samples);
        reader.ReadValueSafe(out int channels);
        reader.ReadValueSafe(out int frequency);
        reader.ReadValueSafe(out float[] data);
        Debug.Log("recieved message");
        if (IsServer)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                Debug.Log("server got client message");
                SendNamedMessage(new AudioData(samples, channels, frequency, data));
                AudioEvents.OnAudioReceived.Invoke(samples, channels, frequency, data);
            }
        }
        else
        {
            //AddMessage(id, message);
            Debug.Log("client got message");
            AudioEvents.OnAudioReceived.Invoke(samples, channels, frequency, data);
            //Debug.Log(id);
        }
    }

    /// <summary>
    /// When recieving a unnamed message, make sure that the message is directed to us
    /// </summary>
    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        Debug.Log("Recived message from: " + clientId);
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
        var writer = new FastBufferWriter((data.Length * 5) + 100, Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;
        // Tip: Placing the writer within a using scope assures it will
        // be disposed upon leaving the using scope
        Debug.Log("i am: " + NetworkManager.LocalClientId + " and server is: " + NetworkManager.ServerClientId);
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
                //customMessagingManager.SendUnnamedMessageToAll(writer, NetworkDelivery.ReliableFragmentedSequenced);
                try
                {
                    customMessagingManager.SendUnnamedMessage(1, writer, NetworkDelivery.ReliableFragmentedSequenced);
                } catch (System.Exception e)
                {
                    Debug.Log(e);
                }
            }
            else
            {
                // This method can be used by a client or server (client to server or server to client)
                customMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
    }

    public virtual void SendNamedMessage(AudioData audioData)
    {
        var writer = new FastBufferWriter((audioData.GetData().Length * 5) + 100, Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;

        using (writer)
        {
            writer.WriteValue<AudioData>(audioData);
            if (IsServer)
            {
                customMessagingManager.SendNamedMessageToAll("voice", writer);
            } else
            {
                customMessagingManager.SendNamedMessage("voice", NetworkManager.ServerClientId, writer);
            }
        }
    }
}
