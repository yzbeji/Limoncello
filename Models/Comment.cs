using System.ComponentModel.DataAnnotations;

namespace Limoncello.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }
        [Required(ErrorMessage = "The comment needs content")]
        public string? Content { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public int ProjectTaskId { get; set; }
        public virtual ProjectTask? ProjectTask { get; set; }
    }
}
