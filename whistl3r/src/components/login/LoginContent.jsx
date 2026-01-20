import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useLoading } from '../../contexts/LoadingContext';
import authService from '../../services/authService';
import whistlersLogo from '../../assets/images/WHISTLERS_LOGO_DARK.png';
import '../shared/App.css';
import './Login.css';

function LoginContent() {
  const navigate = useNavigate();
  const { showLoading, hideLoading } = useLoading();
  
  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });

  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

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
    setIsLoading(true);
    showLoading();

    try {
      const result = await authService.login(formData);

      if (result.success) {
        // Redirect to dashboard or home page
        navigate('/dashboard');
      } else {
        setErrors(result.errors || {});
      }
    } catch {
      setErrors({ general: 'An unexpected error occurred. Please try again.' });
    } finally {
      setIsLoading(false);
      hideLoading();
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  return (
    <div className="login-card">
      <div style={{ textAlign: 'center', marginBottom: '10px' }}>
        <img
          src={whistlersLogo}
          alt="Whistlers Logo"
          style={{ maxWidth: '375px', height: 'auto', marginBottom: '5px' }}
        />
      </div>
      <h2 className="whistler-text-heading" style={{ textAlign: 'center' }}>Login</h2>
        <p className="login-subtitle whistler-text-subheading" style={{ textAlign: 'center' }}>
          <a href="/register" className="whistler-text-subheading">New Users Registeration</a>
        </p>

        {errors.general && (
          <div className="alert alert-error">
            {errors.general}
          </div>
        )}

        <form onSubmit={handleSubmit} className="login-form">
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
            <label htmlFor="password" className="whistler-text">Password *</label>
            <div className="password-input-wrapper">
              <input
                type={showPassword ? 'text' : 'password'}
                id="password"
                name="password"
                value={formData.password}
                onChange={handleChange}
                className={errors.password ? 'input-error' : ''}
                required
                disabled={isLoading}
              />
              <button
                type="button"
                className="password-toggle"
                onClick={togglePasswordVisibility}
                aria-label={showPassword ? 'Hide password' : 'Show password'}
              >
                {showPassword ? '👁️' : '👁️‍🗨️'}
              </button>
            </div>
            {errors.password && (
              <span className="error-message whistler-text">{errors.password}</span>
            )}
          </div>

          <button
            type="submit"
            className="btn-primary whistler-text"
            disabled={isLoading}
          >
            {isLoading ? 'Logging in...' : 'Submit'}
          </button>
        </form>

        <div className="login-footer">
          <p className="whistler-text-subheading">
            <a href="/forgot-password" className="whistler-text-subheading">Forgot Password</a>
          </p>
        </div>
      </div>
  );
}

export default LoginContent;
