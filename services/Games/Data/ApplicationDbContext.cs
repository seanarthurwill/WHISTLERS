using Microsoft.EntityFrameworkCore;
using GamesService.Models;

namespace GamesService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<GameAssignment> GameAssignments { get; set; } = null!;
        public DbSet<GameClaim> GameClaims { get; set; } = null!;
        public DbSet<GameStatus> GameStatuses { get; set; } = null!;
        public DbSet<Sport> Sports { get; set; } = null!;
        public DbSet<League> Leagues { get; set; } = null!;
        public DbSet<AgeLevel> AgeLevels { get; set; } = null!;
        public DbSet<LeagueOrganization> LeagueOrganizations { get; set; } = null!;
        public DbSet<Venue> Venues { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                entity.HasIndex(e => e.PayScaleRuleId);
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
                entity.Property(e => e.Season).HasMaxLength(50);
                entity.HasIndex(e => e.SportId);
                entity.HasIndex(e => e.Season);
            });

            // AgeLevel configuration
            modelBuilder.Entity<AgeLevel>(entity =>
            {
                entity.HasKey(e => e.AgeLevelId);
                entity.ToTable("age_levels");
                
                entity.Property(e => e.AgeLevelName).IsRequired().HasMaxLength(50);
            });

            // GameStatus configuration
            modelBuilder.Entity<GameStatus>(entity =>
            {
                entity.HasKey(e => e.GameStatusId);
                entity.ToTable("game_status");
                
                entity.Property(e => e.GameStatusName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // LeagueOrganization configuration
            modelBuilder.Entity<LeagueOrganization>(entity =>
            {
                entity.HasKey(e => e.LeagueOrganizationId);
                entity.ToTable("league_organizations");
                
                entity.Property(e => e.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                entity.HasIndex(e => new { e.LeagueId, e.OrganizationId }).IsUnique();
                entity.HasIndex(e => e.LeagueId);
                entity.HasIndex(e => e.OrganizationId);
                
                entity.HasOne(e => e.League)
                    .WithMany()
                    .HasForeignKey(e => e.LeagueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Venue configuration
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(e => e.VenueId);
                entity.ToTable("venues");
                
                entity.Property(e => e.VenueName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AddressLine1).HasMaxLength(200);
                entity.Property(e => e.AddressLine2).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.StateProvince).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Timezone).HasMaxLength(100);
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,8)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(11,8)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => new { e.Latitude, e.Longitude });
            });
        }
    }
}
