using Microsoft.EntityFrameworkCore;
using Whistl3rApi.Data;
using Whistl3rApi.Models;

namespace Whistl3rApi.Services
{
    public interface IOrganizationService
    {
        Task<IEnumerable<Organization>> GetAllOrganizationsAsync();
        Task<Organization?> GetOrganizationByIdAsync(int id);
        Task<IEnumerable<Organization>> GetOrganizationsByStateAsync(string state);
        Task<Organization> CreateOrganizationAsync(Organization organization);
        Task<Organization?> UpdateOrganizationAsync(int id, Organization organization);
        Task<bool> DeleteOrganizationAsync(int id);
        Task<bool> OrganizationExistsAsync(int id);
    }

    // ===== IMPLEMENTATIONS =====

    public class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext _context;

        public OrganizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Organization>> GetAllOrganizationsAsync()
        {
            return await _context.Organizations
                .Where(o => o.IsActive)
                .OrderBy(o => o.OrganizationName)
                .ToListAsync();
        }

        public async Task<Organization?> GetOrganizationByIdAsync(int id)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(o => o.OrganizationId == id);
        }

        public async Task<IEnumerable<Organization>> GetOrganizationsByStateAsync(string state)
        {
            return await _context.Organizations
                .Where(o => o.StateProvince == state && o.IsActive)
                .OrderBy(o => o.OrganizationName)
                .ToListAsync();
        }

        public async Task<Organization> CreateOrganizationAsync(Organization organization)
        {
            organization.CreatedAt = DateTime.UtcNow;
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization?> UpdateOrganizationAsync(int id, Organization organization)
        {
            var existing = await _context.Organizations.FindAsync(id);
            if (existing == null) return null;

            existing.OrganizationName = organization.OrganizationName;
            existing.OrganizationType = organization.OrganizationType;
            existing.Website = organization.Website;
            existing.ContactEmail = organization.ContactEmail;
            existing.ContactPhone = organization.ContactPhone;
            existing.AddressLine1 = organization.AddressLine1;
            existing.AddressLine2 = organization.AddressLine2;
            existing.City = organization.City;
            existing.StateProvince = organization.StateProvince;
            existing.PostalCode = organization.PostalCode;
            existing.Country = organization.Country;
            existing.IsActive = organization.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteOrganizationAsync(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null) return false;

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> OrganizationExistsAsync(int id)
        {
            return await _context.Organizations.AnyAsync(o => o.OrganizationId == id);
        }
    }
}
