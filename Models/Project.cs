using System.ComponentModel.DataAnnotations;

namespace Limoncello.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public byte[]? ProjectPicture { get; set; }
        public virtual ICollection<UserProject>? UserProjects { get; set; }
    }
}