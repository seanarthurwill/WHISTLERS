using Microsoft.EntityFrameworkCore;
using Whistl3rApi.Data;
using Whistl3rApi.Models;

namespace Whistl3rApi.Services
{
    public interface IAssignorService
    {
        Task<IEnumerable<Assignor>> GetAllAssignorsAsync();
        Task<Assignor?> GetAssignorByIdAsync(int id);
        Task<Assignor?> GetAssignorByUserIdAsync(int userId);
        Task<Assignor> CreateAssignorAsync(Assignor assignor);
        Task<Assignor?> UpdateAssignorAsync(int id, Assignor assignor);
        Task<bool> DeleteAssignorAsync(int id);
        Task<bool> AssignorExistsAsync(int id);
    }



    // ===== IMPLEMENTATIONS =====

    public class AssignorService : IAssignorService
    {
        private readonly ApplicationDbContext _context;

        public AssignorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Assignor>> GetAllAssignorsAsync()
        {
            return await _context.Assignors
                .Include(a => a.AssignorOrganizations)
                .ToListAsync();
        }

        public async Task<Assignor?> GetAssignorByIdAsync(int id)
        {
            return await _context.Assignors
                .Include(a => a.AssignorOrganizations)
                .FirstOrDefaultAsync(a => a.AssignorId == id);
        }

        public async Task<Assignor?> GetAssignorByUserIdAsync(int userId)
        {
            return await _context.Assignors
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Assignor> CreateAssignorAsync(Assignor assignor)
        {
            _context.Assignors.Add(assignor);
            await _context.SaveChangesAsync();
            return assignor;
        }

        public async Task<Assignor?> UpdateAssignorAsync(int id, Assignor assignor)
        {
            var existing = await _context.Assignors.FindAsync(id);
            if (existing == null) return null;

            existing.UserId = assignor.UserId;
            existing.IsSuperAdmin = assignor.IsSuperAdmin;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAssignorAsync(int id)
        {
            var assignor = await _context.Assignors.FindAsync(id);
            if (assignor == null) return false;

            _context.Assignors.Remove(assignor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignorExistsAsync(int id)
        {
            return await _context.Assignors.AnyAsync(a => a.AssignorId == id);
        }
    }




}
