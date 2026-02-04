using Microsoft.EntityFrameworkCore;
using UsersService.Data;
using UsersService.Models;
using Microsoft.Data.SqlClient;


namespace UsersService.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByResetGuidAsync(string resetGuid);
        Task<User?> GetUserByOfficialIdAsync(int officialId);
        Task<User> CreateUserAsync(User user);
        Task<User?> UpdateUserAsync(int id, User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
    }

    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);
        Task<Role?> GetRoleByNameAsync(string name);
        Task<Role> CreateRoleAsync(Role role);
        Task<Role?> UpdateRoleAsync(int id, Role role);
        Task<bool> DeleteRoleAsync(int id);
        Task<List<Permission>?> GetPermissionByIdAsync(int id); 
    }

    // ===== IMPLEMENTATIONS =====

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.Email)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByResetGuidAsync(string resetGuid)
        {
            if (string.IsNullOrEmpty(resetGuid) || !Guid.TryParse(resetGuid, out var guid))
                return null;

            return await _context.Users
                .FirstOrDefaultAsync(u => u.ResetPasswordGuid == guid);
        }

        public async Task<User?> GetUserByOfficialIdAsync(int officialId)
        {
            var official = await _context.Officials
                .FirstOrDefaultAsync(o => o.OfficialId == officialId);
            
            if (official == null) return null;

            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == official.UserId);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUserAsync(int id, User user)
        {
            var existing = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
                
            if (existing == null) return null;

            existing.Email = user.Email;
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Phone = user.Phone;
            existing.TenantId = user.TenantId;
            existing.DateOfBirth = user.DateOfBirth;
            existing.UserType = user.UserType;
            existing.IsActive = user.IsActive;
            existing.ResetPasswordGuid = user.ResetPasswordGuid;
            existing.LastLogin = user.LastLogin;

            // Update user roles - remove existing and add new ones
            _context.UserRoles.RemoveRange(existing.UserRoles);
            
            foreach (var userRole in user.UserRoles)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = id,
                    RoleId = userRole.RoleId
                });
            }

            await _context.SaveChangesAsync();

            // Only process role-based table creation if user is active
            if (existing.IsActive)
            {
                // Load role names for checking
                var userWithRoles = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (userWithRoles != null)
                {
                    var roleNames = userWithRoles.UserRoles
                        .Select(ur => ur.Role.RoleName)
                        .ToList();

                    // Check and create Official record
                    if (roleNames.Contains("Official", StringComparer.OrdinalIgnoreCase))
                    {
                        var officialExists = await _context.Officials
                            .AnyAsync(o => o.UserId == id);
                        if (!officialExists)
                        {
                            _context.Officials.Add(new Official { UserId = id });
                        }
                    }

                    // Check and create Assignor record
                    if (roleNames.Contains("Assignor", StringComparer.OrdinalIgnoreCase))
                    {
                        var assignorExists = await _context.Assignors
                            .AnyAsync(a => a.UserId == id);
                        if (!assignorExists)
                        {
                            _context.Assignors.Add(new Assignor { UserId = id, IsSuperAdmin = false });
                        }
                    }

                    // Check and create Coach record
                    if (roleNames.Contains("Coach", StringComparer.OrdinalIgnoreCase))
                    {
                        var coachExists = await _context.Coaches
                            .AnyAsync(c => c.UserId == id);
                        if (!coachExists)
                        {
                            _context.Coaches.Add(new Coach { UserId = id });
                        }
                    }

                    // Check and create Mentor record
                    if (roleNames.Contains("Mentor", StringComparer.OrdinalIgnoreCase))
                    {
                        var mentorExists = await _context.Mentors
                            .AnyAsync(m => m.UserId == id);
                        if (!mentorExists)
                        {
                            _context.Mentors.Add(new Mentor { UserId = id });
                        }
                    }

                    // Check and create Parent record
                    if (roleNames.Contains("Parent", StringComparer.OrdinalIgnoreCase))
                    {
                        var parentExists = await _context.Parents
                            .AnyAsync(p => p.UserId == id);
                        if (!parentExists)
                        {
                            _context.Parents.Add(new Parent { UserId = id });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return existing;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.UserId == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }

    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .OrderBy(r => r.RoleName)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

         public async Task<List<Permission>?> GetPermissionByIdAsync(int id)
{
    var sql = @"
        select p.* 
        from role_permissions rp
        left join permissions p on rp.permission_id = p.permission_id
        where rp.role_id = {0}";

    var result = await _context.Database
        .SqlQueryRaw<Permission>(sql, id)
        .ToListAsync();

    return result;
}



        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == name);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            role.CreatedAt = DateTime.UtcNow;
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role?> UpdateRoleAsync(int id, Role role)
        {
            var existing = await _context.Roles.FindAsync(id);
            if (existing == null) return null;

            existing.RoleName = role.RoleName;
            existing.Description = role.Description;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
