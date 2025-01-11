using System.ComponentModel.DataAnnotations;

namespace Limoncello.Models
{
    public class TaskColumn
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please name it")]
        public string? Name { get; set; }
        public int? Index { get; set; }
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        public virtual ICollection<ProjectTask>? ProjectTasks { get; set; }
    }
}
