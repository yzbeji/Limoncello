using System.ComponentModel.DataAnnotations;

namespace Limoncello.Models
{
    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Too many characters")]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
        public TaskStatus? Status { get; set; }
        [Required]
        public DateOnly? StartDate { get; set; }
        [DueDateAfterStartDate]
        [Required]
        public DateOnly? DueDate { get; set; }
        [Required]
        public string? Content { get; set; }
        public int? Index { get; set; }
        public int TaskColumnId { get; set; }
        public virtual TaskColumn? TaskColumn { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<UserTask>? UserTasks { get; set; }
    }
}
