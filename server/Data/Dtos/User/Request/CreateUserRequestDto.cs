using System.ComponentModel.DataAnnotations;

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
        [MinLength(3)]
        public string Username { get; init; }
        
        [Required]
        [Range(1_000, 1_000_000)]
        public int InitialBalance { get; init; }
        
        [Required]
        public string ProfilePictureSeed { get; init; }
    }
}