// JWT token generation and handling
using System.IdentityModel.Tokens.Jwt;
// Claims-based identity for user claims in tokens
using System.Security.Claims;
// Text encoding for byte conversion
using System.Text;
using Amazon.DynamoDBv2.DataModel;

// Token validation and security keys
using Microsoft.IdentityModel.Tokens;
// User and Role models
using UsersService.Models;
using UsersService.Data;
using Microsoft.EntityFrameworkCore;

namespace UsersService.Services
{
    /// <summary>
    /// Authentication service contract for login, token management, and password reset
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>Authenticates user with email/password and returns tokens</summary>
        Task<AuthResult> LoginAsync(string email, string password);
        /// <summary>Generates new access token from valid refresh token</summary>
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        /// <summary>Validates if token is still valid and not revoked</summary>
        Task<bool> ValidateTokenAsync(string token);
        /// <summary>Extracts user claims from a token</summary>
        Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token);
        /// <summary>Generates a new refresh token string</summary>
        string GenerateRefreshToken();
        /// <summary>Revokes a token to prevent further use</summary>
        Task<bool> RevokeTokenAsync(string token);
        /// <summary>Initiates password reset by generating code and sending email</summary>
        Task<AuthResult> RequestPasswordResetAsync(string email);
        /// <summary>Resets user password using reset GUID</summary>
        Task<AuthResult> ResetPasswordAsync(string resetGuid, string newPassword);
    }

    /// <summary>
    /// JWT token service contract for token operations
    /// </summary>
    public interface IJwtService
    {
        /// <summary>Creates JWT access token with user info and roles</summary>
        string GenerateAccessToken(User user, List<Permission> permissions, int? officialId = null);
        /// <summary>Generates cryptographically secure refresh token</summary>
        string GenerateRefreshToken();
        /// <summary>Extracts claims principal from expired token (for refresh flow)</summary>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        /// <summary>Validates token and outputs claims principal if valid</summary>
        bool ValidateToken(string token, out ClaimsPrincipal? principal);
    }

    /// <summary>
    /// Authentication result containing tokens and user information
    /// </summary>
    public class AuthResult
    {
        /// <summary>Whether the authentication operation succeeded</summary>
        public bool Success { get; set; }
        /// <summary>Error or success message</summary>
        public string? Message { get; set; }
        /// <summary>JWT access token for API authorization (short-lived)</summary>
        public string? AccessToken { get; set; }
        /// <summary>Refresh token for obtaining new access tokens (long-lived)</summary>
        public string? RefreshToken { get; set; }
        /// <summary>Authenticated user object</summary>
        public User? User { get; set; }
        /// <summary>List of role names assigned to user</summary>
        public IEnumerable<string>? Roles { get; set; }
        /// <summary>Token expiration time in seconds</summary>
        public int? ExpiresIn { get; set; }
    }

    /// <summary>
    /// Represents a revoked token stored in memory
    /// </summary>
    public class TokenRevocation
    {
        /// <summary>Unique identifier for revocation record</summary>
        public int TokenRevocationId { get; set; }
        /// <summary>The revoked token string</summary>
        public string Token { get; set; } = null!;
        /// <summary>When the token was revoked</summary>
        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
        /// <summary>When the token originally expires</summary>
        public DateTime ExpiresAt { get; set; }
    }

/// <summary>
/// Represents a user session stored in DynamoDB
/// </summary>
[DynamoDBTable("UserSessions")]
public class UserSession
{
    /// <summary>
    /// Hash of the access token (partition key)
    /// </summary>
    [DynamoDBHashKey]
    public string TokenHash { get; set; } = null!;
    
    /// <summary>
    /// User ID associated with this session
    /// </summary>
    [DynamoDBProperty]
    public int UserId { get; set; }
    
    /// <summary>
    /// Refresh token for this session
    /// </summary>
    [DynamoDBProperty]
    public string RefreshToken { get; set; } = null!;
    
