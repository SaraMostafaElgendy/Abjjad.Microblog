using Microsoft.EntityFrameworkCore;
using Abjjad.Microblog.Models;

namespace Abjjad.Microblog.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "alice", PasswordHash = "password123" },
                new User { Id = 2, Username = "bob", PasswordHash = "password123" }
            );
        }
    }
}
