import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import {
  Drawer,
  Box,
  Typography,
  Button,
  Select,
  MenuItem,
  FormControl,
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
  const [allClaims, setAllClaims] = useState([]);
  const [officials, setOfficials] = useState({});
  const [positionAssignments, setPositionAssignments] = useState({});
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (open && gameId) {
      fetchGameDetails();
      fetchAllClaimsForGame();
    } else {
      // Reset state when drawer closes
      setGameDetails(null);
      setAllClaims([]);
      setOfficials({});
      setPositionAssignments({});
      setError(null);
      setSuccess(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, gameId]);

  const fetchAllClaimsForGame = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      
      if (!token) {
        return;
      }

      const response = await fetch(`${API_BASE_URL}/claims/game/${gameId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const claims = await response.json();
        // Filter out withdrawn claims
        const activeClaims = claims.filter(claim => claim.claimStatus !== 'Withdrawn');
        console.log('Fetched all active claims for game:', activeClaims);
        setAllClaims(activeClaims);
        
        // Fetch official details for all claiming officials
        const uniqueOfficialIds = [...new Set(activeClaims.map(claim => claim.officialId))];
        await fetchOfficials(uniqueOfficialIds);
      }
    } catch (err) {
      console.error('Error fetching claims:', err);
    }
  };

  const fetchOfficials = async (officialIds) => {
    try {
      const token = localStorage.getItem('accessToken');
      
      if (!token || officialIds.length === 0) {
        return;
      }

      // Fetch each official's user details using the new endpoint
      const officialPromises = officialIds.map(officialId =>
        fetch(`${API_BASE_URL}/users/official/${officialId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }).then(async res => {
          if (res.ok) {
            const user = await res.json();
            return { officialId, user };
          }
          console.error(`Failed to fetch user for official ${officialId}`);
          return { officialId, user: null };
        }).catch(err => {
          console.error(`Error fetching user for official ${officialId}:`, err);
          return { officialId, user: null };
        })
      );

      const results = await Promise.all(officialPromises);
      
      // Create a lookup object by officialId
      const officialsLookup = {};
      results.forEach(({ officialId, user }) => {
        if (user) {
          officialsLookup[officialId] = user;
        }
      });
      
      console.log('Officials lookup created:', officialsLookup);
      setOfficials(officialsLookup);
    } catch (err) {
      console.error('Error fetching officials:', err);
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

  const handlePositionAssignment = (positionId, officialId) => {
    setPositionAssignments(prev => ({
      ...prev,
      [positionId]: officialId
    }));
  };

  const handleSubmitAssignments = async () => {
    setSubmitting(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('accessToken');
      
      if (!token) {
        setError('Authentication token not found. Please log in again.');
        setSubmitting(false);
        return;
      }

      // TODO: Implement assignment submission to backend
      console.log('Position assignments:', positionAssignments);
      
      // Close drawer and refresh grid
      onClose();
      if (onClaimSuccess) {
        onClaimSuccess();
      }
    } catch (err) {
      console.error('Error submitting assignments:', err);
      setError('An error occurred while submitting assignments');
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
            Assign Game
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
                  Assign Officials to Positions
                </Typography>
                
                {!hasOpenPositions ? (
                  <Alert severity="info">
                    No positions available for this game.
                  </Alert>
                ) : (
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    {openPositions.map((position) => {
                      // Get all claims for this position
                      const positionClaims = allClaims.filter(
                        claim => Number(claim.positionId) === Number(position.positionId)
                      );
                      
                      return (
                        <Box key={position.positionId} sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                          <Typography variant="body1" sx={{ fontWeight: 'bold' }}>
                            {position.positionName}
                            {position.isRequired && (
                              <Typography component="span" sx={{ ml: 1, color: 'error.main', fontSize: '0.875rem' }}>
                                (Required)
                              </Typography>
                            )}
                          </Typography>
                          <FormControl fullWidth size="small">
                            <Select
                              value={positionAssignments[position.positionId] || ''}
                              onChange={(e) => handlePositionAssignment(position.positionId, e.target.value)}
                              displayEmpty
                              sx={{
                                borderRadius: '16px',
                                '& .MuiOutlinedInput-notchedOutline': {
                                  borderRadius: '16px'
                                }
                              }}
                            >
                              <MenuItem value="">
                                <em>Select Official</em>
                              </MenuItem>
                              {positionClaims.map((claim) => {
                                const official = officials[claim.officialId];
                                console.log('Rendering claim:', claim.officialId, 'Official data:', official);
                                const officialName = official 
                                  ? `${official.firstName || official.FirstName || ''} ${official.lastName || official.LastName || ''}`.trim()
                                  : `Official ID: ${claim.officialId}`;
                                return (
                                  <MenuItem key={claim.gameClaimId} value={claim.officialId}>
                                    {officialName || `Official ID: ${claim.officialId}`}
                                  </MenuItem>
                                );
                              })}
                            </Select>
                          </FormControl>
                          {positionClaims.length === 0 && (
                            <Typography variant="caption" color="text.secondary">
                              No claims for this position
                            </Typography>
                          )}
                        </Box>
                      );
                    })}
                  </Box>
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
            onClick={handleSubmitAssignments}
            disabled={submitting}
            sx={{
              backgroundColor: '#667eea',
              '&:hover': { backgroundColor: '#5568d3' }
            }}
          >
            {submitting ? <CircularProgress size={24} /> : 'Assign Officials'}
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
