using Microsoft.AspNetCore.Mvc;
using Whistl3rApi.Models;
using Whistl3rApi.Services;

namespace Whistl3rApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgeLevelsController : ControllerBase
    {
        private readonly IAgeLevelService _ageLevelService;

        public AgeLevelsController(IAgeLevelService ageLevelService)
        {
            _ageLevelService = ageLevelService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AgeLevel>>> GetAll()
        {
            var ageLevels = await _ageLevelService.GetAllAgeLevelsAsync();
            return Ok(ageLevels);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AgeLevel>> GetById(int id)
        {
            var ageLevel = await _ageLevelService.GetAgeLevelByIdAsync(id);
            if (ageLevel == null) return NotFound();
            return Ok(ageLevel);
        }

        [HttpGet("sport/{sportId}")]
        public async Task<ActionResult<IEnumerable<AgeLevel>>> GetBySport(int sportId)
        {
            var ageLevels = await _ageLevelService.GetAgeLevelsBySportAsync(sportId);
            return Ok(ageLevels);
        }
    }
}
