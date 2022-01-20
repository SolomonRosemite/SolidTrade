namespace SolidTradeServer.Common
{
    public static class Constants
    {
        public static string LogMessageTemplate => "{@LogParameters}"; 
        public static string UidHeader => "_Uid";

        public static string GetTradeRepublicProductInfoRequestString(string isin)
            => "{\"type\":\"instrument\",\"id\":\"" + isin + "\"}";

        public static string GetTradeRepublicProductPriceRequestString(string isin)
            => "{\"type\":\"ticker\",\"id\":\"" + isin + "\"}";
    }
}