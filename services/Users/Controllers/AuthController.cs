using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UsersService.Services;

namespace UsersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IRegistrationService _registrationService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticationService authenticationService,
            IUserService userService,
            IRegistrationService registrationService,
            IEncryptionService encryptionService,
            ILogger<AuthController> logger)
        {
            _authenticationService = authenticationService;
            _userService = userService;
            _registrationService = registrationService;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        /// <summary>
        /// Get RSA public key for client-side password encryption
        /// </summary>
        /// <returns>RSA public key and algorithm information</returns>
        [HttpGet("encryption-key")]
        [AllowAnonymous]
        public IActionResult GetEncryptionKey()
        {
            try
            {
                var keys = _encryptionService.GetEncryptionKeys();
                return Ok(new
                {
                    publicKey = keys.PublicKey,
                    algorithm = keys.Algorithm,
                    message = "Use this public key to encrypt password on the client side"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving encryption key: {ex.Message}");
                return StatusCode(500, new { message = "Failed to retrieve encryption key" });
            }
        }

        /// <summary>
        /// Register a new user with encrypted password
        /// </summary>
        /// <param name="request">Registration request with encrypted password</param>
        /// <returns>User registration confirmation</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.FirstName) || 
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.EncryptedPassword) ||
                request.RoleIds == null || request.RoleIds.Count == 0)
            {
                return BadRequest(new { message = "Email, first name, last name, password, and role are required" });
            }

            try
            {
                var result = await _registrationService.RegisterUserAsync(request);

                if (!result.Success)
                {
                    _logger.LogWarning($"Registration failed for email: {request.Email}");
                    return BadRequest(new 
                    { 
                        success = false,
                        message = result.Message,
                        errors = result.ValidationErrors
                    });
                }

                _logger.LogInformation($"User registered successfully: {request.Email}");

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    user = new
                    {
                        userId = result.User?.UserId,
                        email = result.User?.Email,
                        firstName = result.User?.FirstName,
                        lastName = result.User?.LastName,
                        isActive = result.User?.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="request">Login request with email and password</param>
        /// <returns>JWT access token and refresh token with user claims</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            // Decrypt password if encrypted password was provided
            string plainPassword;
            if (!string.IsNullOrWhiteSpace(request.EncryptedPassword))
            {
                try
                {
                    plainPassword = _encryptionService.DecryptPassword(request.EncryptedPassword);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Password decryption failed for {request.Email}: {ex.Message}");
                    return BadRequest(new { message = "Invalid password format" });
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.Password))
            {
                plainPassword = request.Password;
            }
            else
            {
                return BadRequest(new { message = "Password is required" });
            }

            try
            {
                var result = await _authenticationService.LoginAsync(request.Email, plainPassword);

                if (!result.Success)
                {
                    _logger.LogWarning($"Failed login attempt for email: {request.Email}");
                    return Unauthorized(new { message = result.Message });
                }

                _logger.LogInformation($"User {request.Email} logged in successfully");

                return Ok(new
                {
                    success = true,
                    accessToken = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    expiresIn = result.ExpiresIn,
                    user = new
                    {
                        userId = result.User?.UserId,
                        email = result.User?.Email,
                        firstName = result.User?.FirstName,
                        lastName = result.User?.LastName,
                        roles = result.Roles
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New access token</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            try
            {
                var result = await _authenticationService.RefreshTokenAsync(request.RefreshToken);

                if (!result.Success)
                {
                    return Unauthorized(new { message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    accessToken = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    expiresIn = result.ExpiresIn
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token refresh error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during token refresh" });
            }
        }

        /// <summary>
        /// Get current user's profile and claims
        /// </summary>
        /// <returns>Current user information and claims</returns>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new
                {
                    userId = user.UserId,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    isActive = user.IsActive,
                    createdAt = user.CreatedAt,
                    lastLoginAt = user.LastLogin,
                    claims = new
                    {
                        roles = User.FindAll(ClaimTypes.Role),
                        certifications = User.FindAll("Certification"),
                        email = User.FindFirst(ClaimTypes.Email)?.Value,
                        givenName = User.FindFirst(ClaimTypes.GivenName)?.Value,
                        surname = User.FindFirst(ClaimTypes.Surname)?.Value
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching current user: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching user information" });
            }
        }

        /// <summary>
        /// Logout (revoke token)
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                var token = authHeader.Replace("Bearer ", "");

                if (!string.IsNullOrWhiteSpace(token))
                {
                    await _authenticationService.RevokeTokenAsync(token);
                }

                _logger.LogInformation("User logged out");
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        /// <summary>
        /// Validate token
        /// </summary>
        /// <param name="request">Token validation request</param>
        /// <returns>Token validity status</returns>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            try
            {
                var isValid = await _authenticationService.ValidateTokenAsync(request.Token);
                return Ok(new { valid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token validation error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during token validation" });
            }
        }

        /// <summary>
        /// Request a password reset for a user account
        /// </summary>
        /// <param name="request">Request containing the user's email</param>
        /// <returns>Confirmation that reset email was sent (if email exists)</returns>
        [HttpPost("request-password-reset")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request?.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            try
            {
                var result = await _authenticationService.RequestPasswordResetAsync(request.Email);
                
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password reset request error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during password reset request" });
            }
        }

        /// <summary>
        /// Reset password using the reset GUID sent via email
        /// </summary>
        /// <param name="request">Request containing reset GUID and new password</param>
        /// <returns>Confirmation of password reset</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetSubmit request)
        {
            if (!ModelState.IsValid || 
                string.IsNullOrWhiteSpace(request?.ResetGuid) || 
                string.IsNullOrWhiteSpace(request?.NewPassword))
            {
                return BadRequest(new { message = "Reset code and new password are required" });
            }

            try
            {
                var result = await _authenticationService.ResetPasswordAsync(request.ResetGuid, request.NewPassword);
                
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password reset error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during password reset" });
            }
        }
    }

    // ===== REQUEST/RESPONSE MODELS =====

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public string? EncryptedPassword { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = null!;
    }

    public class PasswordResetRequest
    {
        public string Email { get; set; } = null!;
    }

    public class PasswordResetSubmit
    {
        public string ResetGuid { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public IEnumerable<string>? Roles { get; set; }
    }
}
