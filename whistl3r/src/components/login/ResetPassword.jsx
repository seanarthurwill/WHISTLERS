import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, TextField, Button, Alert } from '@mui/material';
import ThreeColumnLayout from '../shared/ThreeColumnLayout';
import whistlersLogo from '../../assets/images/WHISTLERS_LOGO_DARK.png';
import '../shared/App.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

function ResetPassword() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    resetCode: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    
    // Clear error for this field when user starts typing
    if (errors[name]) {
      setErrors((prev) => ({
        ...prev,
        [name]: '',
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.resetCode.trim()) {
      newErrors.resetCode = 'Reset code is required';
    }

    if (!formData.newPassword) {
      newErrors.newPassword = 'New password is required';
    } else if (formData.newPassword.length < 8) {
      newErrors.newPassword = 'Password must be at least 8 characters';
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password';
    } else if (formData.newPassword !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});

    if (!validateForm()) {
      return;
    }

    setIsLoading(true);

    try {
      const response = await fetch(`${API_BASE_URL}/auth/reset-password`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          resetGuid: formData.resetCode.trim(),
          newPassword: formData.newPassword,
        }),
      });

      const data = await response.json();

      if (response.ok) {
        // Success - redirect to login page
        navigate('/login', { 
          state: { message: 'Password reset successfully. Please login with your new password.' }
        });
      } else {
        // Error from server
        setErrors({ 
          general: data.message || 'Failed to reset password. Please check your reset code.' 
        });
      }
    } catch (error) {
      console.error('Password reset error:', error);
      setErrors({ 
        general: 'An unexpected error occurred. Please try again.' 
      });
    } finally {
      setIsLoading(false);
    }
  };

  const centerContent = (
    <Box
      sx={{
        width: '100%',
        maxWidth: '500px',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: 3,
        padding: 4,
      }}
    >
      {/* Logo */}
      <img
        src={whistlersLogo}
        alt="Whistlers Logo"
        style={{ 
          maxWidth: '375px', 
          width: '100%',
          height: 'auto', 
          marginBottom: '20px' 
        }}
      />

      {/* Heading */}
      <Box
        className="whistler-text-heading"
        sx={{
          color: '#FFFFFF',
          textAlign: 'center',
          fontSize: '2rem',
          marginBottom: 2,
        }}
      >
        Reset Password
      </Box>

      {/* Error Alert */}
      {errors.general && (
        <Alert severity="error" sx={{ width: '100%' }}>
          {errors.general}
        </Alert>
      )}

      {/* Form */}
      <Box
        component="form"
        onSubmit={handleSubmit}
        sx={{
          width: '100%',
          display: 'flex',
          flexDirection: 'column',
          gap: 3,
        }}
      >
        {/* Reset Code Field */}
        <TextField
          fullWidth
          label="Reset Code"
          name="resetCode"
          value={formData.resetCode}
          onChange={handleChange}
          error={!!errors.resetCode}
          helperText={errors.resetCode}
          disabled={isLoading}
          placeholder="Enter the code from your email"
          sx={{
            '& .MuiInputBase-root': {
              backgroundColor: '#FFFFFF',
            },
            '& .MuiInputLabel-root': {
              color: '#666',
            },
          }}
        />

        {/* New Password Field */}
        <TextField
          fullWidth
          label="New Password"
          name="newPassword"
          type={showPassword ? 'text' : 'password'}
          value={formData.newPassword}
          onChange={handleChange}
          error={!!errors.newPassword}
          helperText={errors.newPassword}
          disabled={isLoading}
          sx={{
            '& .MuiInputBase-root': {
              backgroundColor: '#FFFFFF',
            },
            '& .MuiInputLabel-root': {
              color: '#666',
            },
          }}
          InputProps={{
            endAdornment: (
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  fontSize: '1.2rem',
                }}
              >
                {showPassword ? '👁️' : '👁️‍🗨️'}
              </button>
            ),
          }}
        />

        {/* Confirm Password Field */}
        <TextField
          fullWidth
          label="Confirm Password"
          name="confirmPassword"
          type={showConfirmPassword ? 'text' : 'password'}
          value={formData.confirmPassword}
          onChange={handleChange}
          error={!!errors.confirmPassword}
          helperText={errors.confirmPassword}
          disabled={isLoading}
          sx={{
            '& .MuiInputBase-root': {
              backgroundColor: '#FFFFFF',
            },
            '& .MuiInputLabel-root': {
              color: '#666',
            },
          }}
          InputProps={{
            endAdornment: (
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  fontSize: '1.2rem',
                }}
              >
                {showConfirmPassword ? '👁️' : '👁️‍🗨️'}
              </button>
            ),
          }}
        />

        {/* Submit Button */}
        <Button
          type="submit"
          variant="contained"
          fullWidth
          disabled={isLoading}
          sx={{
            backgroundColor: '#0087C7',
            color: '#FFFFFF',
            padding: '12px',
            fontSize: '1.1rem',
            fontWeight: 'bold',
            '&:hover': {
              backgroundColor: '#006BA6',
            },
            '&:disabled': {
              backgroundColor: '#666',
            },
          }}
        >
          {isLoading ? 'Resetting Password...' : 'Reset Password'}
        </Button>

        {/* Back to Login Link */}
        <Box sx={{ textAlign: 'center', marginTop: 2 }}>
          <a
            href="/login"
            style={{
              color: '#0087C7',
              textDecoration: 'none',
              fontSize: '0.9rem',
            }}
          >
            Back to Login
          </a>
        </Box>
      </Box>
    </Box>
  );

  return <ThreeColumnLayout centerContent={centerContent} />;
}

export default ResetPassword;
