import { useState, useEffect, useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  flexRender,
} from '@tanstack/react-table';
import { Box, Typography, Paper, Button, Select, MenuItem, FormControl, Tooltip, IconButton } from '@mui/material';
import { Info as InfoIcon, ArrowUpward, ArrowDownward } from '@mui/icons-material';
import { useLoading } from '../../contexts/LoadingContext';
import { formatTime12Hour } from '../../utils/timeFormatter';
import ClaimGame from './ClaimGame';
import CreateGame from './CreateGame';
import './OpenGames.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
console.log('🔍 OpenGames - VITE_API_URL from env:', import.meta.env.VITE_API_URL);
console.log('🔍 OpenGames - Final API_BASE_URL:', API_BASE_URL);
console.log('🔍 OpenGames - All env vars:', import.meta.env);

function OpenGames() {
  const { showLoading, hideLoading } = useLoading();
  const [games, setGames] = useState([]);
  const [allGames, setAllGames] = useState([]);
  const [sports, setSports] = useState([]);
  const [selectedSport, setSelectedSport] = useState('all');
  const [error, setError] = useState(null);
  const [claimDrawerOpen, setClaimDrawerOpen] = useState(false);
  const [createDrawerOpen, setCreateDrawerOpen] = useState(false);
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

  const handleCreateGame = () => {
    setCreateDrawerOpen(true);
  };

  const handleCreateSuccess = () => {
    // Refresh games list after successful creation
    fetchGames();
  };

  const handleCloseCreateDrawer = () => {
    setCreateDrawerOpen(false);
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

  const columns = useMemo(() => [
    {
      id: 'actions',
      header: 'Actions',
      cell: ({ row }) => {
        const hasPendingClaim = userClaims.some(claim => Number(claim.gameId) === Number(row.original.gameId));
        return (
          <Button
            variant="contained"
            size="small"
            onClick={() => handleClaim(row.original)}
            sx={{
              backgroundColor: hasPendingClaim ? '#FF5E00' : '#3F9033',
              '&:hover': { backgroundColor: hasPendingClaim ? '#FFCE00' : '#5aa84a' },
              minWidth: '90px'
            }}
          >
            {hasPendingClaim ? 'Edit Claim' : 'Claim'}
          </Button>
        );
      },
      size: 120,
    },
    {
      id: 'claimStatus',
      header: 'Claim Status',
      cell: ({ row }) => {
        const hasPendingClaim = userClaims.some(claim => Number(claim.gameId) === Number(row.original.gameId));
        const value = hasPendingClaim ? 'Pending' : 'Edit Claim';
        return (
          <Box sx={{ color: value === 'Pending' ? '#FF5E00' : '#3F9033', fontWeight: 'bold' }}>
            {value}
          </Box>
        );
      },
      size: 120,
    },
    {
      id: 'claimCount',
      header: '# of Claims',
      cell: ({ row }) => {
        const gameClaimsCount = allClaims.filter(claim => 
          Number(claim.gameId) === Number(row.original.gameId) && 
          claim.claimStatus !== 'Withdrawn'
        ).length;
        return gameClaimsCount;
      },
      size: 100,
    },
    {
      accessorKey: 'gameId',
      header: 'ID',
      size: 70,
    },
    {
      accessorKey: 'gameDate',
      header: 'Date',
      cell: ({ getValue }) => {
        const value = getValue();
        if (!value) return '';
        return new Date(value).toLocaleDateString();
      },
      size: 110,
    },
    {
      accessorKey: 'gameTime',
      header: 'Time',
      cell: ({ getValue }) => {
        const value = getValue();
        if (!value) return '';
        return formatTime12Hour(value);
      },
      size: 100,
    },
    {
      accessorKey: 'openPositions',
      header: () => (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <span style={{ fontWeight: 'bold' }}>Open Positions</span>
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
      cell: ({ getValue, row }) => {
        const value = getValue();
        const positions = value ? value.split(',').map(p => p.trim()) : [];
        
        return (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
            {positions.map((position, index) => {
              const positionKey = position.toLowerCase().trim();
              const positionId = positionLookup[positionKey];
              
              const positionClaimCount = allClaims.filter(claim => {
                const matchesGame = Number(claim.gameId) === Number(row.original.gameId);
                const notWithdrawn = claim.claimStatus !== 'Withdrawn';
                const matchesPosition = positionId ? Number(claim.positionId) === Number(positionId) : false;
                
                return matchesGame && notWithdrawn && matchesPosition;
              }).length;
              
              let color = '#3F9033';
              if (positionClaimCount >= 3) {
                color = '#A80000';
              } else if (positionClaimCount >= 1) {
                color = '#0066CC';
              }
              
              return (
                <span key={index} style={{ color, fontWeight: 'bold' }}>
                  {position}{index < positions.length - 1 ? ',' : ''}
                </span>
              );
            })}
          </Box>
        );
      },
      size: 260,
    },
    {
      accessorKey: 'ageLevelName',
      header: 'Age Level',
      size: 120,
    },
    {
      accessorKey: 'homeClub',
      header: 'Home',
      size: 150,
    },
    {
      accessorKey: 'awayClub',
      header: 'Away',
      size: 150,
    },
    {
      accessorKey: 'venueName',
      header: 'Venue',
      size: 180,
    },
    {
      accessorKey: 'leagueName',
      header: 'League',
      size: 150,
    },
    {
      accessorKey: 'gameNotes',
      header: 'Notes',
      size: 250,
    },
  ], [userClaims, allClaims, positionLookup]);

  const table = useReactTable({
    data: games,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    initialState: {
      pagination: {
        pageSize: 25,
      },
    },
  });

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

      <Paper className="open-games-grid-container" sx={{ overflow: 'auto', maxHeight: 'calc(100vh - 200px)', display: 'flex', flexDirection: 'column', backgroundColor: 'white' }}>
        <Box sx={{ minWidth: 'fit-content', flex: 1, overflow: 'auto', backgroundColor: 'white' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              {table.getHeaderGroups().map(headerGroup => (
                <tr key={headerGroup.id} style={{ backgroundColor: '#f5f5f5', borderBottom: '2px solid #e0e0e0' }}>
                  {headerGroup.headers.map(header => (
                    <th
                      key={header.id}
                      style={{
                        padding: '12px 16px',
                        textAlign: 'left',
                        fontWeight: 600,
                        fontSize: '0.875rem',
                        width: header.getSize(),
                        cursor: header.column.getCanSort() ? 'pointer' : 'default',
                        userSelect: 'none',
                      }}
                      onClick={header.column.getToggleSortingHandler()}
                    >
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        {header.isPlaceholder
                          ? null
                          : flexRender(
                              header.column.columnDef.header,
                              header.getContext()
                            )}
                        {header.column.getCanSort() && (
                          <Box sx={{ display: 'flex', flexDirection: 'column', ml: 0.5 }}>
                            {header.column.getIsSorted() === 'asc' && <ArrowUpward sx={{ fontSize: 16 }} />}
                            {header.column.getIsSorted() === 'desc' && <ArrowDownward sx={{ fontSize: 16 }} />}
                            {!header.column.getIsSorted() && (
                              <Box sx={{ opacity: 0.3, fontSize: 16 }}>⇅</Box>
                            )}
                          </Box>
                        )}
                      </Box>
                    </th>
                  ))}
                </tr>
              ))}
            </thead>
            <tbody>
              {table.getRowModel().rows.map(row => (
                <tr
                  key={row.id}
                  style={{
                    borderBottom: '1px solid #e0e0e0',
                    backgroundColor: 'white',
                  }}
                >
                  {row.getVisibleCells().map(cell => (
                    <td
                      key={cell.id}
                      style={{
                        padding: '12px 16px',
                        fontSize: '0.875rem',
                        verticalAlign: 'middle',
                        width: cell.column.getSize(),
                      }}
                    >
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </Box>

        {/* Pagination */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: 2, p: 1, borderTop: '1px solid #e0e0e0', flexShrink: 0, backgroundColor: 'white' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="body2">Rows per page:</Typography>
            <Select
              value={table.getState().pagination.pageSize}
              onChange={e => table.setPageSize(Number(e.target.value))}
              size="small"
              sx={{ minWidth: 70 }}
            >
              {[10, 25, 50, 100].map(pageSize => (
                <MenuItem key={pageSize} value={pageSize}>
                  {pageSize}
                </MenuItem>
              ))}
            </Select>
          </Box>
          <Typography variant="body2">
            {table.getState().pagination.pageIndex * table.getState().pagination.pageSize + 1}–
            {Math.min(
              (table.getState().pagination.pageIndex + 1) * table.getState().pagination.pageSize,
              table.getFilteredRowModel().rows.length
            )} of {table.getFilteredRowModel().rows.length}
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              size="small"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              Previous
            </Button>
            <Button
              variant="outlined"
              size="small"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              Next
            </Button>
          </Box>
        </Box>
      </Paper>

      {hasCreatePermission && (
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', marginTop: '20px', marginRight: '24px' }}>
          <Button
            variant="contained"
            onClick={handleCreateGame}
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

      <CreateGame
        open={createDrawerOpen}
        onClose={handleCloseCreateDrawer}
        onCreateSuccess={handleCreateSuccess}
      />
    </Box>
  );
}

export default OpenGames;
