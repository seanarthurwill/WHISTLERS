using Microsoft.EntityFrameworkCore;
using OrganizationsService.Models;

namespace OrganizationsService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<OfficialOrganization> OfficialOrganizations { get; set; } = null!;
        public DbSet<OrganizationRole> OrganizationRoles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Organization configuration
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.OrganizationId);
                entity.ToTable("organizations");
                
                entity.Property(e => e.OrganizationName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OrganizationAbbr).HasMaxLength(50);
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

            // OfficialOrganization configuration
            modelBuilder.Entity<OfficialOrganization>(entity =>
            {
                entity.HasKey(e => e.OfficialOrganizationId);
                entity.ToTable("official_organizations");
                
                entity.Property(e => e.OfficialId).IsRequired();
                entity.Property(e => e.OrganizationId).IsRequired();
                entity.Property(e => e.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // Configure unique constraint
                entity.HasIndex(e => new { e.OfficialId, e.OrganizationId })
                    .IsUnique()
                    .HasDatabaseName("uq_official_organization");
                
                // Configure indexes
                entity.HasIndex(e => e.OfficialId)
                    .HasDatabaseName("idx_official_organizations_official_id");
                    
                entity.HasIndex(e => e.OrganizationId)
                    .HasDatabaseName("idx_official_organizations_organization_id");
                
                // Configure relationship to Organization
                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .HasConstraintName("fk_official_organizations_organization")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrganizationRole configuration
            modelBuilder.Entity<OrganizationRole>(entity =>
            {
                entity.HasKey(e => e.OrganizationRoleId);
                entity.ToTable("organization_roles");
                
                entity.Property(e => e.OrganizationId).IsRequired();
                entity.Property(e => e.RoleId).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // Configure unique constraint
                entity.HasIndex(e => new { e.OrganizationId, e.RoleId })
                    .IsUnique()
                    .HasDatabaseName("uq_organization_role");
                
                // Configure indexes
                entity.HasIndex(e => e.OrganizationId)
                    .HasDatabaseName("idx_organization_roles_organization_id");
                    
                entity.HasIndex(e => e.RoleId)
                    .HasDatabaseName("idx_organization_roles_role_id");
                
                // Configure relationship to Organization
                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .HasConstraintName("fk_organization_roles_organization")
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
