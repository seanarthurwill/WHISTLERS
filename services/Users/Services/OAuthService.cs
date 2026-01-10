using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using UsersService.Models;

namespace UsersService.Services
{
    public interface IOAuthService
    {
        Task<OAuthLoginResult> LoginWithProviderAsync(string provider, ClaimsPrincipal claimsPrincipal);
        Task<User?> GetOrCreateUserFromOAuthAsync(string provider, string externalId, string email, string firstName, string lastName);
        string GetExternalLoginProvider(ClaimsPrincipal principal);
    }

    public class OAuthLoginResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
        public IEnumerable<string>? Roles { get; set; }
        public int? ExpiresIn { get; set; }
    }

    public class OAuthUser
    {
        public string Provider { get; set; } = null!;
        public string ExternalId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class OAuthService : IOAuthService
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IRoleService _roleService;
        private readonly ILogger<OAuthService> _logger;

        public OAuthService(
            IUserService userService,
            IJwtService jwtService,
            IRoleService roleService,
            ILogger<OAuthService> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _roleService = roleService;
            _logger = logger;
        }

        public async Task<OAuthLoginResult> LoginWithProviderAsync(string provider, ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                // Extract OAuth claims
                var emailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
                var nameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                var givenNameClaim = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value;
                var surnameClaim = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value;
                var subClaim = claimsPrincipal.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(emailClaim))
                {
                    return new OAuthLoginResult
                    {
                        Success = false,
                        Message = "Email claim not found in OAuth response"
                    };
                }

                var externalId = subClaim ?? emailClaim;
                var firstName = givenNameClaim ?? nameClaim?.Split(' ').FirstOrDefault() ?? "";
                var lastName = surnameClaim ?? nameClaim?.Split(' ').LastOrDefault() ?? "";

                // Get or create user
                var user = await GetOrCreateUserFromOAuthAsync(provider, externalId, emailClaim, firstName, lastName);

                if (user == null)
                {
                    return new OAuthLoginResult
                    {
                        Success = false,
                        Message = "Failed to create or retrieve user"
                    };
                }

                // Get roles from UserRoles junction table
                var userWithRoles = await _userService.GetUserByIdAsync(user.UserId);
                var roles = new List<Role>();
                if (userWithRoles?.UserRoles != null)
                {
                    foreach (var userRole in userWithRoles.UserRoles)
                    {
                        var role = await _roleService.GetRoleByIdAsync(userRole.RoleId);
                        if (role != null)
                        {
                            roles.Add(role);
                        }
                    }
                }

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(user, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user.UserId, user);

                _logger.LogInformation($"User {user.Email} logged in via {provider}");

                return new OAuthLoginResult
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = user,
                    Roles = roles.Select(r => r.RoleName),
                    ExpiresIn = 3600 // 1 hour
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"OAuth login error for provider {provider}: {ex.Message}");
                return new OAuthLoginResult
                {
                    Success = false,
                    Message = $"OAuth login failed: {ex.Message}"
                };
            }
        }

        public async Task<User?> GetOrCreateUserFromOAuthAsync(string provider, string externalId, string email, string firstName, string lastName)
        {
            try
            {
                // Check if user exists by email
                var existingUser = await _userService.GetUserByEmailAsync(email);

                if (existingUser != null)
                {
                    // Update last login and return
                    existingUser.LastLogin = DateTime.UtcNow;
                    await _userService.UpdateUserAsync(existingUser.UserId, existingUser);
                    return existingUser;
                }

                // Create new user
                var newUser = new User
                {
                    Email = email,
                    FirstName = firstName ?? "User",
                    LastName = lastName ?? email.Split('@')[0],
                    PasswordHash = $"oauth:{provider}:{externalId}", // Mark as OAuth user
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    Phone = ""
                };

                var createdUser = await _userService.CreateUserAsync(newUser);

                if (createdUser != null)
                {
                    // Assign default "Official" role to OAuth users
                    var defaultRole = new Role
                    {
                        RoleName = "Official",
                        Description = "Default OAuth user role"
                    };

                    // Get or create the role
                    var existingRole = await _userService.GetUserByIdAsync(createdUser.UserId);
                    _logger.LogInformation($"New user created via OAuth: {createdUser.Email}");
                }

                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user from OAuth: {ex.Message}");
                return null;
            }
        }

        public string GetExternalLoginProvider(ClaimsPrincipal principal)
        {
            var providerClaim = principal.FindFirst("iss")?.Value ?? "";
            
            if (providerClaim.Contains("google", StringComparison.OrdinalIgnoreCase))
                return "Google";
            if (providerClaim.Contains("microsoft", StringComparison.OrdinalIgnoreCase))
                return "Microsoft";
            if (providerClaim.Contains("github", StringComparison.OrdinalIgnoreCase))
                return "GitHub";

            return "Unknown";
        }
    }
}
