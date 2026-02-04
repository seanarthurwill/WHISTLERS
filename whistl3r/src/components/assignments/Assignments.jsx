import { useState, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { Box, Typography, Paper, Button, Select, MenuItem, FormControl, IconButton, Collapse } from '@mui/material';
import { KeyboardArrowDown, KeyboardArrowRight } from '@mui/icons-material';
import { useLoading } from '../../contexts/LoadingContext';
import { formatTime12Hour } from '../../utils/timeFormatter';
import ClaimGame from './AssignGame';
import './Assignments.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function Assignments() {
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
  const [gameDetails, setGameDetails] = useState({});
  const [expandedRows, setExpandedRows] = useState(new Set());

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
      //console.log('fetchUserClaims: Starting...');
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const token = localStorage.getItem('accessToken');
      
      //console.log('fetchUserClaims: Full user object', user);
      
      // The JWT uses 'OfficialId' claim - if not present, user is not an official
      const officialId = user.OfficialId;
      //console.log('fetchUserClaims: Extracted officialId:', officialId);
      
      if (!officialId || !token) {
        //console.log('fetchUserClaims: No officialId or token, returning empty array');
        return [];
      }

      // Fetch all games to get their IDs, then get claims for each
      const gamesResponse = await fetch(`${API_BASE_URL}/games/details-report`);
      if (!gamesResponse.ok) {
        //console.log('fetchUserClaims: Games fetch failed');
        return [];
      }
      
      const gamesData = await gamesResponse.json();
      const uniqueGameIds = [...new Set(gamesData.map(g => g.gameId))];
      
      //console.log('fetchUserClaims: Fetching claims for games:', uniqueGameIds);
      
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
            //console.log(`Claims data for game ${gameId}:`, data);
            return data;
          }
          //console.log(`No claims for game ${gameId}`);
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
        
        // Fetch details for each game to get positions
        fetchAllGameDetails(data);
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

  const fetchAllGameDetails = async (gamesData) => {
    const token = localStorage.getItem('accessToken');
    if (!token) return;

    const uniqueGameIds = [...new Set(gamesData.map(g => g.gameId))];
    console.log('Fetching details for games:', uniqueGameIds);
    
    const detailsPromises = uniqueGameIds.map(gameId =>
      fetch(`${API_BASE_URL}/games/${gameId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
      }).then(async res => {
        console.log(`Game ${gameId} details response status:`, res.status);
        if (res.ok) {
          const detail = await res.json();
          console.log(`Game ${gameId} details:`, detail);
          console.log(`Game ${gameId} openPositions:`, detail.openPositions);
          return { gameId, detail };
        }
        return { gameId, detail: null };
      }).catch(err => {
        console.error(`Error fetching game ${gameId}:`, err);
        return { gameId, detail: null };
      })
    );

    const results = await Promise.all(detailsPromises);
    const detailsLookup = {};
    results.forEach(({ gameId, detail }) => {
      if (detail) {
        detailsLookup[gameId] = detail;
      }
    });
    
    console.log('Complete game details lookup:', detailsLookup);
    setGameDetails(detailsLookup);
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

  const toggleRowExpansion = (gameId) => {
    setExpandedRows(prev => {
      const newSet = new Set(prev);
      if (newSet.has(gameId)) {
        newSet.delete(gameId);
      } else {
        newSet.add(gameId);
      }
      return newSet;
    });
  };

  const columns = [
    {
      field: 'expand',
      headerName: '',
      width: 50,
      sortable: false,
      filterable: false,
      renderCell: (params) => {
        const details = gameDetails[params.row.gameId];
        const hasPositions = details?.openPositions && details.openPositions.length > 0;
        
        if (!hasPositions) return null;
        
        const isExpanded = expandedRows.has(params.row.gameId);
        return (
          <IconButton
            size="small"
            onClick={() => toggleRowExpansion(params.row.gameId)}
            sx={{ padding: '4px' }}
          >
            {isExpanded ? <KeyboardArrowDown /> : <KeyboardArrowRight />}
          </IconButton>
        );
      }
    },
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
            {hasPendingClaim ? 'Edit' : 'Claim'}
          </Button>
        );
      }
    },
    { 
      field: 'claimStatus', 
      headerName: 'Assignment Status', 
      width: 120,
      valueGetter: (value, row) => {
        //console.log('Checking claim for game:', row.gameId, 'User claims:', userClaims.map(c => c.gameId));
        const hasPendingClaim = userClaims.some(claim => {
          //console.log(`Comparing claim.gameId (${claim.gameId}, ${typeof claim.gameId}) === row.gameId (${row.gameId}, ${typeof row.gameId})`);
          return Number(claim.gameId) === Number(row.gameId);
        });
        return hasPendingClaim ? 'Pending' : 'Edit';
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

  const renderDetailPanel = (row) => {
    const details = gameDetails[row.gameId];
    
    if (!details || !details.openPositions || details.openPositions.length === 0) {
      return null;
    }

    const positions = details.openPositions || [];

    return (
      <Box sx={{ py: 1, px: 6, backgroundColor: 'transparent' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #e0e0e0' }}>
              <th style={{ textAlign: 'left', padding: '8px', fontWeight: 600, color: '#666' }}>Position</th>
              <th style={{ textAlign: 'left', padding: '8px', fontWeight: 600, color: '#666' }}># of Claims</th>
              <th style={{ textAlign: 'left', padding: '8px', fontWeight: 600, color: '#666' }}>Assigned Official</th>
            </tr>
          </thead>
          <tbody>
            {positions.map((position) => {
              const positionClaims = allClaims.filter(
                claim => Number(claim.gameId) === Number(row.gameId) && 
                         Number(claim.positionId) === Number(position.positionId) &&
                         claim.claimStatus !== 'Withdrawn'
              );
              const claimCount = positionClaims.length;
              
              const assignedOfficial = 'Open';

              return (
                <tr key={position.positionId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                  <td style={{ padding: '8px', color: '#333' }}>{position.positionName}</td>
                  <td style={{ padding: '8px', color: '#333' }}>{claimCount}</td>
                  <td style={{ padding: '8px', color: assignedOfficial === 'Open' ? '#3F9033' : '#000', fontWeight: 500 }}>
                    {assignedOfficial}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </Box>
    );
  };

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
            <MenuItem value="completed">Completed Games</MenuItem>
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

      <Paper className="open-games-grid-container" sx={{ position: 'relative' }}>
        <DataGrid
          rows={games}
          columns={columns}
          getRowId={(row) => row.gameId}
          initialState={{
            pagination: {
              paginationModel: { pageSize: 25 },
            },
          }}
          pageSizeOptions={[10, 25, 50, 100]}
          disableRowSelectionOnClick
          className="open-games-datagrid"
          getRowClassName={(params) => 
            expandedRows.has(params.id) ? 'expanded-row' : ''
          }
          sx={{
            '& .MuiDataGrid-row.expanded-row': {
              '& .MuiDataGrid-cell': {
                borderBottom: 'none',
              }
            }
          }}
        />
        {Array.from(expandedRows).map((gameId) => {
          const game = games.find(g => g.gameId === gameId);
          if (!game) return null;
          
          // Calculate which page the game is on
          const gameIndex = games.findIndex(g => g.gameId === gameId);
          const pageSize = 25;
          const currentPage = Math.floor(gameIndex / pageSize);
          
          // Only show detail panels for current page
          // Position based on row index within the page
          const rowIndexInPage = gameIndex % pageSize;
          const headerHeight = 56;
          const rowHeight = 52;
          const topPosition = headerHeight + ((rowIndexInPage + 1) * rowHeight);
          
          return (
            <Box
              key={`detail-${gameId}`}
              sx={{
                position: 'absolute',
                top: `${topPosition}px`,
                left: 0,
                right: 0,
                zIndex: 1,
                backgroundColor: '#fafafa',
                borderBottom: '1px solid #e0e0e0',
              }}
            >
              {renderDetailPanel(game)}
            </Box>
          );
        })}
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

export default Assignments;
