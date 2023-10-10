using Unity.Netcode;
using UnityEngine;

namespace GameFramework.Network.Chat
{
    public class MessageState : INetworkSerializable
    {

        public int Tick;
        public string Player;
        public string Chat;
        public string Guid; 

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Tick);
                reader.ReadValueSafe(out Player);
                reader.ReadValueSafe(out Chat);
                reader.ReadValueSafe(out Guid);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Tick);
                writer.WriteValueSafe(Player);
                writer.WriteValueSafe(Chat);
                writer.WriteValueSafe(Guid);
            }
        }
    }
}