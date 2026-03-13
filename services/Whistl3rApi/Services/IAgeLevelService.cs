using Whistl3rApi.Models;

namespace Whistl3rApi.Services
{
    public interface IAgeLevelService
    {
        Task<IEnumerable<AgeLevel>> GetAllAgeLevelsAsync();
        Task<AgeLevel?> GetAgeLevelByIdAsync(int id);
        Task<IEnumerable<AgeLevel>> GetAgeLevelsBySportAsync(int sportId);
    }
}
