# Users Microservice Authentication API Documentation

## Overview
The Users microservice provides JWT-based authentication with OAuth provider support. Users can authenticate using username/password or external OAuth providers (Google, Microsoft, GitHub).

## Base URL
```
https://localhost:5001/api
```

## Authentication
All authenticated endpoints require a Bearer token in the Authorization header:
```
Authorization: Bearer {access_token}
```

## Endpoints

### 1. Login with Email & Password
Authenticate using email and password credentials.

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "securepassword123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedrefreshtoken",
  "expiresIn": 3600,
  "user": {
    "userId": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Official", "Admin"]
  }
}
```

**Response (401 Unauthorized):**
```json
{
  "message": "Invalid email or password"
}
```

**cURL Example:**
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "securepassword123"
  }'
```

---

### 2. Refresh Access Token
Get a new access token using a valid refresh token.

**Endpoint:** `POST /api/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "base64encodedrefreshtoken"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "newbase64encodedrefreshtoken",
  "expiresIn": 3600
}
```

**Response (401 Unauthorized):**
```json
{
  "message": "Refresh token is required"
}
```

---

### 3. Get Current User Profile
Retrieve the authenticated user's profile and claims.

**Endpoint:** `GET /api/auth/me`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "lastLoginAt": "2024-01-20T14:45:00Z",
  "claims": {
    "roles": [
      "Official",
      "Admin"
    ],
    "certifications": [
      "Level 1 Referee",
      "Soccer Official"
    ],
    "email": "user@example.com",
    "givenName": "John",
    "surname": "Doe"
  }
}
```

**Response (401 Unauthorized):**
```json
{
  "message": "Invalid token"
}
```

**Response (404 Not Found):**
```json
{
  "message": "User not found"
}
```

---

### 4. Logout (Revoke Token)
Invalidate the current access token.

**Endpoint:** `POST /api/auth/logout`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

---

### 5. Validate Token
Check if a token is valid and not expired.

**Endpoint:** `POST /api/auth/validate`

**Request Body:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (200 OK):**
```json
{
  "valid": true
}
```

**Response (200 OK - Invalid Token):**
```json
{
  "valid": false
}
```

---

## JWT Token Structure

### Access Token Claims
```json
{
  "sub": "1",
  "email": "user@example.com",
  "given_name": "John",
  "family_name": "Doe",
  "IsActive": "true",
  "role": ["Official", "Admin"],
  "Certification": ["Level 1 Referee", "Soccer Official"],
  "iat": 1705675200,
  "exp": 1705678800,
  "iss": "Whistl3r",
  "aud": "Whistl3rAPI"
}
```

### Token Expiration
- Access tokens expire after **60 minutes** (configurable in `appsettings.json`)
- Refresh tokens do not expire but can be revoked

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Whistl3r_Users;Integrated Security=true;Encrypt=false;"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "Whistl3r",
    "Audience": "Whistl3rAPI",
    "Authority": "https://localhost:5001",
    "ExpirationMinutes": 60
  },
  "OAuth": {
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret",
      "AuthenticationScheme": "Google"
    },
    "Microsoft": {
      "ClientId": "your-microsoft-client-id",
      "ClientSecret": "your-microsoft-client-secret",
      "AuthenticationScheme": "Microsoft"
    },
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret",
      "AuthenticationScheme": "GitHub"
    }
  }
}
```

---

## OAuth Integration

### Supported Providers
1. **Google** - OAuth 2.0
2. **Microsoft/Azure AD** - OpenID Connect
3. **GitHub** - OAuth 2.0

### OAuth Flow (to be implemented in frontend)
1. Redirect user to OAuth provider login
2. OAuth provider redirects back to callback URL with authorization code
3. Exchange code for token with OAuth provider
4. POST token to `/api/auth/oauth-callback`
5. Receive JWT access and refresh tokens

### Setting Up OAuth Providers

#### Google OAuth
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project
3. Enable Google+ API
4. Create OAuth 2.0 credentials (Web application)
5. Set authorized redirect URIs: `https://yourdomain.com/auth/callback/google`
6. Copy Client ID and Client Secret to `appsettings.json`

#### Microsoft Azure AD
1. Go to [Azure Portal](https://portal.azure.com/)
2. Register a new application in Azure AD
3. Configure API permissions
4. Create a client secret
5. Set redirect URIs: `https://yourdomain.com/auth/callback/microsoft`
6. Copy Application ID and secret to `appsettings.json`

#### GitHub OAuth
1. Go to GitHub Settings > Developer settings > OAuth Apps
2. Create a new OAuth App
3. Set Authorization callback URL: `https://yourdomain.com/auth/callback/github`
4. Copy Client ID and Client Secret to `appsettings.json`

---

## Error Handling

### Common HTTP Status Codes
| Status | Description |
|--------|-------------|
| 200 | Success |
| 400 | Bad Request (invalid input) |
| 401 | Unauthorized (invalid credentials or expired token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 500 | Internal Server Error |

### Error Response Format
```json
{
  "message": "Error description"
}
```

---

## Security Best Practices

1. **HTTPS Only**: Always use HTTPS in production
2. **Secret Management**: Store JWT secret in secure configuration (Azure Key Vault, etc.)
3. **Token Storage**: Store access tokens in memory or secure HTTP-only cookies
4. **Token Rotation**: Regularly rotate refresh tokens
5. **CORS**: Configure CORS properly to prevent unauthorized cross-origin requests
6. **Rate Limiting**: Implement rate limiting on login endpoints
7. **Password Security**: Use bcrypt or similar for password hashing (implement in production)

---

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- SQL Server or LocalDB
- Visual Studio Code or Visual Studio

### Environment Variables (Development)
```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=Whistl3r_Users;Integrated Security=true;Encrypt=false;"
$env:Jwt__SecretKey="your-development-secret-key-here"
```

### Running the Service
```bash
cd services/Users
dotnet restore
dotnet build
dotnet run
```

### API Testing
- Swagger UI: `https://localhost:5001/swagger`
- Use Postman or cURL for API testing

---

## Class Diagram

```
┌─────────────────────────────────────┐
│         AuthController              │
│─────────────────────────────────────│
│ + Login(email, password)            │
│ + RefreshToken(refreshToken)        │
│ + GetCurrentUser()                  │
│ + Logout()                          │
│ + ValidateToken(token)              │
└─────────────────────────────────────┘
           │
           │ uses
           ▼
┌─────────────────────────────────────┐
│   IAuthenticationService            │
│─────────────────────────────────────│
│ + LoginAsync(email, password)       │
│ + RefreshTokenAsync(refreshToken)   │
│ + ValidateTokenAsync(token)         │
│ + GetClaimsFromTokenAsync(token)    │
│ + RevokeTokenAsync(token)           │
└─────────────────────────────────────┘
           │
           │ uses
           ▼
┌─────────────────────────────────────┐
│      IJwtService                    │
│─────────────────────────────────────│
│ + GenerateAccessToken(user, roles)  │
│ + GenerateRefreshToken()            │
│ + ValidateToken(token)              │
│ + GetPrincipalFromExpiredToken()    │
└─────────────────────────────────────┘
```

---

## Related Services

- **User Management**: User models, roles, certifications
- **Role Service**: Manage user roles and permissions
- **Certification Service**: Manage official certifications
- **Logging**: Built-in ASP.NET Core logging

---

## Support

For issues or questions about the authentication API, please refer to:
- [Microsoft ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT.io - JWT Debugger](https://jwt.io/)
- Project documentation in `README.md`
