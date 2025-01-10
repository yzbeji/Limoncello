namespace Limoncello.Models
{
    public class UserTask
    {
        public string UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public int TaskId { get; set; }
        public virtual ProjectTask? Task { get; set; }
    }
}
