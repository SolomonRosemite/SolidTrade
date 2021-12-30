using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Dtos.User.Request
{
    public class CreateUserRequestDto
    {
        [Required]
        public string Token { get; init; }
        
        [Required]
        public string DisplayName { get; init; }
        
        [Required]
        [EmailAddress]
        public string Email { get; init; }
        
        [Required]
        public string Username { get; init; }
    }
}