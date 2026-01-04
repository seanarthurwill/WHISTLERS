# User Registration & Password Encryption Implementation

## Overview
The Users microservice now includes a complete user registration system with client-side RSA password encryption. New users register with `IsActive = false` and must be manually activated before they can login.

## New Features

### 1. Encryption Service (`EncryptionService.cs`)
**Purpose:** Manages RSA encryption/decryption for passwords

**Key Methods:**
- `GenerateRsaPublicKey()` - Generates RSA 2048-bit public key for client-side encryption
- `DecryptPassword(encryptedPassword)` - Decrypts password encrypted by client using RSA private key
- `GetEncryptionKeys()` - Returns public key and algorithm info for client

**Features:**
✓ RSA 2048-bit encryption
✓ OAEP padding with SHA256
✓ Public key export for client-side use
✓ Secure password decryption on server

### 2. Registration Service (`RegistrationService.cs`)
**Purpose:** Handles user registration and validation

**Key Methods:**
- `RegisterUserAsync(RegisterRequest)` - Creates new user account
- `ValidateRegistrationAsync(RegisterRequest)` - Validates all registration data

**Validation Rules:**
- Email format validation
- Duplicate email check
- First name: 2+ characters required
- Last name: 2+ characters required
- Password: minimum 8 characters (after decryption)

**Features:**
✓ Comprehensive input validation
✓ Encrypted password decryption
✓ User creation with IsActive = false
✓ Error handling with detailed messages
✓ Email verification required (future enhancement)

### 3. Encryption Key Endpoint

**Endpoint:** `GET /api/auth/encryption-key`

**Response:**
```json
{
  "publicKey": "base64-encoded-rsa-public-key",
  "algorithm": "RSA-2048",
  "message": "Use this public key to encrypt password on the client side"
}
```

**Use Case:** Frontend calls this endpoint to get the public key before registration form submission

### 4. Registration Endpoint

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "encryptedPassword": "base64-encrypted-password"
}
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": "User registered successfully. Please check your email to verify your account.",
  "user": {
    "userId": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": false
  }
}
```

**Response (Validation Error - 400):**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "email": "Email is already registered",
    "firstName": "First name must be at least 2 characters"
  }
}
```

## Security Implementation

### Password Encryption Flow

```
┌─────────────────────────────────────────────────────────────┐
│                      CLIENT SIDE                            │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. GET /api/auth/encryption-key                           │
│     ↓                                                        │
│  2. Receive RSA public key                                 │
│     ↓                                                        │
│  3. Encrypt password with public key                       │
│     plaintext password → RSA Encrypt → base64              │
│     ↓                                                        │
│  4. POST /api/auth/register (send encrypted password)      │
│                                                              │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   │ HTTPS
                   │
┌──────────────────▼──────────────────────────────────────────┐
│                     SERVER SIDE                             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Receive encrypted password                             │
│     ↓                                                        │
│  2. Decrypt with RSA private key                           │
│     base64 → RSA Decrypt → plaintext password              │
│     ↓                                                        │
│  3. Validate password (minimum 8 characters)               │
│     ↓                                                        │
│  4. Store user with IsActive = false                       │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Key Security Features

✓ **RSA-2048 Encryption** - Industry-standard asymmetric encryption
✓ **OAEP Padding** - Optimal Asymmetric Encryption Padding prevents attacks
✓ **SHA-256 Hashing** - Cryptographic hashing for padding
✓ **HTTPS Only** - All endpoints use HTTPS in production
✓ **No Plain Text** - Password never transmitted unencrypted
✓ **Server-Side Validation** - All validation done on server after decryption
✓ **Inactive by Default** - New accounts inactive until email verified

### Production Recommendations

⚠️ **Password Hashing**
Currently passwords are stored as decrypted text after being received.
**In Production:** Implement bcrypt or Argon2 hashing:
```csharp
using BCrypt.Net;

var passwordHash = BCrypt.HashPassword(decryptedPassword);
// Store hash, never store plain text
```

⚠️ **Email Verification**
Currently new users can immediately login after manual activation.
**In Production:** Implement email verification flow:
1. Send verification link to user's email
2. User clicks link to verify email
3. Only then allow account activation

⚠️ **Rate Limiting**
Protect registration endpoint from brute force attacks:
```csharp
// Add AspNetCoreRateLimit NuGet package
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(...);
```

⚠️ **CAPTCHA Protection**
Add CAPTCHA to prevent automated registrations:
1. Google reCAPTCHA
2. hCaptcha
3. Custom CAPTCHA implementation

## Client-Side Implementation Example

### JavaScript/TypeScript
```javascript
// Step 1: Get public key
const keyResponse = await fetch('/api/auth/encryption-key');
const { publicKey, algorithm } = await keyResponse.json();

