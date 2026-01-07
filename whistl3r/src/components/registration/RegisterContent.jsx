import { useState, useEffect } from 'react';
import { Tooltip } from '@mui/material';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import StyledSelect from '../shared/StyledSelect';
import { useLoading } from '../../contexts/LoadingContext';
import authService from '../../services/authService';
import whistlersLogo from '../../assets/images/WHISTLERS_LOGO_DARK.png';
import '../shared/App.css';
import './Register.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

function RegisterContent() {
  const { showLoading, hideLoading } = useLoading();
  
  const [formData, setFormData] = useState({
    email: '',
    firstName: '',
    lastName: '',
    phone: '',
    sportId: '',
    leagueId: '',
    roleIds: [],
    roleOrganizations: {}, // { roleId: [orgId1, orgId2, ...] }
    password: '',
    confirmPassword: '',
  });

  const [sports, setSports] = useState([]);
  const [leagues, setLeagues] = useState([]);
  const [allLeagues, setAllLeagues] = useState([]);
  const [roles, setRoles] = useState([]);
  const [organizations, setOrganizations] = useState([]);

  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      showLoading();
      try {
        const [sportsResponse, leaguesResponse, rolesResponse, organizationsResponse] = await Promise.all([
          fetch(`${API_BASE_URL}/sports`),
          fetch(`${API_BASE_URL}/leagues`),
          fetch(`${API_BASE_URL}/roles`),
          fetch(`${API_BASE_URL}/organizations`)
        ]);

        if (sportsResponse.ok) {
          const sportsData = await sportsResponse.json();
          setSports(sportsData);
          // Auto-select if only one sport
          if (sportsData.length === 1) {
            setFormData((prev) => ({
              ...prev,
              sportId: sportsData[0].sportId,
            }));
          }
        }

        if (leaguesResponse.ok) {
          const leaguesData = await leaguesResponse.json();
          setAllLeagues(leaguesData);
        }

        if (rolesResponse.ok) {
          const rolesData = await rolesResponse.json();
          setRoles(rolesData);
        }

        if (organizationsResponse.ok) {
          const organizationsData = await organizationsResponse.json();
          setOrganizations(organizationsData);
        }
      } catch (error) {
        console.error('Failed to fetch data:', error);
      } finally {
        hideLoading();
      }
    };

    fetchData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Filter leagues when sport changes
  useEffect(() => {
    if (formData.sportId) {
      const filteredLeagues = allLeagues.filter(
        (league) => league.sportId === parseInt(formData.sportId)
      );
      setLeagues(filteredLeagues);
      
      // Auto-select if only one league
      if (filteredLeagues.length === 1) {
        setFormData((prev) => ({
          ...prev,
          leagueId: filteredLeagues[0].leagueId,
        }));
      } else {
        // Clear league selection if there are multiple options or sport changes
        setFormData((prev) => ({
          ...prev,
          leagueId: '',
        }));
      }
    } else {
      setLeagues([]);
      setFormData((prev) => ({
        ...prev,
        leagueId: '',
      }));
    }
  }, [formData.sportId, allLeagues]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    
    // Clear error for this field when user starts typing
    if (errors[name]) {
      setErrors((prev) => ({
        ...prev,
        [name]: '',
      }));
    }
  };

  const handleRoleOrganizationChange = (roleId, selectedOrgIds) => {
    setFormData((prev) => ({
      ...prev,
      roleOrganizations: {
        ...prev.roleOrganizations,
        [roleId]: selectedOrgIds,
      },
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});
    setSuccessMessage('');

    // Validate that at least one organization is selected
    const hasOrganizations = Object.values(formData.roleOrganizations).some(
      orgIds => Array.isArray(orgIds) && orgIds.length > 0
    );

    if (!hasOrganizations) {
      setErrors({ 
        organizations: 'Please select at least one organization for your role(s)' 
      });
      return;
    }

    setIsLoading(true);

    try {
      const result = await authService.register(formData);

      if (result.success) {
        setSuccessMessage(
          'Registration successful! Your account has been created and is pending activation. Please wait for an administrator to activate your account.'
        );
        // Clear form
        setFormData({
          email: '',
          firstName: '',
          lastName: '',
          phone: '',
          sportId: '',
          leagueId: '',
          roleIds: [],
          roleOrganizations: {},
          password: '',
          confirmPassword: '',
        });
      } else {
        setErrors(result.errors || {});
      }
    } catch {
      setErrors({ general: 'An unexpected error occurred. Please try again.' });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="register-container">
      <div className="register-card">
        <div style={{ textAlign: 'center', marginBottom: '10px' }}>
          <img
            src={whistlersLogo}
            alt="Whistlers Logo"
            style={{ maxWidth: '250px', height: 'auto', marginBottom: '5px' }}
          />
        </div>
        <h2 className="whistler-text-heading" style={{ textAlign: 'center' }}>Create Account</h2>
        <p className="register-subtitle whistler-text-subheading" style={{ textAlign: 'center' }}>Join Whistlers to get started.  After registering there will be short 
          delay prior to activation while your account information is reviewed.  You will receive an email notification once your account is activated.</p>

        {successMessage && (
          <div className="alert alert-success">
            {successMessage}
          </div>
        )}

        {errors.general && (
          <div className="alert alert-error">
            {errors.general}
          </div>
        )}

        <form onSubmit={handleSubmit} className="register-form">
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="firstName" className="whistler-text">First Name <span style={{ color: '#ef4444' }}>*</span></label>
              <input
                type="text"
                id="firstName"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                className={errors.firstName ? 'input-error' : ''}
                required
                disabled={isLoading}
              />
              {errors.firstName && (
                <span className="error-message whistler-text">{errors.firstName}</span>
              )}
            </div>

            <div className="form-group">
              <label htmlFor="lastName" className="whistler-text">Last Name <span style={{ color: '#ef4444' }}>*</span></label>
              <input
                type="text"
                id="lastName"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                className={errors.lastName ? 'input-error' : ''}
                required
                disabled={isLoading}
              />
              {errors.lastName && (
                <span className="error-message whistler-text">{errors.lastName}</span>
              )}
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="email" className="whistler-text">Email <span style={{ color: '#ef4444' }}>*</span></label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              className={errors.email ? 'input-error' : ''}
              required
              disabled={isLoading}
            />
            {errors.email && (
              <span className="error-message whistler-text">{errors.email}</span>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="phone" className="whistler-text">Phone (optional)</label>
            <input
              type="tel"
              id="phone"
              name="phone"
              value={formData.phone}
              onChange={handleChange}
              className={errors.phone ? 'input-error' : ''}
              placeholder="+1 (555) 123-4567"
              disabled={isLoading}
            />
            {errors.phone && (
              <span className="error-message whistler-text">{errors.phone}</span>
            )}
          </div>

          <hr style={{ margin: '10px 0', border: 'none', borderTop: '1px solid #e2e8f0' }} />

          <div className="form-group">
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px', marginBottom: '8px' }}>
              <label htmlFor="sportId" className="whistler-text" style={{ margin: 0, fontSize: '14px', fontWeight: 600, color: '#FFFFFF' }}>
                Sport <span style={{ color: '#ef4444' }}>*</span>
              </label>
              <Tooltip 
                title="Select just one sport to start, you can add more later" 
                placement="top"
                arrow
              >
                <InfoOutlinedIcon 
                  sx={{ 
                    fontSize: 18, 
                    color: '#667eea', 
                    cursor: 'help',
                    '&:hover': { color: '#5a67d8' }
                  }} 
                />
              </Tooltip>
            </div>
            <StyledSelect
              label=""
              id="sportId"
              name="sportId"
              value={formData.sportId}
              onChange={handleChange}
              required
              disabled={isLoading}
              error={!!errors.sportId}
              helperText={errors.sportId}
              options={sports.map(sport => ({
                value: sport.sportId,
                label: sport.sportName
              }))}
              placeholder="Select a sport..."
            />
          </div>

        <div className="form-group">
<div style={{ display: 'flex', alignItems: 'center', gap: '6px', marginBottom: '8px' }}>
              <label htmlFor="leagueId" className="whistler-text" style={{ margin: 0, fontSize: '14px', fontWeight: 600, color: '#FFFFFF' }}>
                League <span style={{ color: '#ef4444' }}>*</span>
              </label>
              <Tooltip 
                title="Select just one league to start, you can add more later" 
                placement="top"
                arrow
              >
                <InfoOutlinedIcon 
                  sx={{ 
                    fontSize: 18, 
                    color: '#667eea', 
                    cursor: 'help',
                    '&:hover': { color: '#5a67d8' }
                  }} 
                />
              </Tooltip>
            </div>
          {formData.sportId && (
            <div className="form-group">
              <StyledSelect
                label=""
                id="leagueId"
                name="leagueId"
                value={formData.leagueId}
                onChange={handleChange}
                required
                disabled={isLoading}
                error={!!errors.leagueId}
                helperText={errors.leagueId}
                options={leagues.map(league => ({
                  value: league.leagueId,
                  label: league.leagueName
                }))}
                placeholder="Select a league..."
              />
            </div>
          )}
          </div>
          {formData.leagueId && (
            <div className="form-group">
              <StyledSelect
                label={<span>Role(s) <span style={{ color: '#ef4444' }}>*</span></span>}
                id="roleIds"
                name="roleIds"
                value={formData.roleIds}
                onChange={handleChange}
                required
                disabled={isLoading}
                error={!!errors.roleIds}
                helperText={errors.roleIds}
                options={roles.map(role => ({
                  value: role.roleId,
                  label: role.roleName
                }))}
                placeholder="Select role(s)..."
                multiple={true}
              />
            </div>
          )}
           <hr style={{ margin: '10px 0', border: 'none', borderTop: '1px solid #e2e8f0' }} />
          {formData.roleIds.length > 0 && (
            <>
   
              {formData.roleIds.map((roleId) => {
                const role = roles.find(r => r.roleId === roleId);
                if (!role) return null;
                return (
                  <div key={roleId} className="form-group">
                    <StyledSelect
                      label={<span>Organizations for {role.roleName} <span style={{ color: '#ef4444' }}>*</span></span>}
                      id={`role-org-${roleId}`}
                      name={`role-org-${roleId}`}
                      value={formData.roleOrganizations[roleId] || []}
                      onChange={(e) => handleRoleOrganizationChange(roleId, e.target.value)}
                      disabled={isLoading}
                      options={organizations.map(org => ({
                        value: org.organizationId,
                        label: org.organizationName
                      }))}
                      placeholder="Select organizations..."
                      multiple={true}
                    />
                  </div>
                );
              })}
              {errors.organizations && (
                <div className="alert alert-error" style={{ marginTop: '10px' }}>
                  {errors.organizations}
                </div>
              )}
              <hr style={{ margin: '10px 0', border: 'none', borderTop: '1px solid #e2e8f0' }} />
            </>
          )}

          <div className="form-group">
            <label htmlFor="password" className="whistler-text">Password <span style={{ color: '#ef4444' }}>*</span></label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              className={errors.password ? 'input-error' : ''}
              required
              disabled={isLoading}
              minLength={8}
            />
            {errors.password && (
              <span className="error-message whistler-text">{errors.password}</span>
            )}
            <span className="help-text whistler-text">Minimum 8 characters</span>
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword" className="whistler-text">Confirm Password <span style={{ color: '#ef4444' }}>*</span></label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              className={errors.confirmPassword ? 'input-error' : ''}
              required
              disabled={isLoading}
            />
            {errors.confirmPassword && (
              <span className="error-message whistler-text">{errors.confirmPassword}</span>
            )}
          </div>

          <button
            type="submit"
            className="btn-primary whistler-text"
            disabled={isLoading}
          >
            {isLoading ? 'Creating Account...' : 'Create Account'}
          </button>
        </form>

        <div className="register-footer">
          <p className="whistler-text">
            Already have an account? <a href="/login" className="whistler-text">Sign in</a>
          </p>
        </div>
      </div>
    </div>
  );
}

export default RegisterContent;
