namespace SolidTradeServer.Common
{
    public static class ErrorMessages
    {
        public const string GoodUntilErrorMessage = "Good until must be future date.";
        public const string CountOfNumberOfSharedMessage = "Number of shares must at least be 1.";
        public const string PriceThresholdMessage = "Price threshold must at least be 0.00010.";
        public const string UsernameFormatMessage = "The username format is not valid.";
    }
}