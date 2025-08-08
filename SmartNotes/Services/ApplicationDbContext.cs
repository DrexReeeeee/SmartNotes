using SmartNotes.Models;
using Microsoft.EntityFrameworkCore;
namespace SmartNotes.Services
{
    public class ApplicationDbContext : DbContext { 
    
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }

        public DbSet<Users> Users { get; set; }
        public DbSet<UserNotes> UserNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserNotes>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    

}
