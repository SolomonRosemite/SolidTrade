using System.Runtime.Serialization;

namespace SolidTradeServer.Data.Models.Enums
{
    public enum ProductCategory
    {
        Warrant,
        [EnumMember(Value = "Unlimited Turbo")]
        UnlimitedTurbo,
        [EnumMember(Value = "Best Turbo")]
        BestTurbo,
        [EnumMember(Value = "Mini Turbo")]
        MiniTurbo,
        [EnumMember(Value = "Turbo")]
        Turbo,
        [EnumMember(Value = "Open End Turbo")]
        OpenEndTurbo,
    }
}