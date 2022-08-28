using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneOf;
using OneOf.Types;
using SolidTradeServer.Data.Dtos.Shared.Common;
using SolidTradeServer.Data.Dtos.TradeRepublic;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Services.TradeRepublic
{
    public class TradeRepublicApiService : TradeRepublicApi
    {
        public TradeRepublicApiService(IConfiguration configuration, OngoingProductsService ongoingProductsService) : base(configuration, ongoingProductsService)
        {
        }

        private async Task<OneOf<Success, ErrorResponse>> IsStockMarketOpen(string isinWithExchangeExtension)
        {
            string content = "{\"type\":\"aggregateHistoryLight\",\"range\":\"1d\",\"id\":\""+ isinWithExchangeExtension + "\"}";
            var oneOfResult = await AddRequest<TradeRepublicIsStockMarketOpenResponseDto>(content, CancellationToken.None);
            
            var result = oneOfResult.Match<OneOf<bool, UnexpectedError>>(dto =>
            {
                if (!dto.ExpectedClosingTime.HasValue)
                    return true;

                return DateTimeOffset.FromUnixTimeMilliseconds(dto.ExpectedClosingTime.Value) > DateTimeOffset.Now;
            }, error => error);

            if (result.TryPickT1(out var unexpectedError, out var isStockMarketOpen))
                return new ErrorResponse(unexpectedError, HttpStatusCode.BadRequest);

            if (!isStockMarketOpen)
            {
                return new ErrorResponse(new StockMarketClosed
                {
                    Title = "Stock market closed",
                    Message = "Tried to trade while stock market was closed.",
                    UserFriendlyMessage = "The stock market is unfortunately already closed.",
                }, HttpStatusCode.BadRequest);
            }

            return new Success();
        }

        public async Task<OneOf<T, ErrorResponse>> MakeTrRequest<T>(string requestString)
        {
            var cts = new CancellationTokenSource();
            T trResponse;
            
            try
            {
                cts.CancelAfter(1000 * 10);
                var oneOfResult =
                    await AddRequest<T>(requestString, cts.Token);

                if (oneOfResult.TryPickT1(out var error, out trResponse))
                    return new ErrorResponse(error, HttpStatusCode.InternalServerError);
            }
            catch (OperationCanceledException e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Task timeout",
                    Message = "Fetching product using trade republic api took too long.",
                    AdditionalData = new { RequestString = requestString },
                    Exception = e,
                }, HttpStatusCode.InternalServerError);
            }
            finally { cts.Dispose(); }

            return trResponse;
        }

        public async Task<OneOf<TradeRepublicProductInfoDto, ErrorResponse>> ValidateRequest(string isinWithoutExchangeExtension)
        {
            if ((await IsStockMarketOpen(isinWithoutExchangeExtension)).TryPickT1(out var errorResponse, out _))
                return errorResponse;

            if ((await MakeTrRequest<TradeRepublicProductInfoDto>(GetTradeRepublicProductInfoRequestString(ToIsinWithoutExchangeExtension(isinWithoutExchangeExtension))))
                .TryPickT1(out var errorResponse2, out var productInfo))
                return errorResponse2;
            
            if (!productInfo.Active!.Value)
            {
                const string message = "Product can not be bought or sold. This might happen if the product is expired or is knocked out.";
                return new ErrorResponse(new TradeFailed
                {
                    Title = "Product can not be traded",
                    Message = message,
                    UserFriendlyMessage = message,
                    AdditionalData = new {Isin = isinWithoutExchangeExtension}
                }, HttpStatusCode.BadRequest);
            }

            return productInfo;
        }
     }
}