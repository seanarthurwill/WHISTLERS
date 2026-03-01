using Microsoft.AspNetCore.Mvc;
using Whistl3rApi.Models;
using Whistl3rApi.Services;

namespace Whistl3rApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerformanceReviewsController : ControllerBase
    {
        private readonly IPerformanceReviewService _reviewService;

        public PerformanceReviewsController(IPerformanceReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PerformanceReview>>> GetAll()
        {
            var reviews = await _reviewService.GetAllPerformanceReviewsAsync();
            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PerformanceReview>> GetById(int id)
        {
            var review = await _reviewService.GetPerformanceReviewByIdAsync(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        [HttpGet("game-assignment/{gameAssignmentId}")]
        public async Task<ActionResult<IEnumerable<PerformanceReview>>> GetForGameAssignment(int gameAssignmentId)
        {
            var reviews = await _reviewService.GetReviewsForGameAssignmentAsync(gameAssignmentId);
            return Ok(reviews);
        }

        [HttpGet("reviewer/{reviewerId}")]
        public async Task<ActionResult<IEnumerable<PerformanceReview>>> GetByReviewer(int reviewerId)
        {
            var reviews = await _reviewService.GetReviewsByReviewerAsync(reviewerId);
            return Ok(reviews);
        }

        [HttpGet("game-assignment/{gameAssignmentId}/public")]
        public async Task<ActionResult<IEnumerable<PerformanceReview>>> GetPublicForGameAssignment(int gameAssignmentId)
        {
            var reviews = await _reviewService.GetPublicReviewsForGameAssignmentAsync(gameAssignmentId);
            return Ok(reviews);
        }

        [HttpGet("game-assignment/{gameAssignmentId}/average-rating")]
        public async Task<ActionResult<double>> GetAverageRating(int gameAssignmentId)
        {
            var average = await _reviewService.GetAverageOverallRatingForGameAssignmentAsync(gameAssignmentId);
            return Ok(average);
        }

        [HttpPost]
        public async Task<ActionResult<PerformanceReview>> Create([FromBody] PerformanceReview review)
        {
            var created = await _reviewService.CreatePerformanceReviewAsync(review);
            return CreatedAtAction(nameof(GetById), new { id = created.PerformanceReviewId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PerformanceReview>> Update(int id, [FromBody] PerformanceReview review)
        {
            var updated = await _reviewService.UpdatePerformanceReviewAsync(id, review);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _reviewService.DeletePerformanceReviewAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
