import { useState, useEffect } from 'react';
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
    roleId: '',
    password: '',
    confirmPassword: '',
  });

  const [sports, setSports] = useState([]);
  const [leagues, setLeagues] = useState([]);
  const [allLeagues, setAllLeagues] = useState([]);
  const [roles, setRoles] = useState([]);

  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      showLoading();
      try {
        const [sportsResponse, leaguesResponse, rolesResponse] = await Promise.all([
          fetch(`${API_BASE_URL}/sports`),
          fetch(`${API_BASE_URL}/leagues`),
          fetch(`${API_BASE_URL}/roles`)
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

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});
    setSuccessMessage('');
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
          roleId: '',
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
        <p className="register-subtitle whistler-text-subheading" style={{ textAlign: 'center' }}>Join Whistlers to get started</p>

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
              <label htmlFor="firstName" className="whistler-text">First Name *</label>
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
              <label htmlFor="lastName" className="whistler-text">Last Name *</label>
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
            <label htmlFor="email" className="whistler-text">Email *</label>
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

          <div className="form-group">
            <StyledSelect
              label="Sport *"
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

          {formData.sportId && (
            <div className="form-group">
              <StyledSelect
                label="League *"
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

          {formData.leagueId && (
            <div className="form-group">
              <StyledSelect
                label="Role *"
                id="roleId"
                name="roleId"
                value={formData.roleId}
                onChange={handleChange}
                required
                disabled={isLoading}
                error={!!errors.roleId}
                helperText={errors.roleId}
                options={roles.map(role => ({
                  value: role.roleId,
                  label: role.roleName
                }))}
                placeholder="Select a role..."
              />
            </div>
          )}

          <div className="form-group">
            <label htmlFor="password" className="whistler-text">Password *</label>
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
            <label htmlFor="confirmPassword" className="whistler-text">Confirm Password *</label>
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
