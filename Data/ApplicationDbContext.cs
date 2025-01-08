using Limoncello.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Limoncello.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>  
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<TaskColumn> TaskColumns { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserProject>()
                .HasKey(up => new { up.Id, up.UserId, up.ProjectId });
            builder.Entity<UserProject>()
                .HasOne(up => up.User)
                .WithMany(up => up.UserProjects)
                .HasForeignKey(up => up.UserId);
             builder.Entity<UserProject>()
                .HasOne(up => up.Project)
                .WithMany(up => up.UserProjects)
                .HasForeignKey(up => up.ProjectId);

            builder.Entity<UserTask>()
                .HasKey(ut => new { ut.UserId, ut.TaskId });
            builder.Entity<UserTask>()
                .HasOne(ut => ut.User)
                .WithMany(ut => ut.UserTasks)
                .HasForeignKey(ut => ut.UserId);
            builder.Entity<UserTask>()
                .HasOne(ut => ut.Task)
                .WithMany(ut => ut.UserTasks)
                .HasForeignKey(ut => ut.TaskId);
        }
    }
}
