using Newtonsoft.Json;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.Messaging
{
    public class ResponseDto
    {
        public ResponseDto(int id, MessageType messageType, object data, bool successful)
        {
            Id = id;
            MessageType = messageType;
            Data = data;
            Successful = successful;
        }

        public ResponseDto(MessageDto message, object data, bool successful)
        {
            Id = message.Id;
            MessageType = (MessageType) (int)message.MessageType!;
            Data = data;
            Successful = successful;
        }

        public int Id { get; }
        public object Data { get; }
        public MessageType MessageType { get; }
        public bool Successful { get; }
        
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}