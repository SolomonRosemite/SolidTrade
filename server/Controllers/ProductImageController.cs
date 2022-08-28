using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.ProductImage.Request;
using SolidTradeServer.Data.Dtos.User.Request;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Services;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/image")]
    public class ProductImageController : Controller
    {
        private readonly ILogger _logger;
        private readonly ProductImageService _productImageService;

        public ProductImageController(ProductImageService productImageService, ILogger logger)
        {
            _productImageService = productImageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetProductImageRequestDto dto)
        {
            var value = await _productImageService.GetOrCreateRedirectUrlToImage(dto);
            
            return value.Match<IActionResult>(
                response => RedirectPermanent(response.RedirectUrl),
                err =>
                {
                    var ex = err.Error.Exception;
                    err.Error.Exception = ex is not null ? new Exception("Exception is defined in the 'exceptions' field.") : null;
                    _logger.Error(ex, LogMessageTemplate, err.Error);

                    return new ObjectResult(new UnexpectedError
                    {
                        Title = err.Error.Title,
                        UserFriendlyMessage = err.Error.UserFriendlyMessage,
                        Message = err.Error.Message,
                    }) {StatusCode = (int) err.Code};
                });
        }
    }
}