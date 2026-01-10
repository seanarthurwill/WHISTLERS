using Microsoft.EntityFrameworkCore;
using GamesService.Data;
using GamesService.Models;

namespace GamesService.Services
{
    public interface IGameService
    {
        Task<IEnumerable<Game>> GetAllGamesAsync();
        Task<Game?> GetGameByIdAsync(int id);
        Task<IEnumerable<Game>> GetGamesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Game>> GetGamesByStatusAsync(string status);
        Task<IEnumerable<GameDetailsReport>> GetGameDetailsReportAsync();
        Task<Game> CreateGameAsync(Game game);
        Task<Game?> UpdateGameAsync(int id, Game game);
        Task<bool> DeleteGameAsync(int id);
        Task<bool> GameExistsAsync(int id);
    }

    public interface IGameAssignmentService
    {
        Task<IEnumerable<GameAssignment>> GetGameAssignmentsAsync(int gameId);
        Task<IEnumerable<GameAssignment>> GetOfficialAssignmentsAsync(int officialId);
        Task<GameAssignment?> GetGameAssignmentByIdAsync(int id);
        Task<IEnumerable<GameAssignment>> GetAssignmentsByStatusAsync(int officialId, string status);
        Task<GameAssignment> CreateGameAssignmentAsync(GameAssignment assignment);
        Task<GameAssignment?> UpdateGameAssignmentAsync(int id, GameAssignment assignment);
        Task<bool> DeleteGameAssignmentAsync(int id);
        Task<bool> UpdateAssignmentStatusAsync(int id, string status);
    }

    public interface ISportService
    {
        Task<IEnumerable<Sport>> GetAllSportsAsync();
        Task<Sport?> GetSportByIdAsync(int id);
        Task<Sport?> GetSportByNameAsync(string sportName);
        Task<Sport> CreateSportAsync(Sport sport);
        Task<Sport?> UpdateSportAsync(int id, Sport sport);
        Task<bool> DeleteSportAsync(int id);
    }

    public interface ILeagueService
    {
        Task<IEnumerable<League>> GetAllLeaguesAsync();
        Task<League?> GetLeagueByIdAsync(int id);
        Task<IEnumerable<League>> GetLeaguesBySportAsync(int sportId);
        Task<League> CreateLeagueAsync(League league);
        Task<League?> UpdateLeagueAsync(int id, League league);
        Task<bool> DeleteLeagueAsync(int id);
    }

    // ===== IMPLEMENTATIONS =====

    public class GameService : IGameService
    {
        private readonly ApplicationDbContext _context;

        public GameService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Game>> GetAllGamesAsync()
        {
            return await _context.Games
                .Include(g => g.Assignments)
                .OrderByDescending(g => g.GameDate)
                .ToListAsync();
        }

        public async Task<Game?> GetGameByIdAsync(int id)
        {
            return await _context.Games
                .Include(g => g.Assignments)
                .FirstOrDefaultAsync(g => g.GameId == id);
        }

