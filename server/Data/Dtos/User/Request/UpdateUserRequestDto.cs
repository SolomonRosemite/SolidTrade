using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SolidTradeServer.Data.Models.Annotations;

namespace SolidTradeServer.Data.Dtos.User.Request
{
    public class UpdateUserDto
    {
        [EmailAddress]
        public string Email { get; init; }
        
        public string DisplayName { get; init; }
        
        public string Username { get; init; }
        
        public string Bio { get; init; }
        
        public string ProfilePictureSeed { get; init; }
        
        // Size limit 10mb
        [MaxFileSize(10000000)]
        public IFormFile ProfilePictureFile { get; init; }
    }
}