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
import { formatTime12Hour } from '../../utils/timeFormatter';
import './AssignGame.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function AssignGame({ open, onClose, gameId, onClaimSuccess }) {
  const { showLoading, hideLoading } = useLoading();
  const [gameDetails, setGameDetails] = useState(null);
  const [selectedPositions, setSelectedPositions] = useState([]);
  const [userClaims, setUserClaims] = useState([]);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (open && gameId) {
      fetchGameDetails();
      fetchUserClaimsForGame();
    } else {
      // Reset state when drawer closes
      setGameDetails(null);
      setSelectedPositions([]);
      setUserClaims([]);
      setError(null);
      setSuccess(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, gameId]);

  const fetchUserClaimsForGame = async () => {
    try {
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const token = localStorage.getItem('accessToken');
      
      if (!user.OfficialId || !token) {
        return;
      }

      const response = await fetch(`${API_BASE_URL}/claims/game/${gameId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const claims = await response.json();
        // Filter to only this user's pending claims
        const myClaims = claims.filter(
          claim => Number(claim.officialId) === Number(user.OfficialId) && claim.claimStatus != 'Withdrawn'
        );
        console.log('Fetched user claims for game:', myClaims);
        setUserClaims(myClaims);
      }
    } catch (err) {
      console.error('Error fetching user claims:', err);
    }
  };

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

  const handlePositionToggle = (position) => {
    setSelectedPositions(prev => {
      const exists = prev.find(p => p.positionId === position.positionId);
      if (exists) {
        return prev.filter(p => p.positionId !== position.positionId);
      } else {
        return [...prev, position];
      }
    });
  };

  const handleUnclaim = async (position) => {
    setError(null);
    console.log('Attempting to unclaim position:', position);
    const claim = userClaims.find(c => Number(c.positionId) === Number(position.positionId));
    console.log('Found claim to delete:', claim);
    if (!claim || !claim.gameClaimId) {
      setError('Could not find claim to delete');
      return;
    }

    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const token = localStorage.getItem('accessToken');
    
    if (!token) {
      setError('Authentication token not found. Please log in again.');
      return;
    }

    if (!user.OfficialId) {
      setError('User ID not found.');
      return;
    }

    try {
      showLoading();
      const response = await fetch(`${API_BASE_URL}/claims/${claim.gameClaimId}?deletedBy=${user.OfficialId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok || response.status === 204) {
        // Remove from selectedPositions if it's there
        setSelectedPositions(prev => prev.filter(p => p.positionId !== position.positionId));
        // Refresh claims to update UI
        await fetchUserClaimsForGame();
        if (onClaimSuccess) {
          onClaimSuccess();
        }
      } else {
        setError('Failed to unclaim position');
      }
    } catch (err) {
      console.error('Error unclaiming position:', err);
      setError('An error occurred while unclaiming the position');
    } finally {
      hideLoading();
    }
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
      const token = localStorage.getItem('accessToken');
      
      if (!token) {
        setError('Authentication token not found. Please log in again.');
        setSubmitting(false);
        return;
      }

      if (!user.OfficialId) {
        setError('Only officials can claim games.');
        setSubmitting(false);
        return;
      }

      // Submit claim for each selected position
      const claimPromises = selectedPositions.map(position => 
        fetch(`${API_BASE_URL}/claims`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify({
            gameId: gameId,
            officialId: user.OfficialId,
            positionId: position.positionId
          })
        })
      );

      const results = await Promise.all(claimPromises);
      const allSuccessful = results.every(r => r.ok);

      if (allSuccessful) {
        // Close drawer and refresh grid immediately
        onClose();
        if (onClaimSuccess) {
          onClaimSuccess();
        }
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

  const openPositions = gameDetails?.openPositions || [];
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
                    <strong>Time:</strong> {formatTime12Hour(gameDetails.gameTime)}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Sport:</strong> {gameDetails.sportName}
                  </Typography>
                  <Typography variant="body2">
                    <strong>League:</strong> {gameDetails.leagueName}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Level:</strong> {gameDetails.levelName}
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
                    {openPositions.map((position) => {
                      const hasClaim = userClaims.some(claim => Number(claim.positionId) === Number(position.positionId));
                      return (
                        <Box key={position.positionId} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <FormControlLabel
                            control={
                              <Checkbox
                                checked={hasClaim || selectedPositions.some(p => p.positionId === position.positionId)}
                                onChange={() => handlePositionToggle(position)}
                                disabled={hasClaim}
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
                                  {hasClaim && (
                                    <Typography component="span" sx={{ ml: 1, color: '#FF5E00', fontWeight: 'bold', fontSize: '0.875rem' }}>
                                      (Claimed)
                                    </Typography>
                                  )}
                                </Typography>
                                {position.isRequired && (
                                  <Typography variant="caption" color="error">
                                    Required Position
                                  </Typography>
                                )}
                              </Box>
                            }
                          />
                          {hasClaim && (
                            <Button
                              variant="outlined"
                              size="small"
                              color="error"
                              onClick={() => handleUnclaim(position)}
                              sx={{ ml: 'auto' }}
                            >
                              Unclaim
                            </Button>
                          )}
                        </Box>
                      );
                    })}
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

AssignGame.propTypes = {
  open: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  gameId: PropTypes.number,
  onClaimSuccess: PropTypes.func
};

export default AssignGame;
