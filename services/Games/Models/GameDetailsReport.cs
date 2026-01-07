namespace GamesService.Models
{
    public class GameDetailsReport
    {
        public int GameId { get; set; }
        public string? SportName { get; set; }
        public string? LeagueName { get; set; }
        public string? AgeLevelName { get; set; }
        public DateTime GameDate { get; set; }
        public TimeSpan GameTime { get; set; }
        public string? VenueName { get; set; }
        public string? HomeClub { get; set; }
        public string? AwayClub { get; set; }
        public string? GameStatusName { get; set; }
        public string? PositionName { get; set; }
        public bool? PositionRequired { get; set; }
        public int? MinRequired { get; set; }
        public int? MaxAllowed { get; set; }
        public string? PositionStatus { get; set; }
        public string? AssignedOfficial { get; set; }
        public string? GameNotes { get; set; }
    }
}
