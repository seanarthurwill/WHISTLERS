import { useState, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { Box, Typography, Paper } from '@mui/material';
import { useLoading } from '../../contexts/LoadingContext';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function OpenGames() {
  const { showLoading, hideLoading } = useLoading();
  const [games, setGames] = useState([]);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchGames = async () => {
      showLoading();
      try {
        const response = await fetch(`${API_BASE_URL}/games/details-report`);
        if (response.ok) {
          const data = await response.json();
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

    fetchGames();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const columns = [
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
      field: 'sportName', 
      headerName: 'Sport', 
      width: 120 
    },
    { 
      field: 'leagueName', 
      headerName: 'League', 
      width: 150 
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
      field: 'gameStatusName', 
      headerName: 'Status', 
      width: 120 
    },
    { 
      field: 'positionName', 
      headerName: 'Position', 
      width: 130 
    },
    { 
      field: 'positionStatus', 
      headerName: 'Position Status', 
      width: 140,
      cellClassName: (params) => {
        if (params.value === 'Open') return 'status-open';
        if (params.value === 'Assigned') return 'status-assigned';
        return '';
      }
    },
    { 
      field: 'assignedOfficial', 
      headerName: 'Assigned Official', 
      width: 180 
    },
    { 
      field: 'positionRequired', 
      headerName: 'Required', 
      width: 100,
      type: 'boolean'
    },
    { 
      field: 'minRequired', 
      headerName: 'Min', 
      width: 70,
      type: 'number'
    },
    { 
      field: 'maxAllowed', 
      headerName: 'Max', 
      width: 70,
      type: 'number'
    },
    { 
      field: 'gameNotes', 
      headerName: 'Notes', 
      width: 250,
      flex: 1
    }
  ];

  return (
    <Box sx={{ 
      height: 'calc(100vh - 100px)', 
      width: '100%', 
      p: 3,
      backgroundColor: '#1a1a2e'
    }}>
      <Paper sx={{ 
        p: 2, 
        mb: 2,
        backgroundColor: '#16213e',
        color: '#fff'
      }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Open Games
        </Typography>
        <Typography variant="body2" color="rgba(255,255,255,0.7)">
          View all games with position assignments and availability
        </Typography>
      </Paper>

      {error && (
        <Paper sx={{ 
          p: 2, 
          mb: 2, 
          backgroundColor: '#ff4444',
          color: '#fff'
        }}>
          <Typography>{error}</Typography>
        </Paper>
      )}

      <Paper sx={{ 
        height: 'calc(100% - 120px)', 
        width: '100%',
        backgroundColor: '#16213e'
      }}>
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
          sx={{
            border: 'none',
            '& .MuiDataGrid-cell': {
              color: '#fff',
              borderColor: 'rgba(255,255,255,0.1)'
            },
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: '#667eea',
              color: '#fff',
              borderColor: 'rgba(255,255,255,0.1)'
            },
            '& .MuiDataGrid-columnHeaderTitle': {
              fontWeight: 'bold'
            },
            '& .MuiDataGrid-row': {
              '&:hover': {
                backgroundColor: 'rgba(102, 126, 234, 0.1)'
              }
            },
            '& .MuiDataGrid-footerContainer': {
              backgroundColor: '#16213e',
              borderColor: 'rgba(255,255,255,0.1)',
              color: '#fff'
            },
            '& .MuiTablePagination-root': {
              color: '#fff'
            },
            '& .MuiIconButton-root': {
              color: '#fff'
            },
            '& .status-open': {
              color: '#4ade80',
              fontWeight: 'bold'
            },
            '& .status-assigned': {
              color: '#60a5fa',
              fontWeight: 'bold'
            }
          }}
        />
      </Paper>
    </Box>
  );
}

export default OpenGames;
