using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Paytak.Models;

namespace Paytak.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ChatMessage configuration
            builder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(4000);
                entity.Property(e => e.Response).IsRequired().HasMaxLength(4000);
                entity.Property(e => e.Model).HasMaxLength(100);
                
                // Relationship with User
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
} 