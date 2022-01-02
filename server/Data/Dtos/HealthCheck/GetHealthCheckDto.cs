namespace SolidTradeServer.Data.Dtos.HealthCheck
{
    public class GetHealthCheckDto
    {
        public GetHealthCheckDto(object requestParams, object requestHeaders)
        {
            RequestParams = requestParams;
            RequestHeaders = requestHeaders;
        }

        public string Message => "Hey there👋";
        
        public object RequestParams { get; }
        public object RequestHeaders { get; }
    }
}