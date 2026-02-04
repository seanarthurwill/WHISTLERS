import { useState, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { Box, Typography, Paper, Button, Select, MenuItem, FormControl, Tooltip, IconButton } from '@mui/material';
import { Info as InfoIcon } from '@mui/icons-material';
import { useLoading } from '../../contexts/LoadingContext';
import { formatTime12Hour } from '../../utils/timeFormatter';
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
  const [userClaims, setUserClaims] = useState([]);
  const [allClaims, setAllClaims] = useState([]);
  const [gameView, setGameView] = useState('upcoming');
  const [hasCreatePermission, setHasCreatePermission] = useState(false);
  const [positionLookup, setPositionLookup] = useState({});

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

  const fetchUserClaims = async () => {
    try {
      console.log('fetchUserClaims: Starting...');
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const token = localStorage.getItem('accessToken');
      
      console.log('fetchUserClaims: Full user object', user);
      
      // Check if user has assignorId to show Create Game button
      setHasCreatePermission(!!user.AssignorId);
      
      // The JWT uses 'OfficialId' claim - if not present, user is not an official
      const officialId = user.OfficialId;
      console.log('fetchUserClaims: Extracted officialId:', officialId);
      
      if (!officialId || !token) {
        console.log('fetchUserClaims: No officialId or token, returning empty array');
        return [];
      }

      // Fetch all games to get their IDs, then get claims for each
      const gamesResponse = await fetch(`${API_BASE_URL}/games/details-report`);
      if (!gamesResponse.ok) {
        console.log('fetchUserClaims: Games fetch failed');
        return [];
      }
      
      const gamesData = await gamesResponse.json();
      const uniqueGameIds = [...new Set(gamesData.map(g => g.gameId))];
      
      console.log('fetchUserClaims: Fetching claims for games:', uniqueGameIds);
      
      // Fetch claims for all games
      const claimsPromises = uniqueGameIds.map(gameId =>
        fetch(`${API_BASE_URL}/claims/game/${gameId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }).then(async res => {
         //console.log(`Claims API response for game ${gameId}:`, res.status, res.ok);
          if (res.ok) {
            const data = await res.json();
            console.log(`Claims data for game ${gameId}:`, data);
            return data;
          }
          console.log(`No claims for game ${gameId}`);
          return [];
        }).catch(err => {
          console.error(`Error fetching claims for game ${gameId}:`, err);
          return [];
        })
      );
      
      const allClaims = await Promise.all(claimsPromises);
      console.log('All claims arrays:', allClaims);
      const flatClaims = allClaims.flat();
      
      console.log('All flat claims:', flatClaims);
      
      // Store all claims
      setAllClaims(flatClaims);
      
      // Filter to only this user's pending claims
      const userPendingClaims = flatClaims.filter(
        claim => Number(claim.officialId) === Number(officialId) && claim.claimStatus === 'Pending'
      );
      
      console.log('User pending claims:', userPendingClaims);
      console.log('OfficialId for comparison:', officialId);
      
      setUserClaims(userPendingClaims);
      return userPendingClaims;
    } catch (err) {
      console.error('Error fetching user claims:', err);
      return [];
    }
  };

  const fetchGames = async () => {
    // Check if user is an official before loading games
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    if (!user.OfficialId) {
      setError('Only officials can view and claim games');
      return;
    }

    showLoading();
    try {
      const [gamesResponse] = await Promise.all([
        fetch(`${API_BASE_URL}/games/details-report`),
        fetchUserClaims()
      ]);
      
      if (gamesResponse.ok) {
        const data = await gamesResponse.json();
        setAllGames(data);
        setGames(data);
        
        // Build position name to ID lookup from game details
        await buildPositionLookup(data);
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

  const buildPositionLookup = async (gamesData) => {
    try {
      const token = localStorage.getItem('accessToken');
      const lookup = {};
      
      // Get unique game IDs
      const uniqueGameIds = [...new Set(gamesData.map(g => g.gameId))];
      
      // Fetch game details for each game to get position data
      const gameDetailsPromises = uniqueGameIds.slice(0, 10).map(gameId =>
        fetch(`${API_BASE_URL}/games/${gameId}`, {
          headers: token ? { 'Authorization': `Bearer ${token}` } : {}
        }).then(async res => {
          if (res.ok) {
            const gameDetail = await res.json();
            if (gameDetail.openPositions && Array.isArray(gameDetail.openPositions)) {
              gameDetail.openPositions.forEach(pos => {
                // Create lookup key as "positionName" for easy matching
                if (pos.positionName && pos.positionId) {
                  const key = pos.positionName.toLowerCase().trim();
                  lookup[key] = pos.positionId;
                }
              });
            }
          }
          return null;
        }).catch(() => null)
      );
      
      await Promise.all(gameDetailsPromises);
      setPositionLookup(lookup);
      console.log('Position lookup built:', lookup);
    } catch (err) {
      console.error('Error building position lookup:', err);
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

  const handleGameViewChange = (event) => {
    setGameView(event.target.value);
  };

  const columns = [
    {
      field: 'actions',
      headerName: 'Actions',
      width: 120,
      sortable: false,
      filterable: false,
      renderCell: (params) => {
        const hasPendingClaim = userClaims.some(claim => Number(claim.gameId) === Number(params.row.gameId));
        return (
          <Button
            variant="contained"
            size="small"
            onClick={() => handleClaim(params.row)}
            sx={{
              backgroundColor: hasPendingClaim ? '#FF5E00' : '#3F9033',
              '&:hover': { backgroundColor: hasPendingClaim ? '#FFCE00' : '#5aa84a' },
              '&:disabled': { backgroundColor: '#515353' },
              minWidth: '90px'
            }}
          >
            {hasPendingClaim ? 'Edit Claim' : 'Claim'}
          </Button>
        );
      }
    },
    { 
      field: 'claimStatus', 
      headerName: 'Claim Status', 
      width: 120,
      valueGetter: (value, row) => {
        //console.log('Checking claim for game:', row.gameId, 'User claims:', userClaims.map(c => c.gameId));
        const hasPendingClaim = userClaims.some(claim => {
          //console.log(`Comparing claim.gameId (${claim.gameId}, ${typeof claim.gameId}) === row.gameId (${row.gameId}, ${typeof row.gameId})`);
          return Number(claim.gameId) === Number(row.gameId);
        });
        return hasPendingClaim ? 'Pending' : 'Edit Claim';
      },
      renderCell: (params) => (
        <Box
          sx={{
            color: params.value === 'Pending' ? '#FF5E00' : '#3F9033',
            fontWeight: 'bold'
          }}
        >
          {params.value}
        </Box>
      )
    },
    { 
      field: 'claimCount', 
      headerName: '# of Claims', 
      width: 100,
      valueGetter: (value, row) => {
        const gameClaimsCount = allClaims.filter(claim => 
          Number(claim.gameId) === Number(row.gameId) && 
          claim.claimStatus !== 'Withdrawn'
        ).length;
        return gameClaimsCount;
      }
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
        return formatTime12Hour(value);
      }
    },
    { 
      field: 'openPositions', 
      headerName: 'Open Positions',
      width: 260,
      renderHeader: (params) => (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <span style={{ fontWeight: 'bold' }}>{params.colDef.headerName}</span>
          <Tooltip 
            title="Red indicates 3 or more claims, Blue indicates 1 or 2 claims, and Green indicates no claims"
            placement="top"
            arrow
          >
            <IconButton size="small" sx={{ padding: 0 }}>
              <InfoIcon sx={{ fontSize: 16, color: '#666' }} />
            </IconButton>
          </Tooltip>
        </Box>
      ),
      renderCell: (params) => {
        const positions = params.value ? params.value.split(',').map(p => p.trim()) : [];
        
        return (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
            {positions.map((position, index) => {
              // Look up the positionId from the position name using our lookup table
              const positionKey = position.toLowerCase().trim();
              const positionId = positionLookup[positionKey];
              
              // Count claims for this specific position and game
              const positionClaimCount = allClaims.filter(claim => {
                const matchesGame = Number(claim.gameId) === Number(params.row.gameId);
                const notWithdrawn = claim.claimStatus !== 'Withdrawn';
                // Match by positionId if we have it
                const matchesPosition = positionId ? Number(claim.positionId) === Number(positionId) : false;
                
                return matchesGame && notWithdrawn && matchesPosition;
              }).length;
              
              // Determine color based on claim count
              let color = '#3F9033'; // Green for 0 claims
              if (positionClaimCount >= 3) {
                color = '#A80000'; // Red for 3+ claims
              } else if (positionClaimCount >= 1) {
                color = '#0066CC'; // Blue for 1-2 claims
              }
              
              return (
                <span key={index} style={{ color, fontWeight: 'bold' }}>
                  {position}{index < positions.length - 1 ? ',' : ''}
                </span>
              );
            })}
          </Box>
        );
      }
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
        <FormControl size="small" className="game-view-filter" variant="outlined">
          <Select
            value={gameView}
            onChange={handleGameViewChange}
            displayEmpty
            sx={{
              backgroundColor: '#3F9033',
              color: 'white',
              minWidth: 180,
              height: 36,
              borderRadius: '16px',
              fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
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
            <MenuItem value="upcoming">Upcoming Games</MenuItem>
            <MenuItem value="my-games">My Games</MenuItem>
          </Select>
        </FormControl>
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
              borderRadius: '16px',
              fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
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

      {hasCreatePermission && (
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', marginTop: '20px', marginRight: '24px' }}>
          <Button
            variant="contained"
            sx={{
              backgroundColor: '#3F9033',
              '&:hover': { backgroundColor: '#5aa84a' },
              borderRadius: '16px',
              padding: '10px 30px',
              fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
              textTransform: 'none',
              fontSize: '16px',
              fontWeight: '500'
            }}
          >
            Create Game
          </Button>
        </Box>
      )}

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
