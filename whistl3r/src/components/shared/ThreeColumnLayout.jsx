import { useState, useEffect } from 'react';
import { Box } from '@mui/material';
import PropTypes from 'prop-types';
import whistlersLogo from '../../assets/images/WHISTLERS_LOGO.png';
import './App.css';

// Import carousel images using Vite's import.meta.glob
const imageModules = import.meta.glob('../../assets/images/carousel/vert/*.{jpg,jpeg,png,gif,svg}', { eager: true });
const carouselImages = Object.values(imageModules).map(module => module.default);

function ThreeColumnLayout({ centerContent }) {
  const [leftImages, setLeftImages] = useState([]);
  const [rightImages, setRightImages] = useState([]);

  useEffect(() => {
    var reverseImages = [...carouselImages].reverse();
    setLeftImages(carouselImages);
    setRightImages(reverseImages);
  }, []);

  return (
    <Box
      sx={{
        display: 'flex',
        width: '100%',
        minHeight: '100vh',
        backgroundColor: '#0B0D0C',
      }}
    >
      {/* Left Column - Vertical Carousel (Scrolling Up) */}
      <Box
        sx={{
          flex: '0 0 30%',
          overflow: 'hidden',
          backgroundColor: '#0B0D0C',
          position: 'sticky',
          top: 0,
          height: '100vh',
          alignSelf: 'flex-start',
        }}
      >
        <Box
          className="scroll-up"
          sx={{
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          {/* Triple images for seamless loop */}
          {[...leftImages, ...leftImages, ...leftImages].map((image, index) => (
            <Box
              key={`left-${index}`}
              sx={{
                width: '100%',
                minHeight: '250px',
                backgroundImage: `url(${image})`,
                backgroundSize: 'cover',
                backgroundPosition: 'center',
                boxShadow: '0 4px 8px rgba(0, 0, 0, 0.2)',
                flexShrink: 0,
              }}
            />
          ))}
        </Box>
      </Box>

      {/* Center Column - Content Area */}
      <Box
        sx={{
          flex: '0 0 40%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          paddingLeft: 2,
          paddingRight: 2,
          paddingTop: 4,
          paddingBottom: 4,
          backgroundColor: '#0B0D0C',
        }}
      >
        {centerContent || (
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              gap: 3,
            }}
          >
            <img
              src={whistlersLogo}
              alt="Whistlers Logo"
              style={{ maxWidth: '80%', height: 'auto', objectFit: 'contain' }}
            />
            <Box
              className="whistler-text-heading"
              sx={{
                color: '#0B0D0C',
                textAlign: 'center',
              }}
            >
              Welcome to Whistlers
            </Box>
          </Box>
        )}
      </Box>

      {/* Right Column - Vertical Carousel (Scrolling Down) */}
      <Box
        sx={{
          flex: '0 0 30%',
          overflow: 'hidden',
          backgroundColor: '#0B0D0C',
          position: 'sticky',
          top: 0,
          height: '100vh',
          alignSelf: 'flex-start',
        }}
      >
        <Box
          className="scroll-down"
          sx={{
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          {/* Triple images for seamless loop */}
          {[...rightImages, ...rightImages, ...rightImages].map((image, index) => (
            <Box
              key={`right-${index}`}
              sx={{
                width: '100%',
                minHeight: '250px',
                backgroundImage: `url(${image})`,
                backgroundSize: 'cover',
                backgroundPosition: 'center',
                boxShadow: '0 4px 8px rgba(0, 0, 0, 0.2)',
                flexShrink: 0,
              }}
            />
          ))}
        </Box>
      </Box>
    </Box>
  );
}

ThreeColumnLayout.propTypes = {
  centerContent: PropTypes.node,
};

export default ThreeColumnLayout;
