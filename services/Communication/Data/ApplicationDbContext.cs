using Microsoft.EntityFrameworkCore;
using CommunicationService.Models;

namespace CommunicationService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Message> Messages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Message configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageId);
                entity.ToTable("messages");
                
                entity.HasIndex(e => e.SenderUserId);
                entity.HasIndex(e => e.RecipientUserId);
                entity.HasIndex(e => e.RecipientGroupId);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.IsRead);
            });
        }
    }
}
