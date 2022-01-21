using System;
using System.Resources;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OneOf;
using OneOf.Types;
using Serilog;
using SolidTradeServer.Data.Models.Errors;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Services
{
    public class CloudinaryService
    {
        private readonly ILogger _logger = Log.ForContext<CloudinaryService>();

        private readonly Cloudinary _cloudinary;
        private readonly string _environment;
        private readonly string _cloudName;
        
        public CloudinaryService(IConfiguration configuration)
        {
            _environment = configuration["Environment"];

            _cloudName = configuration["CloudinaryCredentials:CloudName"];
            var apiKey = configuration["CloudinaryCredentials:ApiKey"];
            var apiSecret = configuration["CloudinaryCredentials:ApiSecret"];
            
            Account account = new Account(_cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public Task<OneOf<UploadResult, UnexpectedError>> UploadProfilePicture(string url, string uid)
            => UploadImage(new FileDescription(url), 75, uid, $"Projects/SolidTrade-{_environment}/");

        public Task<OneOf<UploadResult, UnexpectedError>> UploadProfilePicture(IFormFile file, string uid)
            => UploadImage(new FileDescription(file.Name, file.OpenReadStream()),
                GetAdjustedQualityCompressionRatio(file.Length), uid, $"Projects/SolidTrade-{_environment}/");

        public Task<OneOf<Success, UnexpectedError>> DeleteProfilePicture(string url) 
            => DeleteImage(url);

        private async Task<OneOf<Success, UnexpectedError>> DeleteImage(string url)
        {
            var httpOrHttps = url.StartsWith("https") ? "https" : "http";

            try
            {
                string baseUrl = $"{httpOrHttps}://res.cloudinary.com/{_cloudName}/image/upload/";
                var s1 = url[baseUrl.Length..];

                var slashIndex = s1.IndexOf('/');
            
                var publicId = s1.Substring(slashIndex + 1, s1.LastIndexOf('.') - slashIndex - 1);

                var deletionParams = new DeletionParams(publicId);
                await _cloudinary.DestroyAsync(deletionParams);
                
                _logger.Information("Deletion image with url {@ImageUrl} and public id {@PublicId} was successful", url, publicId);
                return new Success();
            }
            catch (Exception e)
            {
                var error = new UnexpectedError
                {
                    Title = "Deletion failed",
                    Message = "The image deletion failed.",
                    AdditionalData = new {url},
                    Exception = e,
                };
                
                _logger.Error(LogMessageTemplate, error);
                return error;
            }
        }
        
        private async Task<OneOf<UploadResult, UnexpectedError>> UploadImage(FileDescription description, int quality, string uid, string folder)
        {
            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = description,
                    Folder = folder,
                    Transformation = new Transformation().AspectRatio(1).Crop("crop").Quality(quality),
                    FilenameOverride = uid,
                    UseFilename = true,
                };

                _logger.Information("Trying to upload image by uid {@Uid} with quality of {@Quality}", uid, quality);
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                _logger.Information("Image upload by uid {@Uid} with quality of {@Quality} was successful", uid, quality);
                return uploadResult;
            }
            catch (Exception e)
            {
                var error = new UnexpectedError
                {
                    Title = "Upload failed",
                    Message = "The image upload failed.",
                    AdditionalData = new { FileDescription = description, folder, uid},
                    Exception = e,
                };
                
                _logger.Error(LogMessageTemplate, error);
                return error;
            }
        }

        private static int GetAdjustedQualityCompressionRatio(long fileSize)
        {
            const int threshold = 1000000; // 1MB
            var ratio = (double) threshold / fileSize;

            return ratio >= 1 ? 100 : (int)(ratio * 100);
        }
    }
}