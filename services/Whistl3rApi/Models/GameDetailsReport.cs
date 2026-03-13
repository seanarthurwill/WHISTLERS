namespace Whistl3rApi.Models
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
        public int GameStatusId { get; set; }
        public string? GameStatusName { get; set; }
        public string? OpenPositions { get; set; }
        public string? GameNotes { get; set; }
    }
}
