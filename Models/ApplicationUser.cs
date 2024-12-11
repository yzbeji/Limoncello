using Microsoft.AspNetCore.Identity;

namespace Limoncello.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Organization { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public virtual ICollection<UserProject>? UserProjects { get; set; }
    }
}