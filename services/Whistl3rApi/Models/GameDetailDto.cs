using System.ComponentModel.DataAnnotations.Schema;

namespace Whistl3rApi.Models
{
    public class GameDetailDto
    {
        public int GameId { get; set; }
        public string? SportName { get; set; }
        public string? LeagueName { get; set; }
        public string? LevelName { get; set; }
        public DateTime GameDate { get; set; }
        public TimeSpan GameTime { get; set; }
        public string GameTimeFormatted => DateTime.Today.Add(GameTime).ToString("hh:mm tt");
        public string? VenueName { get; set; }
        public string HomeTeam { get; set; } = null!;
        public string AwayTeam { get; set; } = null!;
        public int GameStatusId { get; set; }
        public string? GameStatusName { get; set; }
        
        [NotMapped]
        public GameStatusDto? GameStatus { get; set; }
        
        [NotMapped]
        public List<OpenPositionDto> OpenPositions { get; set; } = new List<OpenPositionDto>();
    }

    public class GameStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class OpenPositionDto
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = null!;
        public bool IsRequired { get; set; }
    }
}
