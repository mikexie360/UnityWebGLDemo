using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using Game.Events;

public class AudioHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler("voice", OnReceiveNamedMessage);
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler("voice");
    }

    protected virtual void OnReceiveNamedMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadNetworkSerializable<AudioData>(out AudioData data);
        //reader.ReadValueSafe<AudioData>(out AudioData data);
        //reader.ReadValueSafe(out int channels);
        //reader.ReadValueSafe(out int frequency);
        //reader.ReadValueSafe(out float[] data);
        reader.ReadValueSafe(out bool first);
        Debug.Log("recieved message");
        Debug.Log(clientId);
        if (IsServer)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                Debug.Log("server got client message");
                AudioEvents.OnAudioReceived.Invoke(data.Samples, data.Channels, data.Frequency, data.Data);
            }
            if (first)
            {
                SendNamedMessage(data, false);
            }

        }
        else
        {
            //AddMessage(id, message);
            Debug.Log("client got message");
            AudioEvents.OnAudioReceived.Invoke(data.Samples, data.Channels, data.Frequency, data.Data);
            //Debug.Log(id);
        }
    }

    public virtual void SendNamedMessage(AudioData audioData, bool first = true)
    {
        var writer = new FastBufferWriter((audioData.Data.Length * 5) + 100, Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;

        using (writer)
        {
            writer.WriteValueSafe<AudioData>(audioData);
            writer.WriteValueSafe<bool>(first);
            if (IsServer)
            {
                //customMessagingManager.SendNamedMessage("voice", NetworkManager.ConnectedClientsIds[1], writer, NetworkDelivery.ReliableFragmentedSequenced);
                customMessagingManager.SendNamedMessageToAll("voice", writer, NetworkDelivery.ReliableFragmentedSequenced);
            } else
            {
                customMessagingManager.SendNamedMessage("voice", NetworkManager.ServerClientId, writer, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
    }
}
