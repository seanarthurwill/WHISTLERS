using System.ComponentModel.DataAnnotations.Schema;

namespace GamesService.Models
{
    [Table("game_claims")]
    public class GameClaim
    {
        [Column("game_claim_id")]
        public int GameClaimId { get; set; }
        
        [Column("game_id")]
        public int GameId { get; set; }
        
        [Column("official_id")]
        public int OfficialId { get; set; }
        
        [Column("position_id")]
        public int PositionId { get; set; }
        
        [Column("claim_status")]
        public string ClaimStatus { get; set; } = "Pending";
        
        [Column("claimed_at")]
        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
        
        [Column("reviewed_by")]
        public int? ReviewedBy { get; set; }
        
        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }
        
        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        public Game? Game { get; set; }
    }
}
