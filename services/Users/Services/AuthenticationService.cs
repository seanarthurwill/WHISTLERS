using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UsersService.Models;

namespace UsersService.Services
{
    public interface IAuthenticationService
    {
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
        Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token);
        string GenerateRefreshToken();
        Task<bool> RevokeTokenAsync(string token);
        Task<AuthResult> RequestPasswordResetAsync(string email);
        Task<AuthResult> ResetPasswordAsync(string resetGuid, string newPassword);
    }

    public interface IJwtService
    {
        string GenerateAccessToken(User user, IEnumerable<Role> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token, out ClaimsPrincipal? principal);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
        public IEnumerable<string>? Roles { get; set; }
        public int? ExpiresIn { get; set; } // seconds
    }

    public class TokenRevocation
    {
        public int TokenRevocationId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }

    // ===== IMPLEMENTATIONS =====

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly int _expirationMinutes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured");
            _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
        }

        public string GenerateAccessToken(User user, IEnumerable<Role> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("IsActive", user.IsActive.ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _configuration["Jwt:Issuer"] ?? "Whistl3r",
                Audience = _configuration["Jwt:Audience"] ?? "Whistl3rAPI",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)),
                ValidateLifetime = false // Allow expired tokens for refresh
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public bool ValidateToken(string token, out ClaimsPrincipal? principal)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "Whistl3rAPI",
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "Whistl3r",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)),
                ValidateLifetime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                return principal != null;
            }
            catch
            {
                principal = null;
                return false;
            }
        }
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, TokenRevocation> _revokedTokens = new();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IUserService userService,
            IRoleService roleService,
            IJwtService jwtService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AuthenticationService> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _jwtService = jwtService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                // Get user by email
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Verify password (in production, use proper hashing like bcrypt)
                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User account is inactive"
                    };
                }

                // Get user role
                var roles = new List<Role>();
                if (user.UserRoles.Any())
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        var r = await _roleService.GetRoleByIdAsync(userRole.RoleId);
                        if (r != null)
                        {
                            roles.Add(r);
                        }
                    }
                }

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(user, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user.UserId, user);

                return new AuthResult
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = user,
                    Roles = roles.Select(r => r.RoleName),
                    ExpiresIn = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Authentication failed: {ex.Message}"
                };
            }
        }

        public Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Task.FromResult(new AuthResult
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    });
                }

                // In production, validate refresh token against a stored token table
                var newAccessToken = _jwtService.GenerateRefreshToken();

                return Task.FromResult(new AuthResult
                {
                    Success = true,
                    AccessToken = newAccessToken,
                    RefreshToken = _jwtService.GenerateRefreshToken(),
                    ExpiresIn = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new AuthResult
                {
                    Success = false,
                    Message = $"Token refresh failed: {ex.Message}"
                });
            }
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            if (_revokedTokens.ContainsKey(token))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(_jwtService.ValidateToken(token, out _));
        }

        public async Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token)
        {
            if (!await ValidateTokenAsync(token))
            {
                return Enumerable.Empty<Claim>();
            }

            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            return principal?.Claims ?? Enumerable.Empty<Claim>();
        }

        public string GenerateRefreshToken()
        {
            return _jwtService.GenerateRefreshToken();
        }

        public Task<bool> RevokeTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return Task.FromResult(false);

            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            if (principal?.FindFirst(System.Security.Claims.ClaimTypes.Expiration) is Claim expClaim)
            {
                if (long.TryParse(expClaim.Value, out var timestamp))
                {
                    var expiresAt = UnixTimeStampToDateTime(timestamp);
                    _revokedTokens[token] = new TokenRevocation
                    {
                        Token = token,
                        RevokedAt = DateTime.UtcNow,
                        ExpiresAt = expiresAt
                    };
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        private bool VerifyPassword(string password, string hash)
        {
            // In production, use proper password hashing like bcrypt
            // For now, simple comparison (REPLACE IN PRODUCTION)
            return password == hash;
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }

        public async Task<AuthResult> RequestPasswordResetAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Email is required"
                    };
                }

                // Get user by email
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    // Return success even if user not found (security best practice to prevent email enumeration)
                    return new AuthResult
                    {
                        Success = true,
                        Message = "If the email exists in our system, a password reset code has been sent"
                    };
                }

                // Generate reset GUID
                var resetGuid = Guid.NewGuid();
                user.ResetPasswordGuid = resetGuid;

                // Update user with reset GUID
                await _userService.UpdateUserAsync(user.UserId, user);

                // Send email via Communication service
                var communicationServiceUrl = _configuration["Services:Communication"] ?? "http://localhost:5003";
                var httpClient = _httpClientFactory.CreateClient();

                var emailRequest = new
                {
                    to = user.Email,
                    subject = "Whistl3r Password Reset Request",
                    body = $"Your password reset code is: {resetGuid}\n\nThis code can be used to reset your password. If you did not request this, please ignore this email.",
                    isHtml = false
                };

                try
                {
                    _logger.LogInformation($"Attempting to send password reset email to {user.Email} via {communicationServiceUrl}");
                    
                    var response = await httpClient.PostAsJsonAsync($"{communicationServiceUrl}/api/email/send", emailRequest);
                    
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Communication service response: Status={response.StatusCode}, Body={responseContent}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Failed to send password reset email bub. Status: {response.StatusCode}, Response: {responseContent}");
                        return new AuthResult
                        {
                            Success = false,
                            Message = "Failed to send password reset email bubba"
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error calling Communication service: {ex.Message}");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Failed to send password reset email"
                    };
                }

                return new AuthResult
                {
                    Success = true,
                    Message = "If the email exists in our system, a password reset code has been sent"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password reset request failed: {ex.Message}");
                return new AuthResult
                {
                    Success = false,
                    Message = $"Password reset request failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResult> ResetPasswordAsync(string resetGuid, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(resetGuid) || string.IsNullOrWhiteSpace(newPassword))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Reset code and new password are required"
                    };
                }

                // Find user by reset GUID
                var user = await _userService.GetUserByResetGuidAsync(resetGuid);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid or expired reset code"
                    };
                }

                // Update password (in production, hash the password properly with bcrypt)
                user.PasswordHash = newPassword; // TODO: Replace with proper password hashing
                user.ResetPasswordGuid = null; // Clear the reset GUID after use

                await _userService.UpdateUserAsync(user.UserId, user);

                return new AuthResult
                {
                    Success = true,
                    Message = "Password has been reset successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password reset failed: {ex.Message}");
                return new AuthResult
                {
                    Success = false,
                    Message = $"Password reset failed: {ex.Message}"
                };
            }
        }
    }
}
