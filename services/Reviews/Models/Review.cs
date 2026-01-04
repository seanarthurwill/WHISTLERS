using System.ComponentModel.DataAnnotations.Schema;

namespace ReviewsService.Models
{
    [Table("performance_reviews")]
    public class PerformanceReview
    {
        [Column("performance_review_id")]
        public int PerformanceReviewId { get; set; }
        
        [Column("game_assignment_id")]
        public int GameAssignmentId { get; set; }
        
        [Column("reviewer_id")]
        public int ReviewerId { get; set; }
        
        [Column("knowledge_of_rules")]
        public int? KnowledgeOfRules { get; set; } // 1-5 scale
        
        [Column("positioning")]
        public int? Positioning { get; set; } // 1-5 scale
        
        [Column("communication")]
        public int? Communication { get; set; } // 1-5 scale
        
        [Column("game_management")]
        public int? GameManagement { get; set; } // 1-5 scale
        
        [Column("professionalism")]
        public int? Professionalism { get; set; } // 1-5 scale
        
        [Column("overall_rating")]
        public int? OverallRating { get; set; } // 1-5 scale
        
        [Column("strengths")]
        public string? Strengths { get; set; }
        
        [Column("areas_for_improvement")]
        public string? AreasForImprovement { get; set; }
        
        [Column("additional_comments")]
        public string? AdditionalComments { get; set; }
        
        [Column("is_public")]
        public bool IsPublic { get; set; } = false;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
