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
        }
    }
}