    /// <summary>
    /// User's email address
    /// </summary>
    [DynamoDBProperty]
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// When the session was created
    /// </summary>
    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the access token expires
    /// </summary>
    [DynamoDBProperty]
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// When the session was revoked (null if active)
    /// </summary>
    [DynamoDBProperty]
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// TTL attribute - DynamoDB auto-deletes when this Unix timestamp passes
    /// </summary>
    [DynamoDBProperty("TTL")]
    public long TimeToLive { get; set; }
}
    // ===== IMPLEMENTATIONS =====

    /// <summary>
    /// Service for JWT token generation and validation
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration; // App settings
        private readonly string _secretKey; // Secret key for signing tokens
        private readonly int _expirationMinutes; // Token lifetime in minutes

        /// <summary>
        /// Initializes JWT service with configuration
        /// </summary>
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Retrieve secret key from config (required)
            _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured");
            // Parse expiration time, default to 60 minutes
            _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
        }

        /// <summary>
        /// Generates JWT access token containing user information and roles
        /// </summary>
        public string GenerateAccessToken(User user, List<Permission> permissions, int? officialId = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey); // Convert secret to bytes

            // Build claims list with user information
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // User ID
                new Claim(ClaimTypes.Email, user.Email), // Email
                new Claim(ClaimTypes.GivenName, user.FirstName), // First name
                new Claim(ClaimTypes.Surname, user.LastName), // Last name
                new Claim("IsActive", user.IsActive.ToString()) // Account status
            };

            // Add OfficialId if provided
            if (officialId.HasValue)
            {
                claims.Add(new Claim("OfficialId", officialId.Value.ToString()));
            }

            // Add role claims for authorization
            foreach (var permission in permissions)
            {
                claims.Add(new Claim(ClaimTypes.Role, permission.PermissionName));
            }

            // Define token properties
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes), // Set expiration
                Issuer = _configuration["Jwt:Issuer"] ?? "Whistl3r", // Who issued it
                Audience = _configuration["Jwt:Audience"] ?? "Whistl3rAPI", // Who it's for
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) // Sign with HMAC SHA256
            };

            var token = tokenHandler.CreateToken(tokenDescriptor); // Create token object
            return tokenHandler.WriteToken(token); // Convert to string
        }

        /// <summary>
        /// Generates cryptographically secure random refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64]; // 64 bytes of randomness
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber); // Fill with random values
                return Convert.ToBase64String(randomNumber); // Convert to base64 string
            }
        }

        /// <summary>
        /// Extracts claims principal from expired token (for refresh token flow)
        /// </summary>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            // Configure validation - skip lifetime check for refresh flow
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, // Don't validate audience
                ValidateIssuer = false, // Don't validate issuer
                ValidateIssuerSigningKey = true, // DO validate signing key
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)),
                ValidateLifetime = false // Allow expired tokens for refresh
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Validate token and extract claims
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                // Verify it's a JWT and uses HMAC SHA256
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    return null; // Invalid algorithm
                }

                return principal; // Return extracted claims
            }
            catch
            {
                return null; // Validation failed
            }
        }

        /// <summary>
        /// Validates token with full validation including expiration
        /// </summary>
        public bool ValidateToken(string token, out ClaimsPrincipal? principal)
        {
            // Configure validation with all checks enabled
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true, // DO validate audience
                ValidAudience = _configuration["Jwt:Audience"] ?? "Whistl3rAPI",
                ValidateIssuer = true, // DO validate issuer
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "Whistl3r",
                ValidateIssuerSigningKey = true, // DO validate signing key
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey)),
                ValidateLifetime = true // DO validate expiration
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Validate and extract principal
                principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                return principal != null;
            }
            catch
            {
                principal = null; // Validation failed
                return false;
            }
        }
    }

    /// <summary>
    /// Main authentication service handling login, token management, and password reset
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService; // User data operations
        private readonly IRoleService _roleService; // Role data operations
        private readonly IJwtService _jwtService; // Token operations
        private readonly IConfiguration _configuration; // App settings
        private readonly Dictionary<string, TokenRevocation> _revokedTokens = new(); // In-memory revoked tokens
        private readonly IHttpClientFactory _httpClientFactory; // HTTP client for service calls
        private readonly ILogger<AuthenticationService> _logger; // Logger
        private readonly ApplicationDbContext _context; // Database context

        private readonly int _expirationMinutes;
        private readonly IDynamoDBContext? _dynamoContext;
        /// <summary>
        /// Initializes authentication service with required dependencies
        /// </summary>
        public AuthenticationService(
            IUserService userService,
            IRoleService roleService,
            IJwtService jwtService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AuthenticationService> logger,
            ApplicationDbContext context,
            IDynamoDBContext? dynamoContext = null)
        {
            _userService = userService;
            _roleService = roleService;
            _jwtService = jwtService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _context = context;
            _dynamoContext = dynamoContext;
            _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");
        }

        /// <summary>
        /// Authenticates user with email/password and returns tokens on success
        /// </summary>
        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                // Get user by email
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    // Generic message for security (don't reveal if email exists)
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Verify password (TODO: in production, use proper hashing like bcrypt)
                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User account is inactive"
                    };
                }

                // Get all roles assigned to user
                var roles = new List<Role>();
                var permissions = new List<Permission>();
                if (user.UserRoles.Any())
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        var r = await _roleService.GetRoleByIdAsync(userRole.RoleId);
                        if (r != null)
                        {
                            roles.Add(r);
                            var rolePermissions = await _roleService.GetPermissionByIdAsync(r.RoleId);
                            if (rolePermissions != null)
                            {
                                permissions.AddRange(rolePermissions);
                            }
                        }
                    }
                }

                // Fetch official_id if the user is an official
                var officialId = await _context.Database
                    .SqlQuery<int?>($"SELECT official_id AS \"Value\" FROM officials WHERE user_id = {user.UserId}")
                    .FirstOrDefaultAsync();

                // Generate JWT access token and refresh token
                var accessToken = _jwtService.GenerateAccessToken(user, permissions, officialId);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Update last login timestamp
                //user.LastLogin = DateTime.UtcNow;
                //await _userService.UpdateUserAsync(user.UserId, user);
