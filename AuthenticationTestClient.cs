using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuthenticationTestClient
{
    class Program
    {
        private static readonly string BaseUrl = "http://localhost:5096/api/auth";
        private static readonly HttpClient Client = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Whistl3r Authentication Test Client ===\n");

            try
            {
                // Step 1: Get encryption key
                Console.WriteLine("Step 1: Retrieving RSA public key for password encryption...");
                var (publicKey, algorithm) = await GetEncryptionKey();
                Console.WriteLine($"✓ Received public key (algorithm: {algorithm})\n");

                // Step 2: Create test password and encrypt it
                Console.WriteLine("Step 2: Encrypting password...");
                string testPassword = "SecurePassword123!";
                string encryptedPassword = EncryptPassword(testPassword, publicKey);
                Console.WriteLine($"✓ Password encrypted successfully\n");

                // Step 3: Register new user
                Console.WriteLine("Step 3: Registering new user...");
                var registerRequest = new
                {
                    email = "john.doe@example.com",
                    firstName = "John",
                    lastName = "Doe",
                    encryptedPassword = encryptedPassword
                };

                var registrationResult = await RegisterUser(registerRequest);
                Console.WriteLine($"✓ Registration response:");
                Console.WriteLine($"  Success: {registrationResult.GetProperty("success").GetBoolean()}");
                Console.WriteLine($"  Message: {registrationResult.GetProperty("message").GetString()}");
                
                var user = registrationResult.GetProperty("user");
                Console.WriteLine($"  User ID: {user.GetProperty("userId").GetInt32()}");
                Console.WriteLine($"  Email: {user.GetProperty("email").GetString()}");
                Console.WriteLine($"  Is Active: {user.GetProperty("isActive").GetBoolean()}\n");

                // Step 4: Attempt login (should fail because account is inactive)
                Console.WriteLine("Step 4: Attempting login (account is inactive)...");
                var loginRequest = new
                {
                    email = "john.doe@example.com",
                    password = testPassword
                };

                var loginResult = await Login(loginRequest);
                Console.WriteLine($"Login response status: {loginResult.GetProperty("message").GetString()}\n");

                Console.WriteLine("=== Test Complete ===");
                Console.WriteLine("\nKey Points:");
                Console.WriteLine("- New users are registered with IsActive = false");
                Console.WriteLine("- Passwords are encrypted on client-side using RSA public key");
                Console.WriteLine("- Server decrypts password using RSA private key");
                Console.WriteLine("- Inactive users cannot login until account is activated");
                Console.WriteLine("- Use the Swagger UI at http://localhost:5096/swagger to test endpoints manually");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        static async Task<(string publicKey, string algorithm)> GetEncryptionKey()
        {
            var response = await Client.GetAsync($"{BaseUrl}/encryption-key");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            return (
                json.GetProperty("publicKey").GetString()!,
                json.GetProperty("algorithm").GetString()!
            );
        }

        static string EncryptPassword(string password, string publicKeyBase64)
        {
            // Decode the base64 public key
            var publicKeyBlob = Convert.FromBase64String(publicKeyBase64);

            // Create RSA instance and import the public key
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBlob, out _);

            // Convert password to bytes
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Encrypt using RSA with OAEP padding and SHA256
            var encryptedBytes = rsa.Encrypt(passwordBytes, RSAEncryptionPadding.OaepSHA256);

            // Return as base64 string
            return Convert.ToBase64String(encryptedBytes);
        }

        static async Task<JsonElement> RegisterUser(object request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"{BaseUrl}/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Registration failed: {response.StatusCode} - {responseContent}");
            }

            return JsonDocument.Parse(responseContent).RootElement;
        }

        static async Task<JsonElement> Login(object request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync($"{BaseUrl}/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(responseContent).RootElement;
        }
    }
}
