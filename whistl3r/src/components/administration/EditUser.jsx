import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import {
  Drawer,
  Box,
  Typography,
  Button,
  TextField,
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
import './EditUser.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function EditUser({ open, onClose, userId }) {
  const { showLoading, hideLoading } = useLoading();
  const [userDetails, setUserDetails] = useState(null);
  const [allRoles, setAllRoles] = useState([]);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    phone: '',
    isActive: false,
    emailVerified: false,
    selectedRoles: []
  });
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (open && userId) {
      fetchUserDetails();
      fetchAllRoles();
    } else {
      // Reset state when drawer closes
      setUserDetails(null);
      setFormData({
        firstName: '',
        lastName: '',
        phone: '',
        isActive: false,
        emailVerified: false,
        selectedRoles: []
      });
      setError(null);
      setSuccess(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, userId]);

  const fetchAllRoles = async () => {
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${API_BASE_URL}/roles`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const roles = await response.json();
        setAllRoles(roles);
      }
    } catch (err) {
      console.error('Error fetching roles:', err);
    }
  };

  const fetchUserDetails = async () => {
    showLoading();
    setError(null);
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`${API_BASE_URL}/users/${userId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setUserDetails(data);
        
        // Populate form data
        setFormData({
          firstName: data.firstName || '',
          lastName: data.lastName || '',
          phone: data.phone || '',
          isActive: data.isActive || false,
          emailVerified: data.emailVerified || false,
          selectedRoles: data.userRoles?.map(ur => ur.roleId) || []
        });
      } else {
        setError('Failed to load user details');
      }
    } catch (err) {
      console.error('Error fetching user details:', err);
      setError('An error occurred while loading user details');
    } finally {
      hideLoading();
    }
  };

  const handleInputChange = (field, value) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleRoleToggle = (roleId) => {
    setFormData(prev => ({
      ...prev,
      selectedRoles: prev.selectedRoles.includes(roleId)
        ? prev.selectedRoles.filter(id => id !== roleId)
        : [...prev.selectedRoles, roleId]
    }));
  };

  const handleSave = async () => {
    setError(null);
    setSuccess(false);
    showLoading();

    try {
      const token = localStorage.getItem('accessToken');
      
      // Build the user roles array
      const userRoles = formData.selectedRoles.map(roleId => ({
        userId: userId,
        roleId: roleId
      }));

      // Build the update payload
      const updatePayload = {
        userId: userId,
        firstName: formData.firstName,
        lastName: formData.lastName,
        phone: formData.phone,
        isActive: formData.isActive,
        emailVerified: formData.emailVerified,
        email: userDetails.email,
        passwordHash: userDetails.passwordHash, // Send existing hash (won't be updated)
        tenantId: userDetails.tenantId,
        userRoles: userRoles
      };

      const response = await fetch(`${API_BASE_URL}/users/${userId}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(updatePayload)
      });

      if (response.ok) {
        setSuccess(true);
        setError(null);
        
        // Close drawer after a short delay to show success message
        setTimeout(() => {
          onClose();
        }, 1500);
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to update user');
      }
    } catch (err) {
      console.error('Error updating user:', err);
      setError('An error occurred while updating the user');
    } finally {
      hideLoading();
    }
  };

  const handleCancel = () => {
    onClose();
  };

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      PaperProps={{
        sx: { width: { xs: '100%', sm: 450 } }
      }}
    >
      <Box className="edit-user-drawer">
        {/* Header */}
        <Box className="edit-user-header">
          <Typography variant="h6" className="edit-user-title">
            Edit User
          </Typography>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>

        {/* Content */}
        <Box className="edit-user-content">
          {!userDetails ? (
            <Box className="edit-user-loading">
              <CircularProgress />
            </Box>
          ) : (
            <>
              {/* User Email (Display Only) */}
              <Box sx={{ mb: 3 }}>
                <Typography variant="subtitle2" sx={{ mb: 1, color: '#666' }}>
                  Email
                </Typography>
                <Typography variant="body1" sx={{ fontWeight: 500 }}>
                  {userDetails.email}
                </Typography>
              </Box>

              <Divider sx={{ my: 2 }} />

              {/* First Name */}
              <TextField
                fullWidth
                label="First Name"
                value={formData.firstName}
                onChange={(e) => handleInputChange('firstName', e.target.value)}
                sx={{ mb: 2 }}
              />

              {/* Last Name */}
              <TextField
                fullWidth
                label="Last Name"
                value={formData.lastName}
                onChange={(e) => handleInputChange('lastName', e.target.value)}
                sx={{ mb: 2 }}
              />

              {/* Phone */}
              <TextField
                fullWidth
                label="Phone"
                value={formData.phone}
                onChange={(e) => handleInputChange('phone', e.target.value)}
                sx={{ mb: 3 }}
              />

              <Divider sx={{ my: 2 }} />

              {/* User Roles */}
              <Typography variant="subtitle1" sx={{ mb: 1, fontWeight: 600 }}>
                User Roles
              </Typography>
              <FormGroup sx={{ mb: 3 }}>
                {allRoles.map(role => (
                  <FormControlLabel
                    key={role.roleId}
                    control={
                      <Checkbox
                        checked={formData.selectedRoles.includes(role.roleId)}
                        onChange={() => handleRoleToggle(role.roleId)}
                      />
                    }
                    label={role.roleName}
                  />
                ))}
              </FormGroup>

              <Divider sx={{ my: 2 }} />

              {/* Active Status */}
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.isActive}
                    onChange={(e) => handleInputChange('isActive', e.target.checked)}
                  />
                }
                label="Active"
              />

              {/* Email Verified (Read-only) */}
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.emailVerified}
                    disabled
                  />
                }
                label="Email Verified"
                sx={{ display: 'block' }}
              />

              {/* Error Messages */}
              {error && (
                <Alert severity="error" sx={{ mt: 2 }}>
                  {error}
                </Alert>
              )}

              {/* Success Messages */}
              {success && (
                <Alert severity="success" sx={{ mt: 2 }}>
                  User updated successfully!
                </Alert>
              )}
            </>
          )}
        </Box>

        {/* Footer */}
        <Box className="edit-user-footer">
          <Button
            variant="outlined"
            onClick={handleCancel}
            sx={{ mr: 1 }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleSave}
            sx={{
              backgroundColor: '#667eea',
              '&:hover': { backgroundColor: '#5568d3' }
            }}
          >
            Save
          </Button>
        </Box>
      </Box>
    </Drawer>
  );
}

EditUser.propTypes = {
  open: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  userId: PropTypes.number
};

export default EditUser;
