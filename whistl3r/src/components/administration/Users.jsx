import { useState, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { Box, Typography, Paper, Select, MenuItem, FormControl, Button } from '@mui/material';
import { useLoading } from '../../contexts/LoadingContext';
import EditUser from './EditUser';
import './Users.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function Users() {
  const { showLoading, hideLoading } = useLoading();
  const [users, setUsers] = useState([]);
  const [error, setError] = useState(null);
  const [userFilter, setUserFilter] = useState('all');
  const [editUserOpen, setEditUserOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState(null);

  useEffect(() => {
    fetchUsers();
  }, []);

  const handleFilterChange = (event) => {
    setUserFilter(event.target.value);
  };

  const handleEditUser = (userId) => {
    setSelectedUserId(userId);
    setEditUserOpen(true);
  };

  const handleCloseEditUser = () => {
    setEditUserOpen(false);
    setSelectedUserId(null);
    // Refresh the users list
    fetchUsers();
  };

  const fetchUsers = async () => {
    showLoading();
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${API_BASE_URL}/users`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setUsers(data);
        setError(null);
      } else {
        setError('Failed to load users');
      }
    } catch (err) {
      console.error('Error fetching users:', err);
      setError('An error occurred while loading users');
    } finally {
      hideLoading();
    }
  };

  const columns = [
    {
      field: 'actions',
      headerName: 'Actions',
      width: 100,
      sortable: false,
      renderCell: (params) => (
        <Button
          variant="contained"
          size="small"
          onClick={() => handleEditUser(params.row.userId)}
          sx={{
            backgroundColor: '#667eea',
            '&:hover': { backgroundColor: '#5568d3' },
            textTransform: 'none'
          }}
        >
          Edit
        </Button>
      )
    },
    { 
      field: 'userId', 
      headerName: 'User ID', 
      width: 100 
    },
    { 
      field: 'firstName', 
      headerName: 'First Name', 
      width: 150 
    },
    { 
      field: 'lastName', 
      headerName: 'Last Name', 
      width: 150 
    },
    { 
      field: 'email', 
      headerName: 'Email', 
      width: 250 
    },
    { 
      field: 'phone', 
      headerName: 'Phone', 
      width: 150 
    },
    { 
      field: 'userRoles', 
      headerName: 'Roles', 
      width: 200,
      valueGetter: (value, row) => {
        if (row.userRoles && Array.isArray(row.userRoles)) {
          return row.userRoles.map(ur => ur.role.roleName).join(', ');
        }
        return '';
      }
    },
    { 
      field: 'isActive', 
      headerName: 'Active', 
      width: 100,
      type: 'boolean'
    },
    { 
      field: 'emailVerified', 
      headerName: 'Email Verified', 
      width: 130,
      type: 'boolean'
    },
    { 
      field: 'createdAt', 
      headerName: 'Created At', 
      width: 180,
      valueFormatter: (value) => {
        if (!value) return '';
        return new Date(value).toLocaleString();
      }
    },
    { 
      field: 'lastLogin', 
      headerName: 'Last Login', 
      width: 180,
      valueFormatter: (value) => {
        if (!value) return 'Never';
        return new Date(value).toLocaleString();
      }
    }
  ];

  return (
    <Box className="users-container">
      <Box className="users-header">
        <FormControl size="small" className="users-filter" variant="outlined">
          <Select
            value={userFilter}
            onChange={handleFilterChange}
            displayEmpty
            sx={{
              backgroundColor: '#3F9033',
              color: 'white',
              minWidth: 180,
              height: 36,
              marginTop: 3,
              marginBottom: 4,
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
            <MenuItem value="all">All Users</MenuItem>
            <MenuItem value="pending">Pending Users</MenuItem>
            <MenuItem value="active">Active Users</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {error && (
        <Paper className="users-error">
          <Typography color="error">{error}</Typography>
        </Paper>
      )}

      <Paper className="users-grid-container">
        <DataGrid
          rows={users}
          columns={columns}
          getRowId={(row) => row.userId}
          initialState={{
            pagination: {
              paginationModel: { pageSize: 25 },
            },
          }}
          pageSizeOptions={[10, 25, 50, 100]}
          disableRowSelectionOnClick
          className="users-datagrid"
        />
      </Paper>

      <EditUser
        open={editUserOpen}
        onClose={handleCloseEditUser}
        userId={selectedUserId}
      />
    </Box>
  );
}

export default Users;
