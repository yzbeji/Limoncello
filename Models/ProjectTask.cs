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
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TaskStatus? Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public string? Content { get; set; }
        public int TaskColumnId { get; set; }
        public virtual TaskColumn? TaskColumn { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<UserTask>? UserTasks { get; set; }
    }
}
