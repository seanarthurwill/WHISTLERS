using Microsoft.AspNetCore.Mvc;
using Whistl3rApi.Models;
using Whistl3rApi.Services;

namespace Whistl3rApi.Controllers
{
    [ApiController]
    [Route("api/payscale-rules")]
    public class PayScaleRulesController : ControllerBase
    {
        private readonly IPayScaleRuleService _payScaleRuleService;

        public PayScaleRulesController(IPayScaleRuleService payScaleRuleService)
        {
            _payScaleRuleService = payScaleRuleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PayScaleRule>>> GetAll()
        {
            var rules = await _payScaleRuleService.GetAllPayScaleRulesAsync();
            return Ok(rules);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PayScaleRule>> GetById(int id)
        {
            var rule = await _payScaleRuleService.GetPayScaleRuleByIdAsync(id);
            if (rule == null) return NotFound();
            return Ok(rule);
        }

        [HttpGet("sport/{sportId}")]
        public async Task<ActionResult<IEnumerable<PayScaleRule>>> GetBySport(int sportId)
        {
            var rules = await _payScaleRuleService.GetPayScaleRulesBySportAsync(sportId);
            return Ok(rules);
        }

        [HttpGet("league/{leagueId}")]
        public async Task<ActionResult<IEnumerable<PayScaleRule>>> GetByLeague(int leagueId)
        {
            var rules = await _payScaleRuleService.GetPayScaleRulesByLeagueAsync(leagueId);
            return Ok(rules);
        }

        [HttpGet("calculate")]
        public async Task<ActionResult<PayScaleRule>> GetPayScaleRule([FromQuery] int sportId, [FromQuery] int? leagueId, [FromQuery] int? ageLevelId, [FromQuery] int? positionId)
        {
            var rule = await _payScaleRuleService.GetPayScaleRuleAsync(sportId, leagueId, ageLevelId, positionId);
            if (rule == null) return NotFound();
            return Ok(rule);
        }

        [HttpPost]
        public async Task<ActionResult<PayScaleRule>> Create([FromBody] PayScaleRule payScaleRule)
        {
            var created = await _payScaleRuleService.CreatePayScaleRuleAsync(payScaleRule);
            return CreatedAtAction(nameof(GetById), new { id = created.PayScaleRuleId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PayScaleRule>> Update(int id, [FromBody] PayScaleRule payScaleRule)
        {
            var updated = await _payScaleRuleService.UpdatePayScaleRuleAsync(id, payScaleRule);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _payScaleRuleService.DeletePayScaleRuleAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
