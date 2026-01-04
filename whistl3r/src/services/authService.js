import JSEncrypt from 'jsencrypt';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

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

    // Role validation
    if (!data.roleId || data.roleId === '' || parseInt(data.roleId) <= 0) {
      errors.roleId = 'Please select a role';
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
        roleId: parseInt(userData.roleId),
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
}

export default new AuthService();
