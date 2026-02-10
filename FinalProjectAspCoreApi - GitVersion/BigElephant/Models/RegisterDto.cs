using System.ComponentModel.DataAnnotations;

namespace BigElephant.Models
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, MinLength(6), MaxLength(64)]
        public string Password { get; set; } = null!;
    }
}
