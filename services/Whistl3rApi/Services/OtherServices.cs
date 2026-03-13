using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Whistl3rApi.Data;
using Whistl3rApi.Models;

namespace Whistl3rApi.Services
{
    public class SportService : ISportService
    {
        private readonly ApplicationDbContext _context;

        public SportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sport>> GetAllSportsAsync()
        {
            return await _context.Sports
                .Where(s => s.IsActive)
                .OrderBy(s => s.SportName)
                .ToListAsync();
        }

        public async Task<Sport?> GetSportByIdAsync(int id)
        {
            return await _context.Sports.FindAsync(id);
        }

        public async Task<Sport?> GetSportByNameAsync(string sportName)
        {
            return await _context.Sports
                .FirstOrDefaultAsync(s => s.SportName == sportName);
        }

        public async Task<Sport> CreateSportAsync(Sport sport)
        {
            _context.Sports.Add(sport);
            await _context.SaveChangesAsync();
            return sport;
        }

        public async Task<Sport?> UpdateSportAsync(int id, Sport sport)
        {
            var existing = await _context.Sports.FindAsync(id);
            if (existing == null) return null;

            existing.SportName = sport.SportName;
            existing.IsActive = sport.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteSportAsync(int id)
        {
            var sport = await _context.Sports.FindAsync(id);
            if (sport == null) return false;

            _context.Sports.Remove(sport);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class LeagueService : ILeagueService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeagueService> _logger;

        public LeagueService(ApplicationDbContext context, ILogger<LeagueService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<League>> GetAllLeaguesAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch leagues from database");
                _logger.LogInformation($"Connection String: {_context.Database.GetConnectionString()}");

                var leagues = await _context.Leagues
                    .OrderBy(l => l.LeagueName)
                    .ToListAsync();

                _logger.LogInformation($"Successfully fetched {leagues.Count()} leagues");
                return leagues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leagues");
                throw;
            }
        }

        public async Task<League?> GetLeagueByIdAsync(int id)
        {
            return await _context.Leagues.FindAsync(id);
        }

        public async Task<IEnumerable<League>> GetLeaguesBySportAsync(int sportId)
        {
            return await _context.Leagues
                .Where(l => l.SportId == sportId && l.IsActive)
                .OrderBy(l => l.LeagueName)
                .ToListAsync();
        }

        public async Task<IEnumerable<League>> GetLeaguesByOrganizationAsync(int organizationId)
        {
            return await _context.LeagueOrganizations
                .Where(lo => lo.OrganizationId == organizationId && lo.IsActive)
                .Include(lo => lo.League)
                .Where(lo => lo.League != null && lo.League.IsActive)
                .Select(lo => lo.League!)
                .OrderBy(l => l.LeagueName)
                .ToListAsync();
        }

        public async Task<League> CreateLeagueAsync(League league)
        {
            _context.Leagues.Add(league);
            await _context.SaveChangesAsync();
            return league;
        }

        public async Task<League?> UpdateLeagueAsync(int id, League league)
        {
            var existing = await _context.Leagues.FindAsync(id);
            if (existing == null) return null;

            existing.LeagueName = league.LeagueName;
            existing.SportId = league.SportId;
            existing.IsActive = league.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteLeagueAsync(int id)
        {
            var league = await _context.Leagues.FindAsync(id);
            if (league == null) return false;

            _context.Leagues.Remove(league);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class ClaimsService : IClaimsService
    {
        private readonly ApplicationDbContext _context;

        public ClaimsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GameClaim>> GetClaimsByGameIdAsync(int gameId)
        {
            return await _context.GameClaims
                .Include(gc => gc.Game)
                .Where(gc => gc.GameId == gameId)
                .OrderBy(gc => gc.ClaimedAt)
                .ToListAsync();
        }

        public async Task<GameClaim> CreateClaimAsync(int gameId, int officialId, int positionId)
        {
            // Check if a claim already exists for this game, official, and position
            var existingClaim = await _context.GameClaims
                .FirstOrDefaultAsync(c =>
                    c.GameId == gameId &&
                    c.OfficialId == officialId &&
                    c.PositionId == positionId);

            if (existingClaim != null)
            {
                // Update existing claim to Pending and refresh the claimed date
                existingClaim.ClaimStatus = "Pending";
                existingClaim.ClaimedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existingClaim;
            }

            // Create new claim if none exists
            var claim = new GameClaim
            {
                GameId = gameId,
                OfficialId = officialId,
                PositionId = positionId,
                ClaimStatus = "Pending",
                ClaimedAt = DateTime.UtcNow,
                ReviewedBy = null,
                ReviewedAt = null,
                Notes = null
            };

            _context.GameClaims.Add(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<bool> SoftDeleteClaimAsync(int claimId, int deletedBy)
        {
            var claim = await _context.GameClaims.FindAsync(claimId);
            if (claim == null) return false;

            // Soft delete by updating status to "Withdrawn"
            claim.ClaimStatus = "Withdrawn";

            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class VenueService : IVenueService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VenueService> _logger;

        public VenueService(ApplicationDbContext context, ILogger<VenueService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            return await _context.Venues
                .Where(v => v.IsActive)
                .OrderBy(v => v.VenueName)
                .ToListAsync();
        }

        public async Task<Venue?> GetVenueByIdAsync(int id)
        {
            return await _context.Venues.FindAsync(id);
        }

        public async Task<IEnumerable<Venue>> GetVenuesByOrganizationAsync(int organizationId)
        {
            _logger.LogInformation($"[GetVenuesByOrganization] Fetching venues for organization {organizationId}");

            var venues = await _context.Venues
                .Where(v => v.OrganizationId == organizationId && v.IsActive)
                .OrderBy(v => v.VenueName)
                .ToListAsync();

            _logger.LogInformation($"[GetVenuesByOrganization] Found {venues.Count()} venues for organization {organizationId}");

            return venues;
        }

        public async Task<Venue> CreateVenueAsync(Venue venue)
        {
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return venue;
        }

        public async Task<Venue?> UpdateVenueAsync(int id, Venue venue)
        {
            var existing = await _context.Venues.FindAsync(id);
            if (existing == null) return null;

            existing.OrganizationId = venue.OrganizationId;
            existing.VenueName = venue.VenueName;
            existing.AddressLine1 = venue.AddressLine1;
            existing.AddressLine2 = venue.AddressLine2;
            existing.City = venue.City;
            existing.StateProvince = venue.StateProvince;
            existing.PostalCode = venue.PostalCode;
            existing.Country = venue.Country;
            existing.Latitude = venue.Latitude;
            existing.Longitude = venue.Longitude;
            existing.Timezone = venue.Timezone;
            existing.IsActive = venue.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteVenueAsync(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return false;

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
