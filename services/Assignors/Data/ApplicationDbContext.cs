using Microsoft.EntityFrameworkCore;
using AssignorsService.Models;

namespace AssignorsService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Assignor> Assignors { get; set; } = null!;
        public DbSet<AssignorOrganization> AssignorOrganizations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Assignor configuration
            modelBuilder.Entity<Assignor>(entity =>
            {
                entity.HasKey(e => e.AssignorId);
                entity.ToTable("assignors");
                
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            // AssignorOrganization configuration
            modelBuilder.Entity<AssignorOrganization>(entity =>
            {
                entity.HasKey(e => e.AssignorOrganizationId);
                entity.ToTable("assignor_organizations");
                
                entity.Property(e => e.RoleLevel).HasMaxLength(50);

                entity.HasOne(e => e.Assignor)
                    .WithMany(a => a.AssignorOrganizations)
                    .HasForeignKey(e => e.AssignorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.AssignorId, e.OrganizationId }).IsUnique();
            });
        }
    }
}
