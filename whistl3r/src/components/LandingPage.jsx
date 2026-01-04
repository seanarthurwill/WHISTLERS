
import { useNavigate } from 'react-router-dom';
import { Button, Box, Link } from '@mui/material';
import ImageCarousel from './shared/ImageCarousel';
import ThreeColumnLayout from './shared/ThreeColumnLayout';
import whistlersDarkLogo from '../assets/images/WHISTLERS_LOGO_DARK.png';
import './shared/App.css';

function LandingPage() {
  const navigate = useNavigate();

  const handleRegister = () => {
    navigate('/register');
  };

  const handleLogin = () => {
    navigate('/login');
  };

  const handleForgotPassword = () => {
    navigate('/forgot-password');
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh', width: '100vw', overflow: 'hidden', margin: 0, padding: 0 }}>
      {/* Sticky Navigation Bar */}
      <Box
        sx={{
          position: 'sticky',
          top: 0,
          height: '125px',
          width: '100%',
          backgroundColor: '#0B0D0C',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: '0 20px',
          margin: 0,
          zIndex: 1000,
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.3)',
        }}
      >
        {/* Logo */}
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <img
            src={whistlersDarkLogo}
            alt="Whistlers Logo"
            style={{ height: '100px', objectFit: 'contain' }}
          />
        </Box>

        {/* Auth Buttons */}
        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 1, marginRight: '40px' }}>
          <Box sx={{ display: 'flex', gap: 2, position: 'relative' }}>
            <Button
              variant="contained"
              onClick={handleRegister}
              sx={{
                backgroundColor: '#0095DA',
                color: '#FFFFFF',
                fontFamily: 'LilGrotesk, Arial, sans-serif',
                fontSize: '12px',
                fontWeight: 600,
                padding: '7.5px 22.5px',
                borderRadius: '6px',
                textTransform: 'none',
                '&:hover': {
                  backgroundColor: '#007AB8',
                },
              }}
            >
              Register
            </Button>
            <Button
              variant="contained"
              onClick={handleLogin}
              id="login-button"
              sx={{
                backgroundColor: '#FE0000',
                color: '#FFFFFF',
                fontFamily: 'LilGrotesk, Arial, sans-serif',
                fontSize: '12px',
                fontWeight: 600,
                padding: '7.5px 22.5px',
                borderRadius: '6px',
                textTransform: 'none',
                '&:hover': {
                  backgroundColor: '#CC0000',
                },
              }}
            >
              Login
            </Button>
          </Box>
          <Box sx={{ width: '100%', display: 'flex', justifyContent: 'flex-end' }}>
            <Link
              component="button"
              onClick={handleForgotPassword}
              sx={{
                fontFamily: 'LilGrotesk, Arial, sans-serif',
                fontSize: '14px',
                color: '#FFFFFF',
                textDecoration: 'none',
                cursor: 'pointer',
                textAlign: 'left',
                '&:hover': {
                  textDecoration: 'underline',
                },
              }}
            >
              Forgot Password?
            </Link>
          </Box>
        </Box>
      </Box>

      {/* Scrollable Content */}
      <Box
        sx={{
          flex: 1,
          width: '100%',
          overflowY: 'auto',
          overflowX: 'hidden',
          margin: 0,
          padding: 0,
        }}
      >
        {/* Carousel Section - Full Screen */}
        <Box
          sx={{
            height: 'calc(100vh - 125px)',
            width: '100%',
            position: 'relative',
          }}
        >
          <ImageCarousel />
        </Box>

        {/* Additional Content Section */}
        <Box
          sx={{
            minHeight: '100vh',
            width: '100%',
          }}
        >
          <ThreeColumnLayout />
        </Box>
      </Box>
    </Box>
  );
}

export default LandingPage;
