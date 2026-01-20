import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import {
  Drawer,
  Box,
  Typography,
  Button,
  Checkbox,
  FormControlLabel,
  FormGroup,
  Divider,
  IconButton,
  Alert,
  CircularProgress
} from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useLoading } from '../../contexts/LoadingContext';
import './ClaimGame.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function ClaimGame({ open, onClose, gameId, onClaimSuccess }) {
  const { showLoading, hideLoading } = useLoading();
  const [gameDetails, setGameDetails] = useState(null);
  const [selectedPositions, setSelectedPositions] = useState([]);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (open && gameId) {
      fetchGameDetails();
    } else {
      // Reset state when drawer closes
      setGameDetails(null);
      setSelectedPositions([]);
      setError(null);
      setSuccess(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, gameId]);

  const fetchGameDetails = async () => {
    showLoading();
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/games/${gameId}`);
      if (response.ok) {
        const data = await response.json();
        setGameDetails(data);
      } else {
        setError('Failed to load game details');
      }
    } catch (err) {
      console.error('Error fetching game details:', err);
      setError('An error occurred while loading game details');
    } finally {
      hideLoading();
    }
  };

  const handlePositionToggle = (positionName) => {
    setSelectedPositions(prev => {
      if (prev.includes(positionName)) {
        return prev.filter(p => p !== positionName);
      } else {
        return [...prev, positionName];
      }
    });
  };

  const handleSubmitClaim = async () => {
    if (selectedPositions.length === 0) {
      setError('Please select at least one position to claim');
      return;
    }

    setSubmitting(true);
    setError(null);
    
    try {
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const token = localStorage.getItem('token');
      
      if (!token) {
        setError('Authentication token not found. Please log in again.');
        setSubmitting(false);
        return;
      }

      // Submit claim for each selected position
      const claimPromises = selectedPositions.map(positionName => 
        fetch(`${API_BASE_URL}/game-assignments`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify({
            gameId: gameId,
            positionName: positionName,
            userId: user.userId
          })
        })
      );

      const results = await Promise.all(claimPromises);
      const allSuccessful = results.every(r => r.ok);

      if (allSuccessful) {
        setSuccess(true);
        setSelectedPositions([]);
        setTimeout(() => {
          if (onClaimSuccess) {
            onClaimSuccess();
          }
          onClose();
        }, 1500);
      } else {
        setError('Failed to claim one or more positions');
      }
    } catch (err) {
      console.error('Error submitting claim:', err);
      setError('An error occurred while submitting your claim');
    } finally {
      setSubmitting(false);
    }
  };

  const openPositions = gameDetails?.positions?.filter(p => p.positionStatus === 'Open') || [];
  const hasOpenPositions = openPositions.length > 0;

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      PaperProps={{
        sx: {
          width: { xs: '100%', sm: 450 },
          backgroundColor: '#f5f5f5'
        }
      }}
    >
      <Box className="claim-game-drawer">
        {/* Header */}
        <Box className="claim-game-header">
          <Typography variant="h6" className="claim-game-title">
            Claim Game Position
          </Typography>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>

        <Divider />

        {/* Content */}
        <Box className="claim-game-content">
          {!gameDetails ? (
            <Box className="claim-game-loading">
              <CircularProgress />
            </Box>
          ) : (
            <>
              {/* Game Details */}
              <Box className="claim-game-details">
                <Typography variant="subtitle2" color="text.secondary">
                  Game Details
                </Typography>
                <Box className="claim-game-info">
                  <Typography variant="body2">
                    <strong>Date:</strong> {new Date(gameDetails.gameDate).toLocaleDateString()}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Time:</strong> {gameDetails.gameTime}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Sport:</strong> {gameDetails.sportName}
                  </Typography>
                  <Typography variant="body2">
                    <strong>League:</strong> {gameDetails.leagueName}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Level:</strong> {gameDetails.ageLevelName}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Teams:</strong> {gameDetails.homeTeam} vs {gameDetails.awayTeam}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Venue:</strong> {gameDetails.venueName}
                  </Typography>
                </Box>
              </Box>

              <Divider sx={{ my: 2 }} />

              {/* Position Selection */}
              <Box className="claim-game-positions">
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  Select Position(s) to Claim
                </Typography>
                
                {!hasOpenPositions ? (
                  <Alert severity="info">
                    No open positions available for this game.
                  </Alert>
                ) : (
                  <FormGroup>
                    {openPositions.map((position) => (
                      <FormControlLabel
                        key={position.positionName}
                        control={
                          <Checkbox
                            checked={selectedPositions.includes(position.positionName)}
                            onChange={() => handlePositionToggle(position.positionName)}
                            sx={{
                              color: '#667eea',
                              '&.Mui-checked': {
                                color: '#667eea'
                              }
                            }}
                          />
                        }
                        label={
                          <Box>
                            <Typography variant="body1">
                              {position.positionName}
                            </Typography>
                            {position.positionRequired && (
                              <Typography variant="caption" color="error">
                                Required Position
                              </Typography>
                            )}
                          </Box>
                        }
                      />
                    ))}
                  </FormGroup>
                )}
              </Box>

              {/* Error/Success Messages */}
              {error && (
                <Alert severity="error" sx={{ mt: 2 }}>
                  {error}
                </Alert>
              )}
              
              {success && (
                <Alert severity="success" sx={{ mt: 2 }}>
                  Position(s) claimed successfully!
                </Alert>
              )}
            </>
          )}
        </Box>

        {/* Footer */}
        <Box className="claim-game-footer">
          <Button
            variant="outlined"
            onClick={onClose}
            disabled={submitting}
            sx={{ mr: 1 }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleSubmitClaim}
            disabled={!hasOpenPositions || selectedPositions.length === 0 || submitting}
            sx={{
              backgroundColor: '#667eea',
              '&:hover': { backgroundColor: '#5568d3' }
            }}
          >
            {submitting ? <CircularProgress size={24} /> : `Claim ${selectedPositions.length > 0 ? `(${selectedPositions.length})` : ''}`}
          </Button>
        </Box>
      </Box>
    </Drawer>
  );
}

ClaimGame.propTypes = {
  open: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  gameId: PropTypes.number,
  onClaimSuccess: PropTypes.func
};

export default ClaimGame;
