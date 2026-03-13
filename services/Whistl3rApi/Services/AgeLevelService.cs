using Microsoft.EntityFrameworkCore;
using Whistl3rApi.Data;
using Whistl3rApi.Models;

namespace Whistl3rApi.Services
{
    public class AgeLevelService : IAgeLevelService
    {
        private readonly ApplicationDbContext _context;

        public AgeLevelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AgeLevel>> GetAllAgeLevelsAsync()
        {
            return await _context.AgeLevels
                .ToListAsync();
        }

        public async Task<AgeLevel?> GetAgeLevelByIdAsync(int id)
        {
            return await _context.AgeLevels
                .FirstOrDefaultAsync(a => a.AgeLevelId == id);
        }

        public async Task<IEnumerable<AgeLevel>> GetAgeLevelsBySportAsync(int sportId)
        {
            return await _context.AgeLevels
                .Where(a => a.SportId == sportId)
                .ToListAsync();
        }
    }
}
