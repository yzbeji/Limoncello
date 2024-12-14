using System.ComponentModel.DataAnnotations;

namespace Limoncello.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string? OrganizerId { get; set; }
        [Required(ErrorMessage = "Please name it")]
        public string? Name { get; set; }
        public byte[]? ProjectPicture { get; set; }
        public virtual ICollection<UserProject>? UserProjects { get; set; }
        public virtual ICollection<TaskColumn>? TaskColumns { get; set; }
    }
}