import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, TextField, Button, Alert, Typography } from '@mui/material';
import ThreeColumnLayout from '../shared/ThreeColumnLayout';
import whistlersLogo from '../../assets/images/WHISTLERS_LOGO_DARK.png';
import '../shared/App.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function ForgotPassword() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleChange = (e) => {
    setEmail(e.target.value);
    // Clear error when user starts typing
    if (error) {
      setError('');
    }
  };

  const validateEmail = () => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email.trim()) {
      setError('Email is required');
      return false;
    }
    if (!emailRegex.test(email)) {
      setError('Please enter a valid email address');
      return false;
    }
    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!validateEmail()) {
      return;
    }

    setIsLoading(true);

    try {
      const response = await fetch(`${API_BASE_URL}/auth/request-password-reset`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: email.trim(),
        }),
      });

      let data;
      try {
        data = await response.json();
      } catch (jsonError) {
        console.error('Failed to parse response as JSON:', jsonError);
        setError('Invalid response from server');
        return;
      }

      console.log('Response status:', response.status);
      console.log('Response data:', data);

      if (response.ok) {
        // Success - redirect to reset password page
        navigate('/reset-password', { 
          state: { 
            email: email.trim(),
            message: 'A reset code has been sent to your email address.' 
          }
        });
      } else {
        // Show the actual error message from the server
        setError(data.message || 'Failed to send reset code. Please try again.');
      }
    } catch (error) {
      console.error('Password reset request error:', error);
      setError('An unexpected error occurred. Please try again.');
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
        Forgot Password
      </Box>

      {/* Instructions */}
      <Typography
        className="whistler-text"
        sx={{
          color: '#CCCCCC',
          textAlign: 'center',
          fontSize: '1rem',
          lineHeight: 1.6,
          marginBottom: 2,
        }}
      >
        Enter your email address below and we&apos;ll send you a reset code. 
        You&apos;ll need to enter this code on the next page to reset your password.
      </Typography>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ width: '100%' }}>
          {error}
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
        {/* Email Field */}
        <TextField
          fullWidth
          label="Email Address"
          name="email"
          type="email"
          value={email}
          onChange={handleChange}
          error={!!error}
          disabled={isLoading}
          placeholder="Enter your email address"
          sx={{
            '& .MuiInputBase-root': {
              backgroundColor: '#FFFFFF',
            },
            '& .MuiInputLabel-root': {
              color: '#666',
            },
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
          {isLoading ? 'Sending Code...' : 'Send Code'}
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

export default ForgotPassword;
