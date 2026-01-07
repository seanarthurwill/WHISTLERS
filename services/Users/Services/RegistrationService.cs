using UsersService.Models;

namespace UsersService.Services
{
    public interface IRegistrationService
    {
        Task<RegistrationResult> RegisterUserAsync(RegisterRequest request);
        Task<RegistrationResult> ValidateRegistrationAsync(RegisterRequest request);
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Phone { get; set; }
        public int LeagueId { get; set; }
        public int RoleId { get; set; }
        public string EncryptedPassword { get; set; } = null!;
    }

    public class RegistrationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public User? User { get; set; }
        public Dictionary<string, string>? ValidationErrors { get; set; }
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(
            IUserService userService,
            IRoleService roleService,
            IEncryptionService encryptionService,
            ILogger<RegistrationService> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<RegistrationResult> ValidateRegistrationAsync(RegisterRequest request)
        {
            var errors = new Dictionary<string, string>();

            // Validate email
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                errors["email"] = "Email is required";
            }
            else if (!IsValidEmail(request.Email))
            {
                errors["email"] = "Email format is invalid";
            }
            else
            {
                // Check if email already exists
                var existingUser = await _userService.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    errors["email"] = "Email is already registered";
                }
            }

            // Validate first name
            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                errors["firstName"] = "First name is required";
            }
            else if (request.FirstName.Length < 2)
            {
                errors["firstName"] = "First name must be at least 2 characters";
            }

            // Validate last name
            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                errors["lastName"] = "Last name is required";
            }
            else if (request.LastName.Length < 2)
            {
                errors["lastName"] = "Last name must be at least 2 characters";
            }

            // Validate encrypted password
            if (string.IsNullOrWhiteSpace(request.EncryptedPassword))
            {
                errors["password"] = "Password is required";
            }

            // Validate role
            if (request.RoleId <= 0)
            {
                errors["roleId"] = "Role is required";
            }
            else
            {
                var role = await _roleService.GetRoleByIdAsync(request.RoleId);
                if (role == null)
                {
                    errors["roleId"] = "Invalid role selected";
                }
            }

            if (errors.Count > 0)
            {
                return new RegistrationResult
                {
                    Success = false,
                    Message = "Validation failed",
                    ValidationErrors = errors
                };
            }

            return new RegistrationResult { Success = true };
        }

        public async Task<RegistrationResult> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                // Validate request
                var validationResult = await ValidateRegistrationAsync(request);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Decrypt password
                string decryptedPassword;
                Console.WriteLine(request.EncryptedPassword);
                try
                {
                    decryptedPassword = _encryptionService.DecryptPassword(request.EncryptedPassword);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Password decryption failed: {ex.Message}");
                    return new RegistrationResult
                    {
                        Success = false,
                        Message = "Failed to process password",
                        ValidationErrors = new Dictionary<string, string> { { "password", "Invalid encrypted password format" } }
                    };
                }

                // Validate decrypted password
                if (string.IsNullOrWhiteSpace(decryptedPassword) || decryptedPassword.Length < 8)
                {
                    return new RegistrationResult
                    {
                        Success = false,
                        Message = "Validation failed",
                        ValidationErrors = new Dictionary<string, string> 
                        { 
                            { "password", "Password must be at least 8 characters long" } 
                        }
                    };
                }

                // Create new user with IsActive = false
                var newUser = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone ?? "",
                    RoleId = request.RoleId,
                    PasswordHash = decryptedPassword, // In production, hash this with bcrypt
                    IsActive = false, // New registrations are inactive until verified
                    CreatedAt = DateTime.UtcNow
                };

                // Save user
                var createdUser = await _userService.CreateUserAsync(newUser);

                if (createdUser == null)
                {
                    return new RegistrationResult
                    {
                        Success = false,
                        Message = "Failed to create user account"
                    };
                }

                _logger.LogInformation($"New user registered: {createdUser.Email}");

                return new RegistrationResult
                {
                    Success = true,
                    Message = "User registered successfully. Please check your email to verify your account.",
                    User = createdUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return new RegistrationResult
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
