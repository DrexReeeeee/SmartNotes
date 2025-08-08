using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SmartNotes.Models
{
    public class UserNotes
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set;}

        [ForeignKey("User")]
        public int UserId { get; set; }

        public Users? User { get; set; }

    }
}
