using System.Runtime.Serialization;

namespace SolidTradeServer.Data.Models.Enums
{
    public enum ProductCategory
    {
        Warrant,
        [EnumMember(Value = "Open End Turbo")]
        OpenEndTurbo,
    }
}