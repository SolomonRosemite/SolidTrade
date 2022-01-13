namespace SolidTradeServer.Data.Models.Enums
{
    public enum OngoingTradeResponse
    {
        Complete,
        WaitingForFill,
        PositionsAlreadyClosed,
        Failed,
    }
}