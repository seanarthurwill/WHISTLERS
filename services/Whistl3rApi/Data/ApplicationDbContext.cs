using Microsoft.EntityFrameworkCore;
using Whistl3rApi.Models;

namespace Whistl3rApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Users Service Entities
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Official> Officials { get; set; } = null!;
        public DbSet<Assignor> Assignors { get; set; } = null!;
        public DbSet<Coach> Coaches { get; set; } = null!;
        public DbSet<Mentor> Mentors { get; set; } = null!;
        public DbSet<Parent> Parents { get; set; } = null!;

        // Games Service Entities
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<GameAssignment> GameAssignments { get; set; } = null!;
        public DbSet<GameClaim> GameClaims { get; set; } = null!;
        public DbSet<GameStatus> GameStatuses { get; set; } = null!;
        public DbSet<Sport> Sports { get; set; } = null!;
        public DbSet<League> Leagues { get; set; } = null!;
        public DbSet<AgeLevel> AgeLevels { get; set; } = null!;
        public DbSet<LeagueOrganization> LeagueOrganizations { get; set; } = null!;
        public DbSet<Venue> Venues { get; set; } = null!;

        // Organizations Service Entities
        public DbSet<Organization> Organizations { get; set; } = null!;

        // Groups Service Entities
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<GroupMember> GroupMembers { get; set; } = null!;

        // PayScale Service Entities
        public DbSet<PayScaleRule> PayScaleRules { get; set; } = null!;

        // Reviews Service Entities
        public DbSet<PerformanceReview> PerformanceReviews { get; set; } = null!;

        // Communication Service Entities
        public DbSet<Message> Messages { get; set; } = null!;

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

            // Game configuration
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.GameId);
                entity.ToTable("games");
                entity.Property(e => e.HomeTeam).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AwayTeam).IsRequired().HasMaxLength(200);
                
                entity.HasOne(e => e.GameStatus)
                    .WithMany(gs => gs.Games)
                    .HasForeignKey(e => e.GameStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.GameDate);
                entity.HasIndex(e => e.VenueId);
                entity.HasIndex(e => e.GameStatusId);
            });

            // GameAssignment configuration
            modelBuilder.Entity<GameAssignment>(entity =>
            {
                entity.HasKey(e => e.GameAssignmentId);
                entity.ToTable("game_assignments");
                entity.Property(e => e.AssignmentStatus).HasMaxLength(20);
                entity.Property(e => e.BasePayAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TravelPayAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.MultiplierApplied).HasColumnType("decimal(5,2)");
                entity.Property(e => e.FinalPayAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DistanceKm).HasColumnType("decimal(10,2)");
                
                entity.HasOne(e => e.Game)
                    .WithMany(g => g.Assignments)
                    .HasForeignKey(e => e.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.GameId);
                entity.HasIndex(e => e.OfficialId);
                entity.HasIndex(e => e.AssignmentStatus);
            });

            // Sport configuration
            modelBuilder.Entity<Sport>(entity =>
            {
                entity.HasKey(e => e.SportId);
                entity.ToTable("sports");
                entity.Property(e => e.SportName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SportCode).HasMaxLength(20);
                entity.HasIndex(e => e.SportName).IsUnique();
            });

            // League configuration
            modelBuilder.Entity<League>(entity =>
            {
                entity.HasKey(e => e.LeagueId);
                entity.ToTable("leagues");
                entity.Property(e => e.LeagueName).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.LeagueName);
            });

            // Venue configuration
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(e => e.VenueId);
                entity.ToTable("venues");
                entity.Property(e => e.VenueName).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.VenueName);
            });

            // Organization configuration
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.OrganizationId);
                entity.Property(e => e.OrganizationName).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.OrganizationName);
            });

            // GameClaim configuration
            modelBuilder.Entity<GameClaim>(entity =>
            {
                entity.HasKey(e => e.GameClaimId);
                entity.ToTable("game_claims");
                entity.Property(e => e.ClaimStatus).HasMaxLength(20);
                entity.HasIndex(e => e.GameId);
                entity.HasIndex(e => e.OfficialId);
                entity.HasIndex(e => e.ClaimStatus);
            });
        }
    }
}
