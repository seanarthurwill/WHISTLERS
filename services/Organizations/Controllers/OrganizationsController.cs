using Microsoft.AspNetCore.Mvc;
using OrganizationsService.Models;
using OrganizationsService.Services;

namespace OrganizationsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetAll()
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            return Ok(organizations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Organization>> GetById(int id)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null) return NotFound();
            return Ok(organization);
        }

        [HttpGet("state/{state}")]
        public async Task<ActionResult<IEnumerable<Organization>>> GetByState(string state)
        {
            var organizations = await _organizationService.GetOrganizationsByStateAsync(state);
            return Ok(organizations);
        }

        [HttpPost]
        public async Task<ActionResult<Organization>> Create([FromBody] Organization organization)
        {
            var created = await _organizationService.CreateOrganizationAsync(organization);
            return CreatedAtAction(nameof(GetById), new { id = created.OrganizationId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Organization>> Update(int id, [FromBody] Organization organization)
        {
            var updated = await _organizationService.UpdateOrganizationAsync(id, organization);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _organizationService.DeleteOrganizationAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
