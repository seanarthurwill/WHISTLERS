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
            return await _context.Games
                .Where(g => g.Status == status)
                .OrderBy(g => g.GameDate)
                .ToListAsync();
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
            existing.Status = game.Status;

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

        public LeagueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<League>> GetAllLeaguesAsync()
        {
            return await _context.Leagues
                .Where(l => l.IsActive)
                .OrderBy(l => l.LeagueName)
                .ToListAsync();
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
