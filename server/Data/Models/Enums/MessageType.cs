namespace SolidTradeServer.Data.Models.Enums
{
    public enum MessageType
    {
        MessageTypeUnspecified = 0, 
        GetUser,
        CreateUser,
        UpdateUser,
        DeleteUser,
     
        GetProduct, // By isin
        
        GetStock,
        
        GetWarrant,
    }
}