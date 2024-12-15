namespace Limoncello.Models
{
    public class UserProject 
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }
            
    }
}