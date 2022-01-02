using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Dtos.User.Request
{
    public class CreateUserRequestDto
    {
        [Required]
        public string Uid { get; init; }
        
        [Required]
        [MinLength(3)]
        public string DisplayName { get; init; }
        
        [Required]
        [EmailAddress]
        public string Email { get; init; }
        
        [Required]
        [MinLength(3)]
        public string Username { get; init; }
    }
}