using System.ComponentModel.DataAnnotations;

namespace Limoncello.Models
{
    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int TaskColumnId { get; set; }
        public virtual TaskColumn? TaskColumn { get; set; }
    }
}