        public async Task<IEnumerable<Game>> GetGamesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Games
                .Where(g => g.GameDate >= startDate && g.GameDate <= endDate)
                .OrderBy(g => g.GameDate)
                .ThenBy(g => g.GameTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetGamesByStatusAsync(string status)
        {
            // Note: This method now expects a game status ID as string, or needs refactoring to accept int
            if (int.TryParse(status, out int statusId))
            {
                return await _context.Games
                    .Where(g => g.GameStatusId == statusId)
                    .OrderBy(g => g.GameDate)
                    .ToListAsync();
            }
            
            return new List<Game>();
        }

        public async Task<Game> CreateGameAsync(Game game)
        {
            game.CreatedAt = DateTime.UtcNow;
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<Game?> UpdateGameAsync(int id, Game game)
        {
            var existing = await _context.Games.FindAsync(id);
            if (existing == null) return null;

            existing.GameDate = game.GameDate;
            existing.GameTime = game.GameTime;
            existing.HomeTeam = game.HomeTeam;
            existing.AwayTeam = game.AwayTeam;
            existing.VenueId = game.VenueId;
            existing.LeagueId = game.LeagueId;
            existing.AgeLevelId = game.AgeLevelId;
            existing.OrganizationId = game.OrganizationId;
            existing.GameStatusId = game.GameStatusId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null) return false;

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GameExistsAsync(int id)
        {
            return await _context.Games.AnyAsync(g => g.GameId == id);
        }

        public async Task<IEnumerable<GameDetailsReport>> GetGameDetailsReportAsync()
        {
            var sql = @"
                SELECT 
                    g.game_id AS GameId,
                    s.sport_name AS SportName,
                    l.league_name AS LeagueName,
                    al.age_level_name AS AgeLevelName,
                    g.game_date AS GameDate,
                    g.game_time AS GameTime,
                    v.venue_name AS VenueName,
                    g.home_team AS HomeClub,
                    g.away_team AS AwayClub,
                    gs.game_status_name AS GameStatusName,
                    p.position_name AS PositionName,
                    alp.is_required AS PositionRequired,
                    alp.min_required AS MinRequired,
                    alp.max_allowed AS MaxAllowed,
                    CASE 
                        WHEN ga.assignment_status IS NOT NULL THEN ga.assignment_status
                        WHEN alp.is_required = true THEN 'Open'
                        ELSE 'Not Required'
                    END AS PositionStatus,
                    CONCAT(u.first_name, ' ', u.last_name) AS AssignedOfficial,
                    STRING_AGG(gn.note_text, '; ' ORDER BY gn.created_at) AS GameNotes
                FROM games g
                LEFT JOIN sports s ON g.organization_id = (SELECT organization_id FROM organizations WHERE organization_id = g.organization_id LIMIT 1)
                    AND s.sport_id = (SELECT sport_id FROM leagues WHERE league_id = g.league_id)
                LEFT JOIN leagues l ON g.league_id = l.league_id
                LEFT JOIN age_levels al ON g.age_level_id = al.age_level_id
                LEFT JOIN venues v ON g.venue_id = v.venue_id
                LEFT JOIN game_status gs ON g.game_status_id = gs.game_status_id
                LEFT JOIN age_level_positions alp ON al.age_level_id = alp.age_level_id AND alp.is_active = true
                LEFT JOIN positions p ON alp.position_id = p.position_id
                LEFT JOIN game_assignments ga ON g.game_id = ga.game_id AND ga.position_id = p.position_id
                LEFT JOIN officials o ON ga.official_id = o.official_id
                LEFT JOIN users u ON o.user_id = u.user_id
                LEFT JOIN game_notes gn ON g.game_id = gn.game_id
                GROUP BY 
                    g.game_id,
                    s.sport_name,
                    l.league_name,
                    al.age_level_name,
                    g.game_date,
                    g.game_time,
                    v.venue_name,
                    g.home_team,
                    g.away_team,
                    gs.game_status_name,
                    p.position_name,
                    alp.is_required,
                    alp.min_required,
                    alp.max_allowed,
                    ga.assignment_status,
                    u.first_name,
                    u.last_name
                ORDER BY g.game_date, g.game_time, g.game_id";

            var result = await _context.Database
                .SqlQueryRaw<GameDetailsReport>(sql)
                .ToListAsync();

            return result;
        }
    }

    public class GameAssignmentService : IGameAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public GameAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GameAssignment>> GetGameAssignmentsAsync(int gameId)
        {
            return await _context.GameAssignments
                .Include(ga => ga.Game)
                .Where(ga => ga.GameId == gameId)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameAssignment>> GetOfficialAssignmentsAsync(int officialId)
        {
            return await _context.GameAssignments
                .Include(ga => ga.Game)
                .Where(ga => ga.OfficialId == officialId)
                .OrderByDescending(ga => ga.Game.GameDate)
                .ToListAsync();
        }

        public async Task<GameAssignment?> GetGameAssignmentByIdAsync(int id)
        {
            return await _context.GameAssignments
                .Include(ga => ga.Game)
                .FirstOrDefaultAsync(ga => ga.GameAssignmentId == id);
        }

        public async Task<IEnumerable<GameAssignment>> GetAssignmentsByStatusAsync(int officialId, string status)
        {
            return await _context.GameAssignments
                .Include(ga => ga.Game)
                .Where(ga => ga.OfficialId == officialId && ga.AssignmentStatus == status)
                .OrderBy(ga => ga.Game.GameDate)
                .ToListAsync();
        }

        public async Task<GameAssignment> CreateGameAssignmentAsync(GameAssignment assignment)
        {
            assignment.AssignedAt = DateTime.UtcNow;
            _context.GameAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<GameAssignment?> UpdateGameAssignmentAsync(int id, GameAssignment assignment)
        {
            var existing = await _context.GameAssignments.FindAsync(id);
            if (existing == null) return null;

            existing.GameId = assignment.GameId;
            existing.OfficialId = assignment.OfficialId;
            existing.PositionId = assignment.PositionId;
            existing.AssignmentStatus = assignment.AssignmentStatus;
            existing.BasePayAmount = assignment.BasePayAmount;
            existing.FinalPayAmount = assignment.FinalPayAmount;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteGameAssignmentAsync(int id)
        {
            var assignment = await _context.GameAssignments.FindAsync(id);
            if (assignment == null) return false;

            _context.GameAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAssignmentStatusAsync(int id, string status)
        {
            var assignment = await _context.GameAssignments.FindAsync(id);
            if (assignment == null) return false;

            assignment.AssignmentStatus = status;
            assignment.AcceptedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class SportService : ISportService
    {
        private readonly ApplicationDbContext _context;

        public SportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sport>> GetAllSportsAsync()
        {
            return await _context.Sports
                .Where(s => s.IsActive)
                .OrderBy(s => s.SportName)
                .ToListAsync();
        }

        public async Task<Sport?> GetSportByIdAsync(int id)
        {
            return await _context.Sports.FindAsync(id);
        }

        public async Task<Sport?> GetSportByNameAsync(string sportName)
        {
            return await _context.Sports
                .FirstOrDefaultAsync(s => s.SportName == sportName);
        }

        public async Task<Sport> CreateSportAsync(Sport sport)
        {
            _context.Sports.Add(sport);
            await _context.SaveChangesAsync();
            return sport;
        }

        public async Task<Sport?> UpdateSportAsync(int id, Sport sport)
        {
            var existing = await _context.Sports.FindAsync(id);
            if (existing == null) return null;

            existing.SportName = sport.SportName;
            existing.IsActive = sport.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteSportAsync(int id)
        {
            var sport = await _context.Sports.FindAsync(id);
            if (sport == null) return false;

            _context.Sports.Remove(sport);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class LeagueService : ILeagueService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeagueService> _logger;

        public LeagueService(ApplicationDbContext context, ILogger<LeagueService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<League>> GetAllLeaguesAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch leagues from database");
                _logger.LogInformation($"Connection String: {_context.Database.GetConnectionString()}");
                
                var leagues = await _context.Leagues
                    .OrderBy(l => l.LeagueName)
                    .ToListAsync();
                    
                _logger.LogInformation($"Successfully fetched {leagues.Count()} leagues");
                return leagues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leagues");
                throw;
            }
        }

        public async Task<League?> GetLeagueByIdAsync(int id)
        {
            return await _context.Leagues.FindAsync(id);
        }

        public async Task<IEnumerable<League>> GetLeaguesBySportAsync(int sportId)
        {
            return await _context.Leagues
                .Where(l => l.SportId == sportId && l.IsActive)
                .OrderBy(l => l.LeagueName)
                .ToListAsync();
        }

        public async Task<League> CreateLeagueAsync(League league)
        {
            _context.Leagues.Add(league);
            await _context.SaveChangesAsync();
            return league;
        }

        public async Task<League?> UpdateLeagueAsync(int id, League league)
        {
            var existing = await _context.Leagues.FindAsync(id);
            if (existing == null) return null;

            existing.LeagueName = league.LeagueName;
            existing.SportId = league.SportId;
            existing.IsActive = league.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteLeagueAsync(int id)
        {
            var league = await _context.Leagues.FindAsync(id);
            if (league == null) return false;

            _context.Leagues.Remove(league);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
