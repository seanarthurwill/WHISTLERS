import JSEncrypt from 'jsencrypt';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';
console.log('🔍 API_BASE_URL:', API_BASE_URL);
console.log('🔍 VITE_API_URL env var:', import.meta.env.VITE_API_URL);

class AuthService {
  constructor() {
    this.publicKey = null;
    this.encrypt = new JSEncrypt();
  }

  /**
   * Fetch the RSA public key from the server
   */
  async fetchEncryptionKey() {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/encryption-key`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error('Failed to fetch encryption key');
      }

      const data = await response.json();
      this.publicKey = data.publicKey;
      this.encrypt.setPublicKey(this.publicKey);
      
      return data;
    } catch (error) {
      console.error('Error fetching encryption key:', error);
      throw error;
    }
  }

  /**
   * Encrypt password using RSA public key
   */
  encryptPassword(password) {
    if (!this.publicKey) {
      throw new Error('Encryption key not loaded. Call fetchEncryptionKey() first.');
    }

    const encrypted = this.encrypt.encrypt(password);
    if (!encrypted) {
      throw new Error('Failed to encrypt password');
    }

    return encrypted;
  }

  /**
   * Validate registration data on client side
   */
  validateRegistrationData(data) {
    const errors = {};

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!data.email || !emailRegex.test(data.email)) {
      errors.email = 'Valid email address is required';
    }

    // First name validation
    if (!data.firstName || data.firstName.trim().length < 2) {
      errors.firstName = 'First name must be at least 2 characters';
    }

    // Last name validation
    if (!data.lastName || data.lastName.trim().length < 2) {
      errors.lastName = 'Last name must be at least 2 characters';
    }

    // Phone validation (optional but validate format if provided)
    if (data.phone) {
      const phoneRegex = /^\+?[\d\s-()]+$/;
      if (!phoneRegex.test(data.phone)) {
        errors.phone = 'Invalid phone number format';
      }
    }

    // Password validation
    if (!data.password || data.password.length < 8) {
      errors.password = 'Password must be at least 8 characters';
    }

    // Password confirmation
    if (data.password !== data.confirmPassword) {
      errors.confirmPassword = 'Passwords do not match';
    }

    // League validation
    if (!data.leagueId || data.leagueId === '' || parseInt(data.leagueId) <= 0) {
      errors.leagueId = 'Please select a league';
    }

    // Role validation (now supports roleIds array)
    if (!data.roleIds || !Array.isArray(data.roleIds) || data.roleIds.length === 0) {
      errors.roleIds = 'Please select at least one role';
    }

    return {
      isValid: Object.keys(errors).length === 0,
      errors,
    };
  }

  /**
   * Register a new user
   */
  async registerUser(userData) {
    try {
      console.log('Starting user registration process');
      // Ensure we have the encryption key
      if (!this.publicKey) {
        await this.fetchEncryptionKey();
      }

      // Validate data
      const validation = this.validateRegistrationData(userData);
      if (!validation.isValid) {
        return {
          success: false,
          errors: validation.errors,
        };
      }

      // Encrypt password
      const encryptedPassword = this.encryptPassword(userData.password);

      // Prepare registration request
      const registrationData = {
        email: userData.email.trim(),
        firstName: userData.firstName.trim(),
        lastName: userData.lastName.trim(),
        phone: userData.phone?.trim() || null,
        sportId: parseInt(userData.sportId),
        leagueId: parseInt(userData.leagueId),
        roleIds: userData.roleIds.map(id => parseInt(id)),
        roleOrganizations: userData.roleOrganizations,
        encryptedPassword: encryptedPassword,
      };

      // Send registration request
      const response = await fetch(`${API_BASE_URL}/auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(registrationData),
      });

      const data = await response.json();

      if (!response.ok) {
        return {
          success: false,
          errors: data.errors || { general: data.message || 'Registration failed' },
        };
      }

      return {
        success: true,
        data: data,
      };
    } catch (error) {
      console.error('Registration error:', error);
      return {
        success: false,
        errors: { general: error.message || 'An unexpected error occurred' },
      };
    }
  }

  /**
   * Complete registration workflow
   */
  async register(userData) {
    return await this.registerUser(userData);
  }

  /**
   * Login user with email and password
   */
  async login(credentials) {
    try {
      // Ensure we have the encryption key
      if (!this.publicKey) {
        await this.fetchEncryptionKey();
      }

      // Validate credentials
      if (!credentials.email || !credentials.password) {
        return {
          success: false,
          errors: { general: 'Email and password are required' },
        };
      }

      // Encrypt password
      const encryptedPassword = this.encryptPassword(credentials.password);

      // Send login request
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: credentials.email.trim(),
          encryptedPassword: encryptedPassword,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        return {
          success: false,
          errors: { general: data.message || 'Login failed' },
        };
      }

      // Store tokens and user data
      if (data.accessToken) {
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        
        // Decode JWT to extract user info and roles
        const userInfo = this.parseJwtToken(data.accessToken);
        localStorage.setItem('user', JSON.stringify(userInfo));
        localStorage.setItem('roles', JSON.stringify(userInfo.roles || []));
      }

      return {
        success: true,
        user: data.user,
        roles: data.roles,
      };
    } catch (error) {
      console.error('Login error:', error);
      return {
        success: false,
        errors: { general: error.message || 'An unexpected error occurred' },
      };
    }
  }

  /**
   * Parse JWT token to extract payload
   */
  parseJwtToken(token) {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('Error parsing JWT token:', error);
      return {};
    }
  }

  /**
   * Get current user from localStorage
   */
  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  /**
   * Get current user roles from localStorage
   */
  getUserRoles() {
    const rolesStr = localStorage.getItem('roles');
    return rolesStr ? JSON.parse(rolesStr) : [];
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated() {
    const token = localStorage.getItem('accessToken');
    if (!token) return false;

    // Check if token is expired
    const tokenData = this.parseJwtToken(token);
    if (!tokenData.exp) return false;

    const currentTime = Math.floor(Date.now() / 1000);
    return tokenData.exp > currentTime;
  }

  /**
   * Logout user
   */
  logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('roles');
  }

  /**
   * Get access token for API requests
   */
  getAccessToken() {
    return localStorage.getItem('accessToken');
  }
}

export default new AuthService();
