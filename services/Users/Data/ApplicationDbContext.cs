using Microsoft.EntityFrameworkCore;
using UsersService.Models;

namespace UsersService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Official> Officials { get; set; } = null!;
        public DbSet<Assignor> Assignors { get; set; } = null!;
        public DbSet<Coach> Coaches { get; set; } = null!;
        public DbSet<Mentor> Mentors { get; set; } = null!;
        public DbSet<Parent> Parents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.UserType).HasMaxLength(20);
                
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.UserType);
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsSystemRole).HasDefaultValue(false);
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // UserRole (junction table) configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.UserRoleId);

                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.RoleId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
