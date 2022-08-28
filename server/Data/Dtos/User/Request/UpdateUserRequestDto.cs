using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SolidTradeServer.Data.Models.Annotations;
using SolidTradeServer.Data.Models.Annotations.Group;
using static SolidTradeServer.Common.Constants;

namespace SolidTradeServer.Data.Dtos.User.Request
{
    public class UpdateUserDto
    {
        [EmailAddress]
        public string Email { get; init; }
        
        [MinLength(3)]
        public string DisplayName { get; init; }
        
        [UsernameValidator]
        public string Username { get; init; }
        
        public bool? PublicPortfolio { get; init; }
        
        public string Bio { get; init; }
        
        public string ProfilePictureSeed { get; init; }
        
        [MaxFileSize(MaxUploadFileSize)]
        public IFormFile ProfilePictureFile { get; init; }
    }
}