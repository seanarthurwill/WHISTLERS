import { useState, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { Box, Typography, Paper, Button, Select, MenuItem, FormControl } from '@mui/material';
import { useLoading } from '../../contexts/LoadingContext';
import ClaimGame from './ClaimGame';
import './OpenGames.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function OpenGames() {
  const { showLoading, hideLoading } = useLoading();
  const [games, setGames] = useState([]);
  const [allGames, setAllGames] = useState([]);
  const [sports, setSports] = useState([]);
  const [selectedSport, setSelectedSport] = useState('all');
  const [error, setError] = useState(null);
  const [claimDrawerOpen, setClaimDrawerOpen] = useState(false);
  const [selectedGameId, setSelectedGameId] = useState(null);

  const handleClaim = (row) => {
    setSelectedGameId(row.gameId);
    setClaimDrawerOpen(true);
  };

  const handleClaimSuccess = () => {
    // Refresh games list after successful claim
    fetchGames();
  };

  const handleCloseDrawer = () => {
    setClaimDrawerOpen(false);
    setSelectedGameId(null);
  };

  const fetchGames = async () => {
    showLoading();
    try {
      const response = await fetch(`${API_BASE_URL}/games/details-report`);
      if (response.ok) {
        const data = await response.json();
        setAllGames(data);
        setGames(data);
      } else {
        setError('Failed to load games');
      }
    } catch (err) {
      console.error('Error fetching games:', err);
      setError('An error occurred while loading games');
    } finally {
      hideLoading();
    }
  };

  useEffect(() => {
    const fetchSports = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/sports`);
        if (response.ok) {
          const data = await response.json();
          setSports(data);
        }
      } catch (err) {
        console.error('Error fetching sports:', err);
      }
    };

    fetchSports();
    fetchGames();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (selectedSport === 'all') {
      setGames(allGames);
    } else {
      // Find the selected sport name
      const selectedSportObj = sports.find(s => s.sportId === selectedSport);
      const selectedSportName = selectedSportObj?.sportName;
      
      console.log('Selected sport name:', selectedSportName);
      
      const filtered = allGames.filter(game => {
        return game.sportName === selectedSportName;
      });
      console.log('Filtered games:', filtered);
      setGames(filtered);
    }
  }, [selectedSport, allGames, sports]);

  const handleSportChange = (event) => {
    setSelectedSport(event.target.value);
  };

  const columns = [
    {
      field: 'actions',
      headerName: 'Actions',
      width: 120,
      sortable: false,
      filterable: false,
      renderCell: (params) => (
        <Button
          variant="contained"
          size="small"
          onClick={() => handleClaim(params.row)}
          sx={{
            backgroundColor: '#FF5E00',
            '&:hover': { backgroundColor: '#FFCE00' },
            '&:disabled': { backgroundColor: '#515353' }
          }}
        >
          Claim
        </Button>
      )
    },
    { 
      field: 'gameId', 
      headerName: 'ID', 
      width: 70 
    },
    { 
      field: 'gameDate', 
      headerName: 'Date', 
      width: 110,
      valueFormatter: (value) => {
        if (!value) return '';
        return new Date(value).toLocaleDateString();
      }
    },
    { 
      field: 'gameTime', 
      headerName: 'Time', 
      width: 100,
      valueFormatter: (value) => {
        if (!value) return '';
        return value;
      }
    },
    { 
      field: 'openPositions', 
      headerName: 'OpenPosition', 
      width: 130 
    },
    { 
      field: 'ageLevelName', 
      headerName: 'Age Level', 
      width: 120 
    },
    { 
      field: 'homeClub', 
      headerName: 'Home', 
      width: 150 
    },
    { 
      field: 'awayClub', 
      headerName: 'Away', 
      width: 150 
    },
    { 
      field: 'venueName', 
      headerName: 'Venue', 
      width: 180 
    },
    { 
      field: 'leagueName', 
      headerName: 'League', 
      width: 150 
    },
    
    { 
      field: 'gameNotes', 
      headerName: 'Notes', 
      width: 250
    }
  ];

  return (
    <Box className="open-games-container">
      <div className="open-games-header">
        <div className="open-games-title-pill">
          <Typography variant="body2" className="open-games-title">
            Available Games
          </Typography>
        </div>
        <FormControl size="small" className="sport-filter" variant="outlined">
          <Select
            value={selectedSport}
            onChange={handleSportChange}
            displayEmpty
            sx={{
              backgroundColor: '#3F9033',
              color: 'white',
              minWidth: 150,
              height: 36,
              borderRadius: '4px',
              fontFamily: "'Lil Grotesk', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
              '& .MuiOutlinedInput-notchedOutline': {
                borderColor: 'rgba(255, 255, 255, 0.5)'
              },
              '&:hover .MuiOutlinedInput-notchedOutline': {
                borderColor: 'rgba(255, 255, 255, 0.8)'
              },
              '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                borderColor: 'white',
                borderWidth: '2px'
              },
              '& .MuiSelect-select': {
                paddingTop: '8px',
                paddingBottom: '8px'
              },
              '& .MuiSvgIcon-root': {
                color: 'white'
              }
            }}
          >
            <MenuItem value="all">All Sports</MenuItem>
            {sports.map((sport) => (
              <MenuItem key={sport.sportId} value={sport.sportId}>
                {sport.sportName}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </div>

      {error && (
        <Paper className="open-games-error">
          <Typography>{error}</Typography>
        </Paper>
      )}

      <Paper className="open-games-grid-container">
        <DataGrid
          rows={games}
          columns={columns}
          getRowId={(row) => `${row.gameId}-${row.positionName || 'no-position'}-${Math.random()}`}
          initialState={{
            pagination: {
              paginationModel: { pageSize: 25 },
            },
          }}
          pageSizeOptions={[10, 25, 50, 100]}
          disableRowSelectionOnClick
          disableColumnVirtualization
          className="open-games-datagrid"
        />
      </Paper>

      <ClaimGame
        open={claimDrawerOpen}
        onClose={handleCloseDrawer}
        gameId={selectedGameId}
        onClaimSuccess={handleClaimSuccess}
      />
    </Box>
  );
}

export default OpenGames;
