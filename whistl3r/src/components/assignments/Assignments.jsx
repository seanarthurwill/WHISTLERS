import { useState, useEffect, useMemo, Fragment } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  getExpandedRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  flexRender,
} from '@tanstack/react-table';
import { Box, Typography, Paper, Button, Select, MenuItem, FormControl, IconButton } from '@mui/material';
import { KeyboardArrowDown, KeyboardArrowRight, ArrowUpward, ArrowDownward } from '@mui/icons-material';
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
  const [officials, setOfficials] = useState({});
  const [positionAssignments, setPositionAssignments] = useState({});
  const [gameAssignments, setGameAssignments] = useState({});

  const handleClaim = (row) => {
    setSelectedGameId(row.gameId);
    setClaimDrawerOpen(true);
  };

  const handleClaimSuccess = () => {
    fetchGames();
  };

  const handleCloseDrawer = () => {
    setClaimDrawerOpen(false);
    setSelectedGameId(null);
  };

  const handleAssignmentChange = (gameId, positionId, officialId) => {
    const key = `${gameId}-${positionId}`;
    setPositionAssignments(prev => ({
      ...prev,
      [key]: officialId
    }));
  };

  const fetchGameAssignments = async (gameIds) => {
    try {
      const token = localStorage.getItem('accessToken');
      if (!token || gameIds.length === 0) {
        return;
      }

      const assignmentsPromises = gameIds.map(gameId =>
        fetch(`${API_BASE_URL}/game-assignments/game/${gameId}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        }).then(async res => {
          if (res.ok) {
            const data = await res.json();
            return { gameId, assignments: data };
          }
          return { gameId, assignments: [] };
        }).catch(() => ({ gameId, assignments: [] }))
      );

      const results = await Promise.all(assignmentsPromises);
      const assignmentsLookup = {};
      results.forEach(({ gameId, assignments }) => {
        assignmentsLookup[gameId] = assignments;
      });
      
      setGameAssignments(assignmentsLookup);
    } catch (err) {
      console.error('Error fetching game assignments:', err);
    }
  };

  const handleAssignPosition = async (gameId, positionId) => {
    const key = `${gameId}-${positionId}`;
    const selectedOfficialId = positionAssignments[key];

    try {
      const token = localStorage.getItem('accessToken');
      if (!token) {
        setError('Authentication required');
        return;
      }

      // If "open" is selected, don't do anything
      if (!selectedOfficialId || selectedOfficialId === 'open') {
        setError('Please select an official to assign');
        return;
      }

      // Get current user details
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const assignedBy = user.OfficialId || user.UserId || 1; // Default to 1 if not found

      // Create assignment
      const assignmentData = {
        gameId: parseInt(gameId),
        officialId: parseInt(selectedOfficialId),
        positionId: parseInt(positionId),
        assignmentStatus: 'Assigned',
        assignedAt: new Date().toISOString(),
        assignedBy: parseInt(assignedBy)
      };

      const response = await fetch(`${API_BASE_URL}/game-assignments`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(assignmentData)
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error('Assignment error:', errorText);
        setError('Failed to assign official to position');
        return;
      }

      console.log(`Successfully assigned official ${selectedOfficialId} to game ${gameId}, position ${positionId}`);
      
      // Clear the selection
      setPositionAssignments(prev => {
        const updated = { ...prev };
        delete updated[key];
        return updated;
      });

      // Refresh games data after assignment
      await fetchGames();
    } catch (err) {
      console.error('Error assigning position:', err);
      setError('Failed to assign position');
    }
  };

  const fetchUserClaims = async () => {
    try {
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const token = localStorage.getItem('accessToken');
      
      const officialId = user.OfficialId;
      
      if (!officialId || !token) {
        return [];
      }

      const gamesResponse = await fetch(`${API_BASE_URL}/games/details-report`);
      if (!gamesResponse.ok) {
        return [];
      }
      
      const gamesData = await gamesResponse.json();
      const uniqueGameIds = [...new Set(gamesData.map(g => g.gameId))];
      
      const claimsPromises = uniqueGameIds.map(gameId =>
        fetch(`${API_BASE_URL}/claims/game/${gameId}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        }).then(async res => {
          if (res.ok) {
            const data = await res.json();
            return data;
          }
          return [];
        }).catch(() => [])
      );
      
      const allClaims = await Promise.all(claimsPromises);
      const flatClaims = allClaims.flat();
      
      setAllClaims(flatClaims);
      
      // Fetch official details for all claiming officials
      const uniqueOfficialIds = [...new Set(flatClaims.map(claim => claim.officialId))];
      console.log('Unique official IDs from claims:', uniqueOfficialIds);
      await fetchOfficials(uniqueOfficialIds);
      
      const userPendingClaims = flatClaims.filter(
        claim => Number(claim.officialId) === Number(officialId) && claim.claimStatus === 'Pending'
      );
      
      setUserClaims(userPendingClaims);
      return userPendingClaims;
    } catch (err) {
      console.error('Error fetching user claims:', err);
      return [];
    }
  };

  const fetchOfficials = async (officialIds) => {
    try {
      const token = localStorage.getItem('accessToken');
      
      if (!token || officialIds.length === 0) {
        return;
      }

      const officialPromises = officialIds.map(officialId =>
        fetch(`${API_BASE_URL}/users/official/${officialId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }).then(async res => {
          console.log(`Fetch /users/official/${officialId}: status ${res.status}`);
          if (res.ok) {
            const user = await res.json();
            console.log(`Successfully fetched official ${officialId}:`, user);
            return { officialId, user };
          } else {
            const text = await res.text();
            console.warn(`Failed to fetch official ${officialId}: ${res.status} - ${text}`);
            return { officialId, user: null };
          }
        }).catch((err) => {
          console.error(`Error fetching official ${officialId}:`, err);
          return { officialId, user: null };
        })
      );

      const results = await Promise.all(officialPromises);
      
      const officialsLookup = {};
      results.forEach(({ officialId, user }) => {
        console.log(`Official ${officialId}:`, user);
        if (user) {
          officialsLookup[officialId] = user;
        }
      });
      
      console.log('Officials lookup:', officialsLookup);
      setOfficials(officialsLookup);
    } catch (err) {
      console.error('Error fetching officials:', err);
    }
  };

  const fetchGames = async () => {
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
        const gameIds = [...new Set(data.map(g => g.gameId))];
        fetchAllGameDetails(data);
        fetchGameAssignments(gameIds);
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
    
    const detailsPromises = uniqueGameIds.map(gameId =>
      fetch(`${API_BASE_URL}/games/${gameId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
      }).then(async res => {
        if (res.ok) {
          const detail = await res.json();
          return { gameId, detail };
        }
        return { gameId, detail: null };
      }).catch(() => ({ gameId, detail: null }))
    );

    const results = await Promise.all(detailsPromises);
    const detailsLookup = {};
    results.forEach(({ gameId, detail }) => {
      if (detail) {
        detailsLookup[gameId] = detail;
      }
    });
    
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

  const getGamesByStatus = (gamesData) => {
    if (gameView === 'upcoming') {
      return gamesData.filter(game => 
        game.gameStatusId === 1 || game.gameStatusId === 2
      );
    } else if (gameView === 'completed') {
      return gamesData.filter(game => 
        game.gameStatusId === 3
      );
    } else {
      // 'all' view - show all games
      return gamesData;
    }
  };

  useEffect(() => {
    let filtered = allGames;

    // Apply sport filter
    if (selectedSport !== 'all') {
      const selectedSportObj = sports.find(s => s.sportId === selectedSport);
      const selectedSportName = selectedSportObj?.sportName;
      
      filtered = filtered.filter(game => {
        return game.sportName === selectedSportName;
      });
    }

    // Apply game status filter
    filtered = getGamesByStatus(filtered);

    setGames(filtered);
  }, [selectedSport, gameView, allGames, sports]);

  const handleSportChange = (event) => {
    setSelectedSport(event.target.value);
  };

  const handleGameViewChange = (event) => {
    setGameView(event.target.value);
  };

  const columns = useMemo(() => [
    {
      id: 'expander',
      header: () => null,
      cell: ({ row }) => {
        const details = gameDetails[row.original.gameId];
        const assignments = gameAssignments[row.original.gameId] || [];
        const hasPositions = details?.openPositions && details.openPositions.length > 0;
        const canExpand = hasPositions || assignments.length > 0;
        
        if (!canExpand) return null;
        
        return (
          <IconButton
            size="small"
            onClick={row.getToggleExpandedHandler()}
            sx={{ padding: '4px' }}
          >
            {row.getIsExpanded() ? <KeyboardArrowDown /> : <KeyboardArrowRight />}
          </IconButton>
        );
      },
      size: 50,
    },
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
            {hasPendingClaim ? 'Edit' : 'Claim'}
          </Button>
        );
      },
      size: 120,
    },
    {
      id: 'claimStatus',
      header: 'Assignment Status',
      cell: ({ row }) => {
        const details = gameDetails[row.original.gameId];
        const gameStatus = details?.gameStatus?.name || 'Unknown';
        return (
          <Box sx={{ fontWeight: 'bold' }}>
            {gameStatus}
          </Box>
        );
      },
      size: 140,
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
  ], [gameDetails, userClaims, allClaims, gameAssignments]);

  const table = useReactTable({
    data: games,
    columns,
    getRowCanExpand: (row) => {
      const details = gameDetails[row.original.gameId];
      const assignments = gameAssignments[row.original.gameId] || [];
      return (details?.openPositions && details.openPositions.length > 0) || assignments.length > 0;
    },
    getCoreRowModel: getCoreRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    initialState: {
      pagination: {
        pageSize: 25,
      },
    },
  });

  const renderDetailPanel = (row) => {
    const details = gameDetails[row.original.gameId];
    const assignments = gameAssignments[row.original.gameId] || [];
    
    if (!details) {
      return null;
    }

    // Get open positions from API
    const openPositions = details.openPositions || [];
    
    // Create a map of position IDs that already have assignments
    const assignedPositionIds = new Set(assignments.map(a => a.positionId));
    
    // Merge open positions with assigned positions (to keep assigned ones visible)
    const displayPositions = [
      ...openPositions,
      ...assignments
        .filter(a => !openPositions.some(p => Number(p.positionId) === Number(a.positionId)))
        .map(a => ({
          positionId: a.positionId,
          positionName: `Position ${a.positionId} (Assigned)`
        }))
    ];

    if (displayPositions.length === 0) {
      return null;
    }

    return (
      <Box sx={{ py: 1, pl: '150px', pr: 6, backgroundColor: 'transparent' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.83015625rem' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #e0e0e0' }}>
              <th style={{ textAlign: 'left', padding: '8px', fontWeight: 600, color: '#666', width: '12%' }}>Position</th>
              <th style={{ textAlign: 'left', padding: '8px', fontWeight: 600, color: '#666', width: '10%' }}># of Claims</th>
              <th style={{ textAlign: 'left', padding: '8px', fontWeight: 600, color: '#666', width: '15%' }}>Assigned Official</th>
              <th style={{ textAlign: 'left', padding: '8px', fontWeight: 600, color: '#666' }}>Claims</th>
              <th style={{ textAlign: 'left', padding: '8px', fontWeight: 600, color: '#666', width: '100px' }}>Action</th>
            </tr>
          </thead>
          <tbody>
            {displayPositions.map((position) => {
              const positionClaims = allClaims.filter(
                claim => Number(claim.gameId) === Number(row.original.gameId) && 
                         Number(claim.positionId) === Number(position.positionId) &&
                         claim.claimStatus !== 'Withdrawn'
              );
              const claimCount = positionClaims.length;
              
              // Get the assigned official for this position if it exists
              const gamePositionAssignments = gameAssignments[row.original.gameId] || [];
              const assignment = gamePositionAssignments.find(
                a => Number(a.positionId) === Number(position.positionId)
              );
              
              let assignedOfficialName = 'Open';
              if (assignment) {
                const assignedOfficialData = officials[assignment.officialId];
                assignedOfficialName = assignedOfficialData 
                  ? `${assignedOfficialData.firstName || ''} ${assignedOfficialData.lastName || ''}`.trim()
                  : `Official ${assignment.officialId}`;
              }
              
              const assignmentKey = `${row.original.gameId}-${position.positionId}`;
              const selectedValue = positionAssignments[assignmentKey] || 'open';
              const isAssigned = assignedPositionIds.has(Number(position.positionId));

              return (
                <tr key={`${row.original.gameId}-${position.positionId}`} style={{ borderBottom: '1px solid #f0f0f0', backgroundColor: isAssigned ? '#f5f5f5' : 'transparent' }}>
                  <td style={{ padding: '8px', color: '#333', width: '12%' }}>{position.positionName}</td>
                  <td style={{ padding: '8px', color: '#333', width: '10%' }}>{claimCount}</td>
                  <td style={{ padding: '8px', color: assignedOfficialName === 'Open' ? '#3F9033' : '#000', fontWeight: 500, width: '15%' }}>
                    {assignedOfficialName}
                  </td>
                  <td style={{ padding: '8px', paddingRight: '25px' }}>
                    <FormControl size="small" fullWidth disabled={isAssigned}>
                      <Select
                        value={selectedValue}
                        onChange={(e) => handleAssignmentChange(row.original.gameId, position.positionId, e.target.value)}
                        disabled={isAssigned}
                        sx={{
                          fontSize: '0.75rem',
                          '& .MuiSelect-select': {
                            padding: '4px 8px'
                          }
                        }}
                      >
                        <MenuItem key="open" value="open" sx={{ fontSize: '0.75rem', color: '#3F9033', fontWeight: 600 }}>
                          Open
                        </MenuItem>
                        {positionClaims.map((claim) => {
                          const official = officials[claim.officialId];
                          const officialName = official 
                            ? `${official.firstName || ''} ${official.lastName || ''}`.trim()
                            : `Official ${claim.officialId}`;
                          return (
                            <MenuItem key={claim.gameClaimId} value={claim.officialId} sx={{ fontSize: '0.75rem' }}>
                              {officialName}
                            </MenuItem>
                          );
                        })}
                      </Select>
                    </FormControl>
                  </td>
                  <td style={{ padding: '8px', width: '100px' }}>
                    <Button
                      variant="contained"
                      size="small"
                      onClick={() => handleAssignPosition(row.original.gameId, position.positionId)}
                      disabled={isAssigned}
                      sx={{
                        backgroundColor: isAssigned ? '#ccc' : '#3F9033',
                        '&:hover': { backgroundColor: isAssigned ? '#ccc' : '#5aa84a' },
                        fontSize: '0.7rem',
                        minWidth: '70px',
                        padding: '4px 12px'
                      }}
                    >
                      {isAssigned ? 'Assigned' : 'Assign'}
                    </Button>
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
            <MenuItem value="all">All Games</MenuItem>
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
                <Fragment key={row.id}>
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
                  {row.getIsExpanded() && (
                    <tr>
                      <td colSpan={columns.length} style={{ padding: 0, backgroundColor: '#fafafa' }}>
                        {renderDetailPanel(row)}
                      </td>
                    </tr>
                  )}
                </Fragment>
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
