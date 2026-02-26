import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import {
  Drawer,
  Box,
  Typography,
  Button,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  IconButton,
  Alert,
  Divider
} from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useLoading } from '../../contexts/LoadingContext';
import './CreateGame.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function CreateGame({ open, onClose, onCreateSuccess }) {
  const { showLoading, hideLoading } = useLoading();
  
  // Form state
  const [formData, setFormData] = useState({
    organizationId: '',
    leagueId: '',
    tournamentId: '',
    venueId: '',
    ageLevelId: '',
    homeTeam: '',
    awayTeam: '',
    gameDate: '',
    gameTime: '',
    gameLengthMinutes: '',
    overrideGameLengthMinutes: '',
    payScaleRuleId: '',
    gameStatusId: 1 // Default to 'Open' or similar
  });

  // Dropdown options
  const [organizations, setOrganizations] = useState([]);
  // const [leagues, setLeagues] = useState([]);
  // const [venues, setVenues] = useState([]);
  // const [ageLevels, setAgeLevels] = useState([]);
  const [gameStatuses, setGameStatuses] = useState([]);
  
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (open) {
      fetchDropdownData();
    } else {
      // Reset form when drawer closes
      resetForm();
    }
  }, [open]);

  const resetForm = () => {
    setFormData({
      organizationId: '',
      leagueId: '',
      tournamentId: '',
      venueId: '',
      ageLevelId: '',
      homeTeam: '',
      awayTeam: '',
      gameDate: '',
      gameTime: '',
      gameLengthMinutes: '',
      overrideGameLengthMinutes: '',
      payScaleRuleId: '',
      gameStatusId: 1
    });
    setError(null);
  };

  const fetchDropdownData = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const headers = token ? { 'Authorization': `Bearer ${token}` } : {};

      // Fetch organizations
      const orgsResponse = await fetch(`${API_BASE_URL}/organizations`, { headers });
      if (orgsResponse.ok) {
        const orgsData = await orgsResponse.json();
        setOrganizations(orgsData);
      }

      // Fetch game statuses
      const statusResponse = await fetch(`${API_BASE_URL}/games/statuses`, { headers });
      if (statusResponse.ok) {
        const statusData = await statusResponse.json();
        setGameStatuses(statusData);
      }

      // TODO: Fetch leagues, venues, age levels when those endpoints are available
      // For now, we'll allow manual entry or use placeholder data
    } catch (err) {
      console.error('Error fetching dropdown data:', err);
    }
  };

  const handleInputChange = (field, value) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = async () => {
    setError(null);

    // Validation
    if (!formData.organizationId || !formData.venueId || !formData.ageLevelId ||
        !formData.homeTeam || !formData.awayTeam || !formData.gameDate || !formData.gameTime) {
      setError('Please fill in all required fields');
      return;
    }

    setSubmitting(true);
    showLoading();

    try {
      const token = localStorage.getItem('accessToken');
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      
      if (!token || !user.UserId) {
        setError('You must be logged in to create a game');
        return;
      }

      // Prepare the payload
      const payload = {
        organizationId: parseInt(formData.organizationId),
        leagueId: formData.leagueId ? parseInt(formData.leagueId) : null,
        tournamentId: formData.tournamentId ? parseInt(formData.tournamentId) : null,
        venueId: parseInt(formData.venueId),
        ageLevelId: parseInt(formData.ageLevelId),
        homeTeam: formData.homeTeam,
        awayTeam: formData.awayTeam,
        gameDate: formData.gameDate,
        gameTime: formData.gameTime,
        gameLengthMinutes: formData.gameLengthMinutes ? parseInt(formData.gameLengthMinutes) : null,
        overrideGameLengthMinutes: formData.overrideGameLengthMinutes ? parseInt(formData.overrideGameLengthMinutes) : null,
        payScaleRuleId: formData.payScaleRuleId ? parseInt(formData.payScaleRuleId) : null,
        gameStatusId: parseInt(formData.gameStatusId),
        createdBy: user.UserId
      };

      const response = await fetch(`${API_BASE_URL}/games`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        const createdGame = await response.json();
        console.log('Game created successfully:', createdGame);
        onCreateSuccess?.();
        onClose();
      } else {
        const errorData = await response.json().catch(() => ({}));
        setError(errorData.message || 'Failed to create game');
      }
    } catch (err) {
      console.error('Error creating game:', err);
      setError('An error occurred while creating the game');
    } finally {
      setSubmitting(false);
      hideLoading();
    }
  };

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      PaperProps={{
        sx: {
          width: { xs: '100%', sm: 500 },
          display: 'flex',
          flexDirection: 'column',
          height: '100%'
        }
      }}
    >
      <Box sx={{ 
        display: 'flex', 
        flexDirection: 'column', 
        height: '100%',
        maxHeight: '100vh'
      }}>
        {/* Header */}
        <Box sx={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'center', 
          p: 3, 
          pb: 2,
          flexShrink: 0
        }}>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Create New Game
          </Typography>
          <IconButton onClick={onClose} disabled={submitting}>
            <CloseIcon />
          </IconButton>
        </Box>

        <Divider sx={{ flexShrink: 0 }} />

        {/* Error Alert */}
        {error && (
          <Alert severity="error" sx={{ m: 3, mb: 0 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {/* Form Fields */}
        <Box sx={{ 
          flex: 1,
          overflowY: 'auto', 
          px: 3,
          py: 3,
          minHeight: 0
        }}>
          {/* Organization */}
          <FormControl fullWidth sx={{ mb: 2 }} required>
            <InputLabel>Organization</InputLabel>
            <Select
              value={formData.organizationId}
              onChange={(e) => handleInputChange('organizationId', e.target.value)}
              label="Organization"
            >
              {organizations.map((org) => (
                <MenuItem key={org.organizationId} value={org.organizationId}>
                  {org.organizationName}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* League ID */}
          <TextField
            fullWidth
            label="League ID"
            type="number"
            value={formData.leagueId}
            onChange={(e) => handleInputChange('leagueId', e.target.value)}
            sx={{ mb: 2 }}
          />

          {/* Tournament ID */}
          <TextField
            fullWidth
            label="Tournament ID"
            type="number"
            value={formData.tournamentId}
            onChange={(e) => handleInputChange('tournamentId', e.target.value)}
            sx={{ mb: 2 }}
          />

          {/* Venue ID */}
          <TextField
            fullWidth
            label="Venue ID"
            type="number"
            value={formData.venueId}
            onChange={(e) => handleInputChange('venueId', e.target.value)}
            sx={{ mb: 2 }}
            required
          />

          {/* Age Level ID */}
          <TextField
            fullWidth
            label="Age Level ID"
            type="number"
            value={formData.ageLevelId}
            onChange={(e) => handleInputChange('ageLevelId', e.target.value)}
            sx={{ mb: 2 }}
            required
          />

          {/* Home Team */}
          <TextField
            fullWidth
            label="Home Team"
            value={formData.homeTeam}
            onChange={(e) => handleInputChange('homeTeam', e.target.value)}
            sx={{ mb: 2 }}
            required
          />

          {/* Away Team */}
          <TextField
            fullWidth
            label="Away Team"
            value={formData.awayTeam}
            onChange={(e) => handleInputChange('awayTeam', e.target.value)}
            sx={{ mb: 2 }}
            required
          />

          {/* Game Date */}
          <TextField
            fullWidth
            label="Game Date"
            type="date"
            value={formData.gameDate}
            onChange={(e) => handleInputChange('gameDate', e.target.value)}
            InputLabelProps={{ shrink: true }}
            sx={{ mb: 2 }}
            required
          />

          {/* Game Time */}
          <TextField
            fullWidth
            label="Game Time"
            type="time"
            value={formData.gameTime}
            onChange={(e) => handleInputChange('gameTime', e.target.value)}
            InputLabelProps={{ shrink: true }}
            sx={{ mb: 2 }}
            required
          />

          {/* Game Length Minutes */}
          <TextField
            fullWidth
            label="Game Length (Minutes)"
            type="number"
            value={formData.gameLengthMinutes}
            onChange={(e) => handleInputChange('gameLengthMinutes', e.target.value)}
            sx={{ mb: 2 }}
          />

          {/* Override Game Length Minutes */}
          <TextField
            fullWidth
            label="Override Game Length (Minutes)"
            type="number"
            value={formData.overrideGameLengthMinutes}
            onChange={(e) => handleInputChange('overrideGameLengthMinutes', e.target.value)}
            sx={{ mb: 2 }}
          />

          {/* Pay Scale Rule ID */}
          <TextField
            fullWidth
            label="Pay Scale Rule ID"
            type="number"
            value={formData.payScaleRuleId}
            onChange={(e) => handleInputChange('payScaleRuleId', e.target.value)}
            sx={{ mb: 2 }}
          />

          {/* Game Status */}
          <FormControl fullWidth sx={{ mb: 3 }}>
            <InputLabel>Game Status</InputLabel>
            <Select
              value={formData.gameStatusId}
              onChange={(e) => handleInputChange('gameStatusId', e.target.value)}
              label="Game Status"
            >
              {gameStatuses.map((status) => (
                <MenuItem key={status.gameStatusId} value={status.gameStatusId}>
                  {status.statusName}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        {/* Action Buttons */}
        <Box sx={{ 
          display: 'flex', 
          gap: 2, 
          px: 3,
          pb: 3,
          pt: 2,
          borderTop: '1px solid #e0e0e0',
          flexShrink: 0  // Prevent buttons from shrinking
        }}>
          <Button
            variant="outlined"
            fullWidth
            onClick={onClose}
            disabled={submitting}
            sx={{ borderRadius: '16px' }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            fullWidth
            onClick={handleSubmit}
            disabled={submitting}
            sx={{
              backgroundColor: '#3F9033',
              '&:hover': { backgroundColor: '#5aa84a' },
              borderRadius: '16px'
            }}
          >
            {submitting ? 'Creating...' : 'Save Game'}
          </Button>
        </Box>
      </Box>
    </Drawer>
  );
}

CreateGame.propTypes = {
  open: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  onCreateSuccess: PropTypes.func
};

export default CreateGame;
