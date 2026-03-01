using System.ComponentModel.DataAnnotations.Schema;

namespace Whistl3rApi.Models
{
    [Table("pay_scale_rules")]
    public class PayScaleRule
    {
        [Column("pay_scale_rule_id")]
        public int PayScaleRuleId { get; set; }
        
        [Column("sport_id")]
        public int? SportId { get; set; }
        
        [Column("age_level_id")]
        public int? AgeLevelId { get; set; }
        
        [Column("position_id")]
        public int? PositionId { get; set; }
        
        [Column("league_id")]
        public int? LeagueId { get; set; }
        
        [Column("base_pay_amount")]
        public decimal BasePayAmount { get; set; }
        
        [Column("pay_multiplier")]
        public decimal PayMultiplier { get; set; } = 1.00m;
        
        [Column("pay_per_km")]
        public decimal PayPerKm { get; set; } = 0.00m;
        
        [Column("priority")]
        public int Priority { get; set; } = 100;
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
