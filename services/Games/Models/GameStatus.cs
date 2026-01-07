using System.ComponentModel.DataAnnotations.Schema;

namespace GamesService.Models
{
    [Table("game_status")]
    public class GameStatus
    {
        [Column("game_status_id")]
        public int GameStatusId { get; set; }
        
        [Column("game_status_name")]
        public string GameStatusName { get; set; } = null!;
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
