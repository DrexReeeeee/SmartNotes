using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace SmartNotes.Models
{

    [Index("Email", IsUnique = true)]
    public class Users
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public Users() { }
    }
}