// NEW: Store session in DynamoDB
            var tokenHash = ComputeHash(accessToken); // Hash for privacy
            var expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);
            
            var session = new UserSession
            {
                TokenHash = tokenHash,
                UserId = user.UserId,
                RefreshToken = refreshToken,
                Email = user.Email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                RevokedAt = null, // Active session
                TimeToLive = new DateTimeOffset(expiresAt.AddDays(30)).ToUnixTimeSeconds() // TTL
            };

            // Save to DynamoDB (persists across Lambda invocations)
            if (_dynamoContext != null)
            {
                await _dynamoContext.SaveAsync(session);
                _logger.LogInformation($"Session created for user {user.UserId}: {tokenHash}");
            }
            else
            {
                _logger.LogWarning("DynamoDB context not available - session not persisted");
            }

                // Return successful result with tokens
                return new AuthResult
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = user,
                    Roles = roles.Select(r => r.RoleName),
                    ExpiresIn = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60 // Convert to seconds
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
private string ComputeHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
        /// <summary>
        /// Generates new access token from valid refresh token
        /// </summary>
            public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Refresh token is required"
                };
            }

            if (_dynamoContext == null)
            {
                _logger.LogWarning("DynamoDB context not available - cannot verify refresh token");
                return new AuthResult { Success = false, Message = "Session management unavailable" };
            }

            // NEW: Find session by refresh token using scan (or create GSI for better performance)
            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("RefreshToken", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, refreshToken),
                new ScanCondition("RevokedAt", Amazon.DynamoDBv2.DocumentModel.ScanOperator.IsNull)
            };

            var sessions = await _dynamoContext.ScanAsync<UserSession>(scanConditions).GetRemainingAsync();
            var session = sessions.FirstOrDefault();

            if (session == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid or expired refresh token"
                };
            }

            // Get user and roles
            var user = await _userService.GetUserByIdAsync(session.UserId);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var roles = new List<Role>();
            var permissions = new List<Permission>();
            foreach (var userRole in user.UserRoles)
            {
                var role = await _roleService.GetRoleByIdAsync(userRole.RoleId);
                if (role != null) roles.Add(role);
            }

            // Fetch official_id if the user is an official
            var officialId = await _context.Database
                .SqlQuery<int?>($"SELECT official_id AS \"Value\" FROM officials WHERE user_id = {user.UserId}")
                .FirstOrDefaultAsync();

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user, permissions, officialId);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Revoke old session
            session.RevokedAt = DateTime.UtcNow;
            if (_dynamoContext != null)
            {
                await _dynamoContext.SaveAsync(session);
            }

            // Create new session
            var tokenHash = ComputeHash(newAccessToken);
            var expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);
            
            var newSession = new UserSession
            {
                TokenHash = tokenHash,
                UserId = user.UserId,
                RefreshToken = newRefreshToken,
                Email = user.Email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                RevokedAt = null,
                TimeToLive = new DateTimeOffset(expiresAt.AddDays(30)).ToUnixTimeSeconds()
            };

            if (_dynamoContext != null)
            {
                await _dynamoContext.SaveAsync(newSession);
            }

            return new AuthResult
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = user,
                Roles = roles.Select(r => r.RoleName),
                ExpiresIn = _expirationMinutes * 60
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Token refresh failed: {ex.Message}");
            return new AuthResult
            {
                Success = false,
                Message = $"Token refresh failed: {ex.Message}"
            };
        }
    }

        /// <summary>
        /// Validates if token is still valid and not revoked
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
        {
            // First check JWT signature and expiration
            if (!_jwtService.ValidateToken(token, out var principal))
            {
                return false; // Invalid or expired JWT
            }

            if (_dynamoContext == null)
            {
                _logger.LogWarning("DynamoDB context not available - skipping session validation");
                return true; // Allow token if DynamoDB not available (local dev)
            }

            // NEW: Check DynamoDB for revocation
            var tokenHash = ComputeHash(token);
            var session = await _dynamoContext.LoadAsync<UserSession>(tokenHash);

            if (session == null)
            {
                _logger.LogWarning($"Token not found in session store: {tokenHash}");
                return false; // Token never existed or TTL expired
            }

            if (session.RevokedAt != null)
            {
                _logger.LogWarning($"Token was revoked at {session.RevokedAt}: {tokenHash}");
                return false; // Token was explicitly revoked
            }

            return true; // Token is valid and not revoked
        }
        catch (Exception ex)
        {
            _logger.LogError($"Token validation failed: {ex.Message}");
            return false;
        }
        }

        /// <summary>
        /// Extracts user claims from a token
        /// </summary>
        public async Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token)
        {
            // First validate the token
            if (!await ValidateTokenAsync(token))
            {
                return Enumerable.Empty<Claim>();
            }

            // Extract claims from token
            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            return principal?.Claims ?? Enumerable.Empty<Claim>();
        }

        /// <summary>
        /// Generates a new refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            return _jwtService.GenerateRefreshToken();
        }

        /// <summary>
        /// Revokes a token, adding it to the revoked tokens list
        /// </summary>
         public async Task<bool> RevokeTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return false;

            if (_dynamoContext == null)
            {
                _logger.LogWarning("DynamoDB context not available - cannot revoke token");
                return false;
            }

            // Load session from DynamoDB
            var tokenHash = ComputeHash(token);
            var session = await _dynamoContext.LoadAsync<UserSession>(tokenHash);

            if (session == null)
            {
                _logger.LogWarning($"Attempted to revoke non-existent token: {tokenHash}");
                return false;
            }

            // Mark as revoked
            session.RevokedAt = DateTime.UtcNow;
            if (_dynamoContext != null)
            {
                await _dynamoContext.SaveAsync(session);
            }
            
            _logger.LogInformation($"Token revoked for user {session.UserId}: {tokenHash}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Token revocation failed: {ex.Message}");
            return false;
        }
    }

        /// <summary>
        /// Verifies if password matches stored hash
        /// TODO: In production, use proper password hashing like bcrypt
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            // INSECURE: Simple string comparison - REPLACE IN PRODUCTION with bcrypt
            return password == hash;
        }

        /// <summary>
        /// Converts Unix timestamp (seconds since 1970) to DateTime
        /// </summary>
        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // Unix epoch
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime(); // Add seconds
            return dateTime;
        }

        /// <summary>
        /// Initiates password reset by generating reset code and sending email
        /// </summary>
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

                // Look up user by email
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

                // Generate unique reset GUID
                var resetGuid = Guid.NewGuid();
                user.ResetPasswordGuid = resetGuid;

                // Save reset GUID to user record
                await _userService.UpdateUserAsync(user.UserId, user);

                // Get Communication service URL from config
                var communicationServiceUrl = _configuration["Services:Communication"] ?? "http://localhost:5003";
                // Create HTTP client for calling Communication service
                var httpClient = _httpClientFactory.CreateClient();

                // Build email request payload
                var emailRequest = new
                {
                    to = user.Email,
                    subject = "Whistl3r Password Reset Request",
                    body = $"Your password reset code is: {resetGuid}\n\nThis code can be used to reset your password. If you did not request this, please ignore this email.",
                    isHtml = false // Send as plain text
                };

                try
                {
                    _logger.LogInformation($"Attempting to send password reset email to {user.Email} via {communicationServiceUrl}");
                    
                    // POST to Communication service to send email
                    var response = await httpClient.PostAsJsonAsync($"{communicationServiceUrl}/api/email/send", emailRequest);
                    
                    // Log response for debugging
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Communication service response: Status={response.StatusCode}, Body={responseContent}");
                    
                    // Check if email sending failed
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
                    // Handle Communication service errors (network issues, service down, etc.)
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

        /// <summary>
        /// Resets user password using reset GUID and new password
        /// </summary>
        public async Task<AuthResult> ResetPasswordAsync(string resetGuid, string newPassword)
        {
            try
            {
                // Validate input
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

                // Update password (TODO: in production, hash password properly with bcrypt)
                user.PasswordHash = newPassword; // INSECURE: Should be hashed
                user.ResetPasswordGuid = null; // Clear reset GUID so it can't be reused

                // Save updated user
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
