using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Dtos.User.Request
{
    public class UpdateUserDto
    {
        [Required]
        public int Id { get; init; }
     
        [Required]
        public string Token { get; init; }
     
        [EmailAddress]
        public string Email { get; init; }
        
        public string DisplayName { get; init; }
        
        public string Username { get; init; }
        
        public string ProfilePictureUrl { get; init; }
    }
}