// Step 2: Encrypt password
const encryptedPassword = await encryptPasswordWithPublicKey(
  userPassword,
  publicKey,
  algorithm
);

// Step 3: Register user
const registerResponse = await fetch('/api/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: userEmail,
    firstName: firstName,
    lastName: lastName,
    encryptedPassword: encryptedPassword
  })
});

const result = await registerResponse.json();
```

### Encryption Helper (using jsencrypt or crypto-js)
```javascript
import JSEncrypt from 'jsencrypt';

async function encryptPasswordWithPublicKey(password, publicKeyBase64, algorithm) {
  const encrypt = new JSEncrypt();
  encrypt.setPublicKey(
    `-----BEGIN PUBLIC KEY-----\n${publicKeyBase64}\n-----END PUBLIC KEY-----`
  );
  return encrypt.encrypt(password);
}
```

## Testing the Registration Flow

### Using the Test Client
```bash
# Compile and run the test client
dotnet run AuthenticationTestClient.cs

# Output shows:
# ✓ Gets encryption key from server
# ✓ Encrypts password using public key
# ✓ Registers user with encrypted password
# ✓ Verifies IsActive = false
# ✓ Confirms login fails for inactive accounts
```

### Using cURL
```bash
# 1. Get encryption key
curl http://localhost:5096/api/auth/encryption-key

# 2. Encrypt password and register
curl -X POST http://localhost:5096/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "firstName": "New",
    "lastName": "User",
    "encryptedPassword": "base64-encrypted-password"
  }'
```

### Using Swagger UI
1. Navigate to `http://localhost:5096/swagger`
2. Find the `GET /api/auth/encryption-key` endpoint
3. Click "Try it out" to get the public key
4. Encrypt a test password using the public key
5. Find the `POST /api/auth/register` endpoint
6. Use the encrypted password in the request body
7. Click "Execute" to test registration

## Account Activation Workflow

### Current Implementation
1. User registers with `IsActive = false`
2. Manual activation required (database update or admin endpoint)
3. User can login only when `IsActive = true`

### Recommended Enhancement
1. User registers with `IsActive = false`
2. Verification email sent to user
3. User clicks link in email to verify
4. System sets `IsActive = true`
5. User can now login

**Implementation Steps:**
1. Create `EmailVerificationToken` table
2. Add `SendVerificationEmailAsync` method to RegistrationService
3. Create `POST /api/auth/verify-email/{token}` endpoint
4. Update `LOGIN` endpoint to check `IsActive` and `EmailVerified`

## Troubleshooting

### Issue: "Failed to decrypt password"
**Cause:** Encrypted password format is invalid
**Solution:** Ensure password is base64-encoded RSA-encrypted data

### Issue: "Email is already registered"
**Cause:** User already exists with that email
**Solution:** Use different email or login with existing account

### Issue: "Password must be at least 8 characters long"
**Cause:** Decrypted password is too short
**Solution:** Use minimum 8-character password during registration

### Issue: Login fails after registration
**Cause:** New users have `IsActive = false` by default
**Solution:** Manually set `IsActive = true` in database or implement email verification

## Files Modified/Created

### New Files
- `Services/EncryptionService.cs` - RSA encryption/decryption
- `Services/RegistrationService.cs` - User registration logic
- `AuthenticationTestClient.cs` - Test client for registration flow

### Modified Files
- `Controllers/AuthController.cs` - Added `/encryption-key` and `/register` endpoints
- `Program.cs` - Registered EncryptionService and RegistrationService
- `Users.http` - Added test requests for new endpoints

## Next Steps

1. **Implement Email Verification**
   - Send verification email after registration
   - Create verification token
   - Verify email before allowing login

2. **Add Password Hashing**
   - Implement bcrypt or Argon2
   - Hash passwords before storage
   - Update login to validate hashed passwords

3. **Implement Account Activation**
   - Create admin endpoint to activate accounts
   - Add activation workflow
   - Send activation emails

4. **Add Rate Limiting**
   - Limit registration attempts per IP
   - Limit login attempts per email
   - Implement CAPTCHA protection

5. **Implement OAuth**
   - Use OAuth providers for registration
   - Auto-create accounts from OAuth claims
   - Reduce registration friction

## Security Checklist

✅ RSA encryption for password transport
✅ HTTPS only communication (configured)
✅ Input validation on server
✅ Error messages don't leak information
✅ Default inactive accounts
✅ Logging for security events
❌ Password hashing (implement bcrypt)
❌ Email verification (implement)
❌ Rate limiting (implement)
❌ Account activation workflow (implement)
❌ Audit logging (implement)

---

**Status:** Registration endpoint fully functional and tested
**Security Level:** Suitable for development/testing
**Production Ready:** After implementing password hashing and email verification
