using System.ComponentModel.DataAnnotations.Schema;

namespace Whistl3rApi.Models
{
    [Table("games")]
    public class Game
    {
        [Column("game_id")]
        public int GameId { get; set; }
        
        [Column("organization_id")]
        public int OrganizationId { get; set; }
        
        [Column("league_id")]
        public int? LeagueId { get; set; }
        
        [Column("tournament_id")]
        public int? TournamentId { get; set; }
        
        [Column("venue_id")]
        public int VenueId { get; set; }
        
        [Column("age_level_id")]
        public int AgeLevelId { get; set; }
        
        [Column("home_team")]
        public string HomeTeam { get; set; } = null!;
        
        [Column("away_team")]
        public string AwayTeam { get; set; } = null!;
        
        [Column("game_date")]
        public DateTime GameDate { get; set; }
        
        [Column("game_time")]
        public TimeSpan GameTime { get; set; }
        
        [Column("game_length_minutes")]
        public int? GameLengthMinutes { get; set; }
        
        [Column("override_game_length_minutes")]
        public int? OverrideGameLengthMinutes { get; set; }
        
        [Column("pay_scale_rule_id")]
        public int? PayScaleRuleId { get; set; }
        
        [Column("game_status_id")]
        public int GameStatusId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("created_by")]
        public int CreatedBy { get; set; }

        // Navigation properties
        public GameStatus? GameStatus { get; set; }
        public ICollection<GameAssignment> Assignments { get; set; } = new List<GameAssignment>();
    }

    [Table("game_assignments")]
    public class GameAssignment
    {
        [Column("game_assignment_id")]
        public int GameAssignmentId { get; set; }
        
        [Column("game_id")]
        public int GameId { get; set; }
        
        [Column("official_id")]
        public int OfficialId { get; set; }
        
        [Column("position_id")]
        public int PositionId { get; set; }
        
        [Column("assignment_status")]
        public string AssignmentStatus { get; set; } = "Assigned"; // Assigned, Accepted, Declined, Completed, NoShow
        
        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        [Column("assigned_by")]
        public int AssignedBy { get; set; }
        
        [Column("accepted_at")]
        public DateTime? AcceptedAt { get; set; }
        
        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }
        
        [Column("base_pay_amount")]
        public decimal? BasePayAmount { get; set; }
        
        [Column("travel_pay_amount")]
        public decimal? TravelPayAmount { get; set; }
        
        [Column("multiplier_applied")]
        public decimal? MultiplierApplied { get; set; }
        
        [Column("final_pay_amount")]
        public decimal? FinalPayAmount { get; set; }
        
        [Column("distance_km")]
        public decimal? DistanceKm { get; set; }

        // Navigation properties
        public Game Game { get; set; } = null!;
    }

    [Table("sports")]
    public class Sport
    {
        [Column("sport_id")]
        public int SportId { get; set; }
        
        [Column("sport_name")]
        public string SportName { get; set; } = null!;
        
        [Column("sport_code")]
        public string? SportCode { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }

    [Table("leagues")]
    public class League
    {
        [Column("league_id")]
        public int LeagueId { get; set; }
        
        [Column("sport_id")]
        public int SportId { get; set; }
        
        [Column("league_name")]
        public string LeagueName { get; set; } = null!;
        
        [Column("season")]
        public string? Season { get; set; }
        
        [Column("start_date")]
        public DateTime? StartDate { get; set; }
        
        [Column("end_date")]
        public DateTime? EndDate { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("age_levels")]
    public class AgeLevel
    {
        [Column("age_level_id")]
        public int AgeLevelId { get; set; }
        
        [Column("sport_id")]
        public int SportId { get; set; }
        
        [Column("age_level_name")]
        public string AgeLevelName { get; set; } = null!;
        
        [Column("min_age")]
        public int? MinAge { get; set; }
        
        [Column("max_age")]
        public int? MaxAge { get; set; }
        
        [Column("display_order")]
        public int? DisplayOrder { get; set; }
        
        [Column("is_active")]
        public bool? IsActive { get; set; }
    }

    [Table("league_organizations")]
    public class LeagueOrganization
    {
        [Column("league_organization_id")]
        public int LeagueOrganizationId { get; set; }
        
        [Column("league_id")]
        public int LeagueId { get; set; }
        
        [Column("organization_id")]
        public int OrganizationId { get; set; }
        
        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public League? League { get; set; }
    }

    [Table("venues")]
    public class Venue
    {
        [Column("venue_id")]
        public int VenueId { get; set; }
        
        [Column("organization_id")]
        public int OrganizationId { get; set; }
        
        [Column("venue_name")]
        public string VenueName { get; set; } = null!;
        
        [Column("address_line1")]
        public string? AddressLine1 { get; set; }
        
        [Column("address_line2")]
        public string? AddressLine2 { get; set; }
        
        [Column("city")]
        public string? City { get; set; }
        
        [Column("state_province")]
        public string? StateProvince { get; set; }
        
        [Column("postal_code")]
        public string? PostalCode { get; set; }
        
        [Column("country")]
        public string? Country { get; set; }
        
        [Column("latitude")]
        public decimal? Latitude { get; set; }
        
        [Column("longitude")]
        public decimal? Longitude { get; set; }
        
        [Column("timezone")]
        public string? Timezone { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
