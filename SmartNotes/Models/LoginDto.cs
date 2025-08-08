using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SmartNotes.Models
{
    public class LoginDto
    {
    

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, PasswordPropertyText]
        public string Password { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        public LoginDto() { }

    }
}
