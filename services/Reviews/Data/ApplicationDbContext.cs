using Microsoft.EntityFrameworkCore;
using ReviewsService.Models;

namespace ReviewsService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PerformanceReview> PerformanceReviews { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PerformanceReview configuration
            modelBuilder.Entity<PerformanceReview>(entity =>
            {
                entity.HasKey(e => e.PerformanceReviewId);
                entity.ToTable("performance_reviews");
                
                entity.HasIndex(e => e.GameAssignmentId);
                entity.HasIndex(e => e.ReviewerId);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
