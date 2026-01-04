# User Registration Component - Quick Start

## Overview
The registration component allows users to create new accounts with encrypted password transmission using RSA-2048 encryption.

## Features
✅ Client-side password encryption (RSA-2048)
✅ Real-time form validation
✅ Email, first name, last name, phone, and password fields
✅ Phone field is optional
✅ Password strength validation (minimum 8 characters)
✅ Automatic error handling and display
✅ Beautiful, responsive UI

## How to Use

### 1. Start the Backend Service
```powershell
cd c:\dev\services\Users
dotnet run
```
The service will start on http://localhost:5096

### 2. Start the React Frontend
```powershell
cd c:\dev\whistl3r
npm run dev
```
The frontend will start (typically on http://localhost:5173)

### 3. Register a New User
1. Open your browser to the frontend URL
2. Fill in the registration form:
   - **First Name**: Minimum 2 characters (required)
   - **Last Name**: Minimum 2 characters (required)
   - **Email**: Valid email format (required)
   - **Phone**: Any valid phone format (optional)
   - **Password**: Minimum 8 characters (required)
   - **Confirm Password**: Must match password (required)
3. Click "Create Account"

## Registration Flow

```
User fills form
    ↓
Frontend validates input
    ↓
Frontend fetches RSA public key from API
    ↓
Frontend encrypts password with public key
    ↓
Frontend sends encrypted data to API
    ↓
Backend decrypts password with private key
    ↓
Backend validates all fields
    ↓
Backend creates user with IsActive = false
    ↓
User account created (pending activation)
```

## Security Features

1. **Password Encryption**
   - Passwords are NEVER sent in plain text
   - RSA-2048 encryption with OAEP-SHA256 padding
   - Server-side decryption with private key

2. **Account Activation**
   - New accounts are created with `IsActive = false`
   - Admin must activate accounts before users can log in
   - Future: Email verification will auto-activate

3. **Validation**
   - Client-side validation for immediate feedback
   - Server-side validation for security
   - Email uniqueness check

## API Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/auth/encryption-key` | GET | Retrieve RSA public key |
| `/api/auth/register` | POST | Register new user |

## Files Created

```
whistl3r/
├── src/
│   ├── components/
│   │   ├── Register.jsx          # Registration component
│   │   └── Register.css          # Component styles
│   └── services/
│       └── authService.js        # Authentication service with encryption
└── package.json                  # Added jsencrypt dependency
```

## Configuration

The API base URL is configured in `src/services/authService.js`:
```javascript
const API_BASE_URL = 'http://localhost:5096/api';
```

Change this if your backend runs on a different port or URL.

## Validation Rules

| Field | Rules | Example |
|-------|-------|---------|
| First Name | 2+ characters, required | ✅ John, ❌ J |
| Last Name | 2+ characters, required | ✅ Doe, ❌ D |
| Email | Valid format, unique, required | ✅ user@example.com |
| Phone | Valid format, optional | ✅ +1 (555) 123-4567 |
| Password | 8+ characters, required | ✅ SecurePass123! |
| Confirm Password | Must match password | ✅ SecurePass123! |

## Error Handling

The component handles various error scenarios:

- **Network errors**: "An unexpected error occurred"
- **Validation errors**: Displayed under each field
- **Duplicate email**: "Email is already registered"
- **Server errors**: Appropriate error messages from API

## Success Flow

When registration succeeds:
1. Success message is displayed
2. Form is cleared automatically
3. User is informed their account is pending activation

## Testing

### Test a Valid Registration
```
First Name: John
Last Name: Doe
Email: john.doe@example.com
Phone: +1 (555) 123-4567
Password: SecurePassword123!
Confirm Password: SecurePassword123!
```

### Test Validation Errors
```
First Name: J              ❌ Too short
Last Name: D               ❌ Too short
Email: notanemail          ❌ Invalid format
Password: short            ❌ Less than 8 characters
Confirm Password: different ❌ Doesn't match
```

## Next Steps

1. **Implement Email Verification**
   - Send verification email on registration
   - Auto-activate account on email verification

2. **Add Admin Activation Endpoint**
   - Allow admins to activate/deactivate accounts
   - Admin dashboard for user management

3. **Add Login Component**
   - Allow activated users to log in
   - Store JWT token for authenticated requests

4. **Implement Password Hashing**
   - Add bcrypt to backend (CRITICAL for production)
   - Hash passwords before storing in database

## Troubleshooting

### "Failed to fetch encryption key"
- Ensure backend is running on http://localhost:5096
- Check CORS configuration in backend
- Verify firewall is not blocking the connection

### "Registration failed"
- Check browser console for detailed errors
- Verify all required fields are filled
- Check backend logs for server-side errors

### CORS Errors
The backend should have CORS configured in `Program.cs`. If you see CORS errors:
1. Verify CORS is enabled in the backend
2. Check that the frontend URL is allowed
3. Restart the backend service

## Dependencies

### Frontend
- `react` - UI framework
- `jsencrypt` - RSA encryption library

### Backend
- `.NET 8.0` - Runtime
- `System.IdentityModel.Tokens.Jwt` - JWT handling
- `Microsoft.EntityFrameworkCore` - Database access

## Support

For issues or questions:
1. Check the REGISTRATION_IMPLEMENTATION.md for detailed docs
2. Review backend logs: `services\Users\bin\Debug\net8.0\logs\`
3. Check browser console for frontend errors
4. Verify API endpoints in Swagger: http://localhost:5096/swagger
