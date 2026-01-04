using Microsoft.AspNetCore.Mvc;
using AssignorsService.Models;
using AssignorsService.Services;

namespace AssignorsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignorsController : ControllerBase
    {
        private readonly IAssignorService _assignorService;

        public AssignorsController(IAssignorService assignorService)
        {
            _assignorService = assignorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignor>>> GetAll()
        {
            var assignors = await _assignorService.GetAllAssignorsAsync();
            return Ok(assignors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Assignor>> GetById(int id)
        {
            var assignor = await _assignorService.GetAssignorByIdAsync(id);
            if (assignor == null) return NotFound();
            return Ok(assignor);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Assignor>> GetByUserId(int userId)
        {
            var assignor = await _assignorService.GetAssignorByUserIdAsync(userId);
            if (assignor == null) return NotFound();
            return Ok(assignor);
        }

        [HttpPost]
        public async Task<ActionResult<Assignor>> Create([FromBody] Assignor assignor)
        {
            var created = await _assignorService.CreateAssignorAsync(assignor);
            return CreatedAtAction(nameof(GetById), new { id = created.AssignorId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Assignor>> Update(int id, [FromBody] Assignor assignor)
        {
            var updated = await _assignorService.UpdateAssignorAsync(id, assignor);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _assignorService.DeleteAssignorAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }


}
