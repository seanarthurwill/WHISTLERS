using Microsoft.EntityFrameworkCore;
using Whistl3rApi.Data;
using Whistl3rApi.Models;

namespace Whistl3rApi.Services
{
    public interface IPerformanceReviewService
    {
        Task<IEnumerable<PerformanceReview>> GetAllPerformanceReviewsAsync();
        Task<PerformanceReview?> GetPerformanceReviewByIdAsync(int id);
        Task<IEnumerable<PerformanceReview>> GetReviewsForGameAssignmentAsync(int gameAssignmentId);
        Task<IEnumerable<PerformanceReview>> GetReviewsByReviewerAsync(int reviewerId);
        Task<IEnumerable<PerformanceReview>> GetPublicReviewsForGameAssignmentAsync(int gameAssignmentId);
        Task<PerformanceReview> CreatePerformanceReviewAsync(PerformanceReview review);
        Task<PerformanceReview?> UpdatePerformanceReviewAsync(int id, PerformanceReview review);
        Task<bool> DeletePerformanceReviewAsync(int id);
        Task<double> GetAverageOverallRatingForGameAssignmentAsync(int gameAssignmentId);
    }

    // ===== IMPLEMENTATIONS =====

    public class PerformanceReviewService : IPerformanceReviewService
    {
        private readonly ApplicationDbContext _context;

        public PerformanceReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PerformanceReview>> GetAllPerformanceReviewsAsync()
        {
            return await _context.PerformanceReviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<PerformanceReview?> GetPerformanceReviewByIdAsync(int id)
        {
            return await _context.PerformanceReviews
                .FirstOrDefaultAsync(r => r.PerformanceReviewId == id);
        }

        public async Task<IEnumerable<PerformanceReview>> GetReviewsForGameAssignmentAsync(int gameAssignmentId)
        {
            return await _context.PerformanceReviews
                .Where(r => r.GameAssignmentId == gameAssignmentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceReview>> GetReviewsByReviewerAsync(int reviewerId)
        {
            return await _context.PerformanceReviews
                .Where(r => r.ReviewerId == reviewerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceReview>> GetPublicReviewsForGameAssignmentAsync(int gameAssignmentId)
        {
            return await _context.PerformanceReviews
                .Where(r => r.GameAssignmentId == gameAssignmentId && r.IsPublic)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<PerformanceReview> CreatePerformanceReviewAsync(PerformanceReview review)
        {
            review.CreatedAt = DateTime.UtcNow;
            _context.PerformanceReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<PerformanceReview?> UpdatePerformanceReviewAsync(int id, PerformanceReview review)
        {
            var existing = await _context.PerformanceReviews.FindAsync(id);
            if (existing == null) return null;

            existing.GameAssignmentId = review.GameAssignmentId;
            existing.ReviewerId = review.ReviewerId;
            existing.KnowledgeOfRules = review.KnowledgeOfRules;
            existing.Positioning = review.Positioning;
            existing.Communication = review.Communication;
            existing.GameManagement = review.GameManagement;
            existing.Professionalism = review.Professionalism;
            existing.OverallRating = review.OverallRating;
            existing.Strengths = review.Strengths;
            existing.AreasForImprovement = review.AreasForImprovement;
            existing.AdditionalComments = review.AdditionalComments;
            existing.IsPublic = review.IsPublic;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePerformanceReviewAsync(int id)
        {
            var review = await _context.PerformanceReviews.FindAsync(id);
            if (review == null) return false;

            _context.PerformanceReviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<double> GetAverageOverallRatingForGameAssignmentAsync(int gameAssignmentId)
        {
            var reviews = await _context.PerformanceReviews
                .Where(r => r.GameAssignmentId == gameAssignmentId && r.OverallRating.HasValue)
                .ToListAsync();

            if (!reviews.Any()) return 0;
            return reviews.Average(r => r.OverallRating!.Value);
        }
    }
}
