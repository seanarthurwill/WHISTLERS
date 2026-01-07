using Microsoft.EntityFrameworkCore;
using PayScaleService.Models;

namespace PayScaleService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PayScaleRule> PayScaleRules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PayScaleRule configuration
            modelBuilder.Entity<PayScaleRule>(entity =>
            {
                entity.HasKey(e => e.PayScaleRuleId);
                entity.ToTable("pay_scale_rules");
                
                entity.Property(e => e.BasePayAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PayMultiplier).HasColumnType("decimal(5,2)");
                entity.Property(e => e.PayPerKm).HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => e.SportId);
            });
        }
    }
}
