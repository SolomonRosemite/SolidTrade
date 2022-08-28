using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SolidTradeServer.Data.Models.Annotations;
using SolidTradeServer.Data.Models.Annotations.Group;
using static SolidTradeServer.Common.Constants;

namespace SolidTradeServer.Data.Dtos.User.Request
{
    public class CreateUserRequestDto
    {
        [Required]
        [MinLength(3)]
        public string DisplayName { get; init; }
        
        [Required]
        [EmailAddress]
        public string Email { get; init; }
        
        [Required]
        [UsernameValidator]
        public string Username { get; init; }
        
        [Required]
        [Range(1_000, 1_000_000)]
        public int InitialBalance { get; init; }
        
        [RequiredIf(nameof(ProfilePictureFile), null)]
        public string ProfilePictureSeed { get; init; }
        
        [MaxFileSize(MaxUploadFileSize)]
        [RequiredIf(nameof(ProfilePictureSeed), null)]
        public IFormFile ProfilePictureFile { get; init; }
    }
}