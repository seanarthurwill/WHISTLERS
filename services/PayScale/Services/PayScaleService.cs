using Microsoft.EntityFrameworkCore;
using PayScaleService.Data;
using PayScaleService.Models;

namespace PayScaleService.Services
{
    public interface IPayScaleRuleService
    {
        Task<IEnumerable<PayScaleRule>> GetAllPayScaleRulesAsync();
        Task<PayScaleRule?> GetPayScaleRuleByIdAsync(int id);
        Task<IEnumerable<PayScaleRule>> GetPayScaleRulesBySportAsync(int sportId);
        Task<IEnumerable<PayScaleRule>> GetPayScaleRulesByLeagueAsync(int leagueId);
        Task<PayScaleRule?> GetPayScaleRuleAsync(int sportId, int? leagueId, int? ageLevelId, int? positionId);
        Task<PayScaleRule> CreatePayScaleRuleAsync(PayScaleRule payScaleRule);
        Task<PayScaleRule?> UpdatePayScaleRuleAsync(int id, PayScaleRule payScaleRule);
        Task<bool> DeletePayScaleRuleAsync(int id);
    }



    // ===== IMPLEMENTATIONS =====

    public class PayScaleRuleService : IPayScaleRuleService
    {
        private readonly ApplicationDbContext _context;

        public PayScaleRuleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PayScaleRule>> GetAllPayScaleRulesAsync()
        {
            return await _context.PayScaleRules
                .Where(p => p.IsActive)
                .OrderBy(p => p.SportId)
                .ThenBy(p => p.PositionId)
                .ToListAsync();
        }

        public async Task<PayScaleRule?> GetPayScaleRuleByIdAsync(int id)
        {
            return await _context.PayScaleRules.FindAsync(id);
        }

        public async Task<IEnumerable<PayScaleRule>> GetPayScaleRulesBySportAsync(int sportId)
        {
            return await _context.PayScaleRules
                .Where(p => p.SportId == sportId && p.IsActive)
                .OrderBy(p => p.PositionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PayScaleRule>> GetPayScaleRulesByLeagueAsync(int leagueId)
        {
            return await _context.PayScaleRules
                .Where(p => p.LeagueId == leagueId && p.IsActive)
                .OrderBy(p => p.PositionId)
                .ToListAsync();
        }

        public async Task<PayScaleRule?> GetPayScaleRuleAsync(int sportId, int? leagueId, int? ageLevelId, int? positionId)
        {
            return await _context.PayScaleRules
                .FirstOrDefaultAsync(p => 
                    p.SportId == sportId 
                    && p.LeagueId == leagueId 
                    && p.AgeLevelId == ageLevelId 
                    && p.PositionId == positionId
                    && p.IsActive);
        }

        public async Task<PayScaleRule> CreatePayScaleRuleAsync(PayScaleRule payScaleRule)
        {
            payScaleRule.CreatedAt = DateTime.UtcNow;
            _context.PayScaleRules.Add(payScaleRule);
            await _context.SaveChangesAsync();
            return payScaleRule;
        }

        public async Task<PayScaleRule?> UpdatePayScaleRuleAsync(int id, PayScaleRule payScaleRule)
        {
            var existing = await _context.PayScaleRules.FindAsync(id);
            if (existing == null) return null;

            existing.SportId = payScaleRule.SportId;
            existing.LeagueId = payScaleRule.LeagueId;
            existing.AgeLevelId = payScaleRule.AgeLevelId;
            existing.PositionId = payScaleRule.PositionId;
            existing.BasePayAmount = payScaleRule.BasePayAmount;
            existing.PayMultiplier = payScaleRule.PayMultiplier;
            existing.PayPerKm = payScaleRule.PayPerKm;
            existing.IsActive = payScaleRule.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePayScaleRuleAsync(int id)
        {
            var payScaleRule = await _context.PayScaleRules.FindAsync(id);
            if (payScaleRule == null) return false;

            _context.PayScaleRules.Remove(payScaleRule);
            await _context.SaveChangesAsync();
            return true;
        }
    }


}
