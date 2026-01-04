# Registration Feature - Quick Start Guide

## 🚀 Quick Reference

### New Endpoints
```
GET  /api/auth/encryption-key      → Get RSA public key for password encryption
POST /api/auth/register             → Register new user with encrypted password
POST /api/auth/login                → Login with email & password
GET  /api/auth/me                   → Get current user (requires auth)
POST /api/auth/validate             → Validate token
POST /api/auth/refresh              → Refresh access token
POST /api/auth/logout               → Logout (revoke token)
```

## 🔐 Encryption Flow (Visual)

```
┌─────────────────────────────────┐
│   CLIENT (Browser/App)          │
├─────────────────────────────────┤
│                                 │
│  1. GET /encryption-key         │
│     ↓                           │
│  2. Receive: {                  │
│       publicKey: "base64...",   │
│       algorithm: "RSA-2048"     │
│     }                           │
│     ↓                           │
│  3. Encrypt password:           │
│     plaintext → RSA → base64    │
│     ↓                           │
│  4. POST /register with         │
│     {                           │
│       email,                    │
│       firstName,                │
│       lastName,                 │
│       encryptedPassword         │
│     }                           │
│                                 │
└────────────┬────────────────────┘
             │ HTTPS
             │
┌────────────▼────────────────────┐
│   SERVER (.NET 8.0)             │
├─────────────────────────────────┤
│                                 │
│  1. Receive encrypted password  │
│     ↓                           │
│  2. Decrypt with RSA private    │
│     base64 → RSA → plaintext    │
│     ↓                           │
│  3. Validate:                   │
│     - Email format              │
│     - Email uniqueness          │
│     - Name lengths (2+)         │
│     - Password length (8+)      │
│     ↓                           │
│  4. Create user:                │
│     - IsActive = false          │
│     - Store in database         │
│     - Return user info          │
│                                 │
└─────────────────────────────────┘
```

## 📋 Step-by-Step Registration

### Step 1️⃣: Get Encryption Key
```powershell
# Get the RSA public key
curl http://localhost:5096/api/auth/encryption-key

# Response:
#{
#  "publicKey": "MIIBIjANBgkqhkiG9w0...",
#  "algorithm": "RSA-2048",
#  "message": "Use this public key..."
#}
```

### Step 2️⃣: Encrypt Password (Client-Side)
```javascript
// JavaScript example
const publicKey = "MIIBIjANBgkqhkiG9w0...";
const password = "SecurePassword123!";

const encrypt = new JSEncrypt();
encrypt.setPublicKey(formatAsPem(publicKey));
const encryptedPassword = encrypt.encrypt(password);

// encryptedPassword is now base64 encoded and safe to send
```

### Step 3️⃣: Send Registration Request
```powershell
# Register the user
curl -X POST http://localhost:5096/api/auth/register `
  -H "Content-Type: application/json" `
  -d '{
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "encryptedPassword": "base64-encrypted-value-here"
  }'

# Response:
#{
#  "success": true,
#  "message": "User registered successfully...",
#  "user": {
#    "userId": 1,
#    "email": "john@example.com",
#    "firstName": "John",
#    "lastName": "Doe",
#    "isActive": false
#  }
#}
```

### Step 4️⃣: User Activation (Admin Only - Future)
```powershell
# Manual database update needed:
# UPDATE Users SET IsActive = 1 WHERE UserId = 1

# Or implement admin endpoint (future):
# PUT /api/admin/users/1/activate
```

### Step 5️⃣: User Can Now Login
```powershell
# Login with the registered account
curl -X POST http://localhost:5096/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{
    "email": "john@example.com",
    "password": "SecurePassword123!"
  }'

# Response:
#{
#  "success": true,
#  "accessToken": "eyJhbGc...",
#  "refreshToken": "base64...",
#  "expiresIn": 3600,
#  "user": {
#    "userId": 1,
#    "email": "john@example.com",
#    "firstName": "John",
#    "lastName": "Doe",
#    "roles": []
#  }
#}
```

## ✅ Validation Rules

| Field | Rules | Example |
|-------|-------|---------|
| Email | Format valid, must be unique | user@example.com ❌ user@example ✓ user@example.com (if not exists) |
| First Name | 2+ characters | John ✓ Jo ❌ J ❌ |
| Last Name | 2+ characters | Doe ✓ Do ❌ D ❌ |
| Password | 8+ characters (after decryption) | SecurePassword123! ✓ Pass123! ❌ |

## 🛡️ Security Checklist

