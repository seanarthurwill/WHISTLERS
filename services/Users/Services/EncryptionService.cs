// Import namespace for cryptographic operations (RSA encryption/decryption)
using System.Security.Cryptography;
// Import namespace for encoding/decoding text to bytes
using System.Text;

namespace UsersService.Services
{
    /// <summary>
    /// Interface defining encryption service contract for RSA operations
    /// </summary>
    public interface IEncryptionService
    {
        // Method to generate and return the RSA public key for client-side encryption
        string GenerateRsaPublicKey();
        // Method to decrypt password that was encrypted with the public key
        string DecryptPassword(string encryptedPassword);
        // Method to retrieve encryption configuration (public key + algorithm type)
        EncryptionKeys GetEncryptionKeys();
    }

    /// <summary>
    /// Data transfer object containing encryption configuration
    /// </summary>
    public class EncryptionKeys
    {
        // The RSA public key in PEM format for client-side password encryption
        public string PublicKey { get; set; } = null!;
        // The encryption algorithm being used (RSA with 2048-bit key)
        public string Algorithm { get; set; } = "RSA-2048";
    }

    /// <summary>
    /// Service that handles RSA encryption/decryption for secure password transmission
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        // RSA cryptographic provider instance that holds the public/private key pair
        private RSA _rsa;
        // Logger instance for recording encryption/decryption operations and errors
        private readonly ILogger<EncryptionService> _logger;

        /// <summary>
        /// Constructor that initializes the RSA provider with a 2048-bit key
        /// </summary>
        public EncryptionService(ILogger<EncryptionService> logger)
        {
            // Store the logger for use throughout the service
            _logger = logger;
            // Create a new RSA instance with 2048-bit key length (secure standard)
            _rsa = RSA.Create(2048);
        }

        /// <summary>
        /// Generates and returns the RSA public key in PEM format for client use
        /// </summary>
        public string GenerateRsaPublicKey()
        {
            try
            {
                // Export the public key in PEM format - compatible with JavaScript JSEncrypt library
                var publicKeyPem = _rsa.ExportSubjectPublicKeyInfoPem();
                // Return the PEM-formatted public key string
                return publicKeyPem;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during key generation
                _logger.LogError($"Error generating RSA public key: {ex.Message}");
                // Re-throw the exception to be handled by the caller
                throw;
            }
        }

        /// <summary>
        /// Decrypts a password that was encrypted by the client using the public key
        /// </summary>
        public string DecryptPassword(string encryptedPassword)
        {
            try
            {
                // Validate that the encrypted password string is not null or whitespace
                if (string.IsNullOrWhiteSpace(encryptedPassword))
                {
                    // Throw an exception if validation fails
                    throw new ArgumentException("Encrypted password cannot be null or empty");
                }

                // Log the decryption attempt with the encrypted password length
                _logger.LogInformation($"Attempting to decrypt password. Length: {encryptedPassword.Length}");
                // Log the first 50 characters of the encrypted password for debugging
                _logger.LogInformation($"Encrypted password: {encryptedPassword.Substring(0, Math.Min(50, encryptedPassword.Length))}...");

                // Convert the base64-encoded string to a byte array
                var encryptedBytes = Convert.FromBase64String(encryptedPassword);
                // Log how many bytes were decoded from the base64 string
                _logger.LogInformation($"Decoded {encryptedBytes.Length} bytes from base64");
                
                // Try PKCS1 padding first (most common padding scheme for RSA)
                try
                {
                    // Decrypt the bytes using RSA with PKCS1 padding
                    var decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
                    // Convert the decrypted bytes back to a UTF-8 string
                    var result = Encoding.UTF8.GetString(decryptedBytes);
                    // Log successful decryption
                    _logger.LogInformation("Successfully decrypted with PKCS1 padding");
                    // Return the decrypted password
                    return result;
                }
                catch (CryptographicException ex1)
                {
                    // Log that PKCS1 failed and we're trying the fallback padding scheme
                    _logger.LogWarning($"PKCS1 decryption failed: {ex1.Message}, trying OaepSHA1");
                    
                    // Try OaepSHA1 padding as fallback (alternative padding scheme)
                    var decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA1);
                    // Convert the decrypted bytes back to a UTF-8 string
                    var result = Encoding.UTF8.GetString(decryptedBytes);
                    // Log successful decryption with fallback padding
                    _logger.LogInformation("Successfully decrypted with OaepSHA1 padding");
                    // Return the decrypted password
                    return result;
                }
            }
            catch (FormatException)
            {
                // Handle case where the encrypted password is not valid base64
                _logger.LogError("Invalid base64 format for encrypted password");
                // Throw a more user-friendly exception
                throw new InvalidOperationException("Encrypted password format is invalid");
            }
            catch (CryptographicException ex)
            {
                // Handle case where decryption fails (wrong key, corrupted data, etc.)
                _logger.LogError($"Decryption failed: {ex.Message}");
                // Throw a more user-friendly exception
                throw new InvalidOperationException("Failed to decrypt password");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                _logger.LogError($"Error decrypting password: {ex.Message}");
                // Re-throw the exception to be handled by the caller
                throw;
            }
        }

        /// <summary>
        /// Returns the encryption configuration including the public key
        /// </summary>
        public EncryptionKeys GetEncryptionKeys()
        {
            // Create and return a new EncryptionKeys object
            return new EncryptionKeys
            {
                // Generate the current RSA public key
                PublicKey = GenerateRsaPublicKey(),
                // Specify the algorithm type (RSA with 2048-bit key)
                Algorithm = "RSA-2048"
            };
        }
    }
}
