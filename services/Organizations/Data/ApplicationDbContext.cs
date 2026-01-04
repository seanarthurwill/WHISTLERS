using Microsoft.EntityFrameworkCore;
using OrganizationsService.Models;

namespace OrganizationsService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Organization> Organizations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Organization configuration
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.OrganizationId);
                entity.ToTable("organizations");
                
                entity.Property(e => e.OrganizationName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OrganizationType).HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(500);
                entity.Property(e => e.ContactEmail).HasMaxLength(255);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
                entity.Property(e => e.AddressLine1).HasMaxLength(200);
                entity.Property(e => e.AddressLine2).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.StateProvince).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                
                entity.HasIndex(e => e.TenantId);
            });
        }
    }
}
