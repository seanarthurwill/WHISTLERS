using Microsoft.AspNetCore.Mvc;
using GamesService.Models;
using GamesService.Services;

namespace GamesService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetAll()
        {
            var games = await _gameService.GetAllGamesAsync();
            return Ok(games);
        }

        [HttpGet("details-report")]
        public async Task<ActionResult<IEnumerable<GameDetailsReport>>> GetDetailsReport()
        {
            var report = await _gameService.GetGameDetailsReportAsync();
            return Ok(report);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetById(int id)
        {
            var game = await _gameService.GetGameByIdAsync(id);
            if (game == null) return NotFound();
            return Ok(game);
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<Game>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var games = await _gameService.GetGamesByDateRangeAsync(startDate, endDate);
            return Ok(games);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Game>>> GetByStatus(string status)
        {
            var games = await _gameService.GetGamesByStatusAsync(status);
            return Ok(games);
        }

        [HttpPost]
        public async Task<ActionResult<Game>> Create([FromBody] Game game)
        {
            var created = await _gameService.CreateGameAsync(game);
            return CreatedAtAction(nameof(GetById), new { id = created.GameId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Game>> Update(int id, [FromBody] Game game)
        {
            var updated = await _gameService.UpdateGameAsync(id, game);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _gameService.DeleteGameAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/game-assignments")]
    public class GameAssignmentsController : ControllerBase
    {
        private readonly IGameAssignmentService _assignmentService;

        public GameAssignmentsController(IGameAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpGet("game/{gameId}")]
        public async Task<ActionResult<IEnumerable<GameAssignment>>> GetByGame(int gameId)
        {
            var assignments = await _assignmentService.GetGameAssignmentsAsync(gameId);
            return Ok(assignments);
        }

        [HttpGet("official/{officialId}")]
        public async Task<ActionResult<IEnumerable<GameAssignment>>> GetByOfficial(int officialId)
        {
            var assignments = await _assignmentService.GetOfficialAssignmentsAsync(officialId);
            return Ok(assignments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GameAssignment>> GetById(int id)
        {
            var assignment = await _assignmentService.GetGameAssignmentByIdAsync(id);
            if (assignment == null) return NotFound();
            return Ok(assignment);
        }

        [HttpGet("official/{officialId}/status/{status}")]
        public async Task<ActionResult<IEnumerable<GameAssignment>>> GetByOfficialAndStatus(int officialId, string status)
        {
            var assignments = await _assignmentService.GetAssignmentsByStatusAsync(officialId, status);
            return Ok(assignments);
        }

        [HttpPost]
        public async Task<ActionResult<GameAssignment>> Create([FromBody] GameAssignment assignment)
        {
            var created = await _assignmentService.CreateGameAssignmentAsync(assignment);
            return CreatedAtAction(nameof(GetById), new { id = created.GameAssignmentId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GameAssignment>> Update(int id, [FromBody] GameAssignment assignment)
        {
            var updated = await _assignmentService.UpdateGameAssignmentAsync(id, assignment);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var updated = await _assignmentService.UpdateAssignmentStatusAsync(id, status);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _assignmentService.DeleteGameAssignmentAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/sports")]
    public class SportsController : ControllerBase
    {
        private readonly ISportService _sportService;

        public SportsController(ISportService sportService)
        {
            _sportService = sportService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sport>>> GetAll()
        {
            var sports = await _sportService.GetAllSportsAsync();
            return Ok(sports);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Sport>> GetById(int id)
        {
            var sport = await _sportService.GetSportByIdAsync(id);
            if (sport == null) return NotFound();
            return Ok(sport);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<Sport>> GetByName(string name)
        {
            var sport = await _sportService.GetSportByNameAsync(name);
            if (sport == null) return NotFound();
            return Ok(sport);
        }

        [HttpPost]
        public async Task<ActionResult<Sport>> Create([FromBody] Sport sport)
        {
            var created = await _sportService.CreateSportAsync(sport);
            return CreatedAtAction(nameof(GetById), new { id = created.SportId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Sport>> Update(int id, [FromBody] Sport sport)
        {
            var updated = await _sportService.UpdateSportAsync(id, sport);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _sportService.DeleteSportAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/leagues")]
    public class LeaguesController : ControllerBase
    {
        private readonly ILeagueService _leagueService;

        public LeaguesController(ILeagueService leagueService)
        {
            _leagueService = leagueService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<League>>> GetAll()
        {
            var leagues = await _leagueService.GetAllLeaguesAsync();
            return Ok(leagues);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<League>> GetById(int id)
        {
            var league = await _leagueService.GetLeagueByIdAsync(id);
            if (league == null) return NotFound();
            return Ok(league);
        }

        [HttpGet("sport/{sportId}")]
        public async Task<ActionResult<IEnumerable<League>>> GetBySport(int sportId)
        {
            var leagues = await _leagueService.GetLeaguesBySportAsync(sportId);
            return Ok(leagues);
        }

        [HttpPost]
        public async Task<ActionResult<League>> Create([FromBody] League league)
        {
            var created = await _leagueService.CreateLeagueAsync(league);
            return CreatedAtAction(nameof(GetById), new { id = created.LeagueId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<League>> Update(int id, [FromBody] League league)
        {
            var updated = await _leagueService.UpdateLeagueAsync(id, league);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _leagueService.DeleteLeagueAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