| Feature | Status | Details |
|---------|--------|---------|
| RSA Encryption | ✅ Implemented | 2048-bit, OAEP SHA256 |
| HTTPS Ready | ✅ Configured | Use in production |
| Input Validation | ✅ Implemented | Server-side after decryption |
| Inactive by Default | ✅ Implemented | IsActive = false on registration |
| Error Handling | ✅ Implemented | Safe error messages |
| Password Hashing | ❌ TODO | Implement bcrypt |
| Email Verification | ❌ TODO | Send verification email |
| Rate Limiting | ❌ TODO | Prevent brute force |
| Audit Logging | ❌ TODO | Log registration attempts |

## 📁 Files Created/Modified

### New Files ✨
```
✨ Services/EncryptionService.cs          - RSA encryption/decryption
✨ Services/RegistrationService.cs        - Registration logic
✨ AuthenticationTestClient.cs            - C# test client
✨ wwwroot/js/registration-service.js     - JavaScript client
✨ REGISTRATION_IMPLEMENTATION.md         - Full documentation
✨ REGISTRATION_COMPLETE.md               - This summary
```

### Modified Files 🔄
```
🔄 Controllers/AuthController.cs          - Added 2 endpoints
🔄 Program.cs                             - Added service registrations
🔄 Users.http                             - Added test requests
```

## 🧪 Testing

### Option A: Swagger UI (Easiest)
```
1. Start service: dotnet run
2. Open http://localhost:5096/swagger
3. Find GET /api/auth/encryption-key
4. Click "Try it out" → "Execute"
5. Copy publicKey value
6. Find POST /api/auth/register
7. Paste encrypted password and other fields
8. Click "Execute"
```

### Option B: C# Test Client (Comprehensive)
```bash
# From c:\dev directory
dotnet run AuthenticationTestClient.cs

# Shows:
# ✓ Gets encryption key
# ✓ Encrypts password
# ✓ Registers user
# ✓ Verifies IsActive = false
# ✓ Attempts login (fails)
```

### Option C: JavaScript (Real-World)
```javascript
const service = new RegistrationService();
const result = await service.register(
  'user@example.com',
  'John',
  'Doe',
  'SecurePassword123!',
  'SecurePassword123!'
);
console.log(result); // { success: true, user: {...} }
```

### Option D: REST Client (.http file)
```
Open Users.http and use VS Code REST Client extension
```

## 🚨 Common Issues

### "Failed to decrypt password"
**Cause:** Invalid encrypted format
**Fix:** Ensure password is properly encrypted with public key

### "Email is already registered"
**Cause:** Email already exists
**Fix:** Use different email address

### "Password must be at least 8 characters"
**Cause:** Decrypted password too short
**Fix:** Use 8+ character password

### "Login fails after registration"
**Cause:** IsActive = false (by design)
**Fix:** Admin must activate account manually (see Step 4️⃣ above)

## 📊 Database Schema (Users Table)

```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    IsActive BIT DEFAULT 0,  -- Defaults to inactive!
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    LastLoginAt DATETIME2 NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(255) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    ZipCode NVARCHAR(10) NULL
);
```

## 🔗 Related Endpoints

```
Auth Endpoints:
├── GET    /api/auth/encryption-key      → Get RSA public key
├── POST   /api/auth/register             → Register new user ← NEW
├── POST   /api/auth/login                → Login user
├── GET    /api/auth/me                   → Get current user
├── POST   /api/auth/validate             → Validate token
├── POST   /api/auth/refresh              → Refresh token
└── POST   /api/auth/logout               → Logout user

User Endpoints (CRUD):
├── GET    /api/users                     → Get all users
├── GET    /api/users/{id}                → Get user by ID
├── PUT    /api/users/{id}                → Update user
└── DELETE /api/users/{id}                → Delete user

Role Endpoints:
├── GET    /api/roles                     → Get all roles
├── POST   /api/roles                     → Create role
├── GET    /api/roles/{id}                → Get role
└── DELETE /api/roles/{id}                → Delete role
```

## 🎯 Implementation Status

✅ **Completed:**
- RSA encryption service
- Password encryption/decryption
- User registration with validation
- Encrypted password handling
- IsActive = false default
- Error handling
- Test clients

🔄 **In Progress:**
- Testing and verification

⏳ **Next Steps:**
- Password hashing (bcrypt)
- Email verification
- Admin activation endpoint
- Rate limiting
- Audit logging

## 📞 Support Resources

- **Swagger UI:** http://localhost:5096/swagger
- **API Docs:** AUTHENTICATION_IMPLEMENTATION.md
- **Registration Docs:** REGISTRATION_IMPLEMENTATION.md
- **Test Client:** AuthenticationTestClient.cs
- **JS Client:** wwwroot/js/registration-service.js

---

**Last Updated:** December 4, 2025
**Status:** ✅ Ready for Testing
**Next:** Implement password hashing for production
