using System.Security.Cryptography;
using System.Text;

namespace UsersService.Services
{
    public interface IEncryptionService
    {
        string GenerateRsaPublicKey();
        string DecryptPassword(string encryptedPassword);
        EncryptionKeys GetEncryptionKeys();
    }

    public class EncryptionKeys
    {
        public string PublicKey { get; set; } = null!;
        public string Algorithm { get; set; } = "RSA-2048";
    }

    public class EncryptionService : IEncryptionService
    {
        private RSA _rsa;
        private readonly ILogger<EncryptionService> _logger;

        public EncryptionService(ILogger<EncryptionService> logger)
        {
            _logger = logger;
            _rsa = RSA.Create(2048);
        }

        public string GenerateRsaPublicKey()
        {
            try
            {
                // Export in PEM format for JSEncrypt compatibility
                var publicKeyPem = _rsa.ExportSubjectPublicKeyInfoPem();
                return publicKeyPem;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating RSA public key: {ex.Message}");
                throw;
            }
        }

        public string DecryptPassword(string encryptedPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(encryptedPassword))
                {
                    throw new ArgumentException("Encrypted password cannot be null or empty");
                }

                _logger.LogInformation($"Attempting to decrypt password. Length: {encryptedPassword.Length}");
                _logger.LogInformation($"Encrypted password: {encryptedPassword.Substring(0, Math.Min(50, encryptedPassword.Length))}...");

                var encryptedBytes = Convert.FromBase64String(encryptedPassword);
                _logger.LogInformation($"Decoded {encryptedBytes.Length} bytes from base64");
                
                // Try PKCS1 first
                try
                {
                    var decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
                    var result = Encoding.UTF8.GetString(decryptedBytes);
                    _logger.LogInformation("Successfully decrypted with PKCS1 padding");
                    return result;
                }
                catch (CryptographicException ex1)
                {
                    _logger.LogWarning($"PKCS1 decryption failed: {ex1.Message}, trying OaepSHA1");
                    
                    // Try OaepSHA1 as fallback
                    var decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA1);
                    var result = Encoding.UTF8.GetString(decryptedBytes);
                    _logger.LogInformation("Successfully decrypted with OaepSHA1 padding");
                    return result;
                }
            }
            catch (FormatException)
            {
                _logger.LogError("Invalid base64 format for encrypted password");
                throw new InvalidOperationException("Encrypted password format is invalid");
            }
            catch (CryptographicException ex)
            {
                _logger.LogError($"Decryption failed: {ex.Message}");
                throw new InvalidOperationException("Failed to decrypt password");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error decrypting password: {ex.Message}");
                throw;
            }
        }

        public EncryptionKeys GetEncryptionKeys()
        {
            return new EncryptionKeys
            {
                PublicKey = GenerateRsaPublicKey(),
                Algorithm = "RSA-2048"
            };
        }
    }
}
