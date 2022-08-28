using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.ProductImage.Request;
using SolidTradeServer.Data.Dtos.ProductImage.Response;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.Cache;

namespace SolidTradeServer.Services
{
    public class ProductImageService
    {
        private readonly ILogger _logger = Log.ForContext<ProductImageService>();
        private readonly CloudinaryService _cloudinaryService;
        private readonly ICacheService _cacheService;
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;

        public ProductImageService(DbSolidTrade database, ICacheService cacheService, IMapper mapper, CloudinaryService cloudinaryService)
        {
            _database = database;
            _cacheService = cacheService;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<OneOf<GetProductImageResponseDto, ErrorResponse>> GetOrCreateRedirectUrlToImage(GetProductImageRequestDto dto)
        {
            var cachedValue = _cacheService.GetCachedValue<GetProductImageResponseDto>(GetIdentifier(dto.Isin, dto.ThemeColor!.Value));

            if (!cachedValue.Expired)
                return CorrectGetProductImageResponseDto(cachedValue.Value, dto.IsWeb);
            
            var productImageRelation = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_database.ProductImageRelations, p =>
                p.Isin == dto.Isin && p.ThemeColor == dto.ThemeColor);

            if (productImageRelation is not null)
            {
                var responseDto = _mapper.Map<GetProductImageResponseDto>(productImageRelation);
                
                _cacheService.SetCachedValue(GetIdentifier(dto.Isin, dto.ThemeColor!.Value), responseDto, int.MaxValue);
                return CorrectGetProductImageResponseDto(responseDto, dto.IsWeb);
            }

            var result = await CreateProductImage(dto.Isin);

            if (result.TryPickT1(out var error, out _))
            {
                return error;
            }

            var (lightThemeResponse, darkThemeResponse) = result.AsT0;
            
            _cacheService.SetCachedValue(
                GetIdentifier(lightThemeResponse.Isin, lightThemeResponse.ThemeColor), 
                _mapper.Map<GetProductImageResponseDto>(lightThemeResponse), int.MaxValue);
            _cacheService.SetCachedValue(
                GetIdentifier(darkThemeResponse.Isin, darkThemeResponse.ThemeColor), 
                _mapper.Map<GetProductImageResponseDto>(darkThemeResponse), int.MaxValue);

            try
            {
                _database.ProductImageRelations.Add(lightThemeResponse);
                _database.ProductImageRelations.Add(darkThemeResponse);
                
                _logger.Information("Trying to save new product image relation with isin {@Isin}", dto.Isin);
                await _database.SaveChangesAsync();
                _logger.Information("Save product image relation with isin {@Isin} was successful", dto.Isin);
                
                return CorrectGetProductImageResponseDto(
                    _mapper.Map<GetProductImageResponseDto>(dto.ThemeColor == lightThemeResponse.ThemeColor
                        ? lightThemeResponse
                        : darkThemeResponse), dto.IsWeb);
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Failed to add new product image relation",
                    Message = "Failed to add new product image relation.",
                    UserFriendlyMessage = "Something went wrong. Please try again later.",
                    Exception = e,
                    AdditionalData = new { dto },
                }, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<OneOf<(ProductImageRelation, ProductImageRelation), ErrorResponse>> CreateProductImage(string isin)
        {
            var lightImage = _cloudinaryService.UploadTradeRepublicProductImage(isin, ProductImageThemeColor.Light);
            var darkImage = _cloudinaryService.UploadTradeRepublicProductImage(isin, ProductImageThemeColor.Dark);

            var results = await Task.WhenAll(lightImage, darkImage);

            if (results.Any(r => r.IsT1))
                return new ErrorResponse(results.First(r => r.IsT1).AsT1, HttpStatusCode.InternalServerError);

            var x = await darkImage;
            
            if (results.Any(r => r.AsT0.Error != null))
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Unexpected error when uploading trade republic product image",
                    Message = results.First(r => r.AsT0.Error != null).AsT0.Error.Message,
                    AdditionalData = new { isin },
                }, HttpStatusCode.InternalServerError);

            var lightProductImageUri = (await lightImage).AsT0.SecureUrl.AbsoluteUri;
            if (lightProductImageUri.EndsWith(".svg"))
            {
                lightProductImageUri = lightProductImageUri[..^3] + "png";
            }

            var darkProductImageUri = (await darkImage).AsT0.SecureUrl.AbsoluteUri;
            if (darkProductImageUri.EndsWith(".svg"))
            {
                darkProductImageUri = darkProductImageUri[..^3] + "png";
            }
            
            var lightProductImage = new ProductImageRelation
            {
               Isin = isin,
               ThemeColor = ProductImageThemeColor.Light,
               CorrespondingImageUrl = lightProductImageUri,
            };
            
            var darkProductImage = new ProductImageRelation
            {
               Isin = isin,
               ThemeColor = ProductImageThemeColor.Dark,
               CorrespondingImageUrl = darkProductImageUri,
            };
            
            return (lightProductImage, darkProductImage);
        }

        public static string GetIdentifier(string isin, ProductImageThemeColor themeColor)
            => $"{isin}-{themeColor}";

        private static GetProductImageResponseDto CorrectGetProductImageResponseDto(GetProductImageResponseDto dto,
            bool isWeb)
        {
            if (isWeb)
                return dto;
            
            dto.RedirectUrl = dto.RedirectUrl[..^3] + "svg";
            return dto;
        }
    }
}