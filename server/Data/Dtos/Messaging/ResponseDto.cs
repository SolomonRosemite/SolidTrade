using Newtonsoft.Json;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors.Base;

namespace SolidTradeServer.Data.Dtos.Messaging
{
    public class ResponseDto
    {
        private ResponseDto() { }

        public static ResponseDto Success(MessageDto message, object data)
            => Success(message.Id, message.MessageType, data);
        
        public static ResponseDto Success(int id, MessageType messageType, object data)
            => new() {Id = id, MessageType = messageType, Data = data, Successful = true};

        public static ResponseDto Failed(MessageDto message, IBaseErrorModel error)
            => Failed(message.Id, message.MessageType, error);
        public static ResponseDto Failed(int id, MessageType messageType, IBaseErrorModel error) 
            => new() {Id = id, MessageType = messageType, Error = error, Successful = false};
        
        public int Id { get; private init; }
        public object Data { get; private init; }
        public IBaseErrorModel Error { get; private init; }
        public MessageType MessageType { get; private init; }
        public bool Successful { get; private init; }
        
        public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.None);
    }
}