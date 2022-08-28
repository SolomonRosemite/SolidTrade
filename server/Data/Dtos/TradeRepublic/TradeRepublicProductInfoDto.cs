using System;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.TradeRepublic
{
    public class TradeRepublicProductInfoDto
    {
        public bool? Active { get; init; }
        public string[] ExchangeIds { get; init; }
        public string Isin { get; init; }
        public DerivativeInfo DerivativeInfo { get; init; }
        public StockSplitInfo[] Splits { get; init; }
    }

    public class DerivativeInfo
    {
        public ProductCategory ProductCategoryName { get; init; }
        public DerivativeInfoProperties Properties { get; init; }
    }

    public class StockSplitInfo
    {
        public long Date { get; init; }
        public int Initial { get; init; }
        public int Final { get; init; }
    }

    public class DerivativeInfoProperties
    {
        public DateTime? Expiry { get; init; }
        public string Currency { get; init; }
    }
}