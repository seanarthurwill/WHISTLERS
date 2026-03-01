using Microsoft.AspNetCore.Mvc;
using Whistl3rApi.Models;
using Whistl3rApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace Whistl3rApi.Controllers
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
        public async Task<ActionResult<GameDetailDto>> GetById(int id)
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
    [Route("api/claims")]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimsService _claimsService;

        public ClaimsController(IClaimsService claimsService)
        {
            _claimsService = claimsService;
        }

        /// <summary>
        /// Get all claims for a specific game
        /// </summary>
        [HttpGet("game/{gameId}")]
        public async Task<ActionResult<IEnumerable<GameClaim>>> GetClaimsByGameId(int gameId)
        {
            var claims = await _claimsService.GetClaimsByGameIdAsync(gameId);
            return Ok(claims);
        }

        /// <summary>
        /// Create a new claim for a game position
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GameClaim>> CreateClaim([FromBody] CreateClaimRequest request)
        {
            var claim = await _claimsService.CreateClaimAsync(
                request.GameId, 
                request.OfficialId, 
                request.PositionId
            );
            return CreatedAtAction(nameof(GetClaimsByGameId), new { gameId = claim.GameId }, claim);
        }

        /// <summary>
        /// Soft delete (cancel) a claim
        /// </summary>
        [HttpDelete("{claimId}")]
        public async Task<IActionResult> SoftDeleteClaim(int claimId, [FromQuery] int deletedBy)
        {
            var deleted = await _claimsService.SoftDeleteClaimAsync(claimId, deletedBy);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }

    public class CreateClaimRequest
    {
        public int GameId { get; set; }
        public int OfficialId { get; set; }
        public int PositionId { get; set; }
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

        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<League>>> GetByOrganization(int organizationId)
        {
            var leagues = await _leagueService.GetLeaguesByOrganizationAsync(organizationId);
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

    [ApiController]
    [Route("api/venues")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venue>>> GetAll()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            return Ok(venues);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Venue>> GetById(int id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null) return NotFound();
            return Ok(venue);
        }

        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<Venue>>> GetByOrganization(int organizationId)
        {
            var venues = await _venueService.GetVenuesByOrganizationAsync(organizationId);
            return Ok(venues);
        }

        [HttpPost]
        public async Task<ActionResult<Venue>> Create([FromBody] Venue venue)
        {
            var created = await _venueService.CreateVenueAsync(venue);
            return CreatedAtAction(nameof(GetById), new { id = created.VenueId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Venue>> Update(int id, [FromBody] Venue venue)
        {
            var updated = await _venueService.UpdateVenueAsync(id, venue);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _venueService.DeleteVenueAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
