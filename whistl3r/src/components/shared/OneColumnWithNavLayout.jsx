import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { ChevronLeft, ChevronRight } from '@mui/icons-material';
import { Menu, MenuItem } from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';
import whistlersIcon from '../../assets/images/WHISTLERS_ICON_dark.png';
import whistlersDarkLogo from '../../assets/images/WHISTLERS_LOGO_DARK.png';
import rounderImage from '../../assets/images/rounder.png';
import './OneColumnWithNavLayout.css';

function OneColumnWithNavLayout({ children, navItems = [] }) {
  const [isExpanded, setIsExpanded] = useState(true);
  const [user, setUser] = useState(null);
  const [avatarColor, setAvatarColor] = useState('#667eea');
  const [anchorEl, setAnchorEl] = useState(null);
  const navigate = useNavigate();
  const location = useLocation();
  const open = Boolean(anchorEl);

  useEffect(() => {
    // Get user from localStorage
    const userStr = localStorage.getItem('user');
    //console.log('User string from localStorage:', userStr);
    if (userStr) {
      try {
        const userData = JSON.parse(userStr);
        console.log('Parsed user data:', userData);
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setUser(userData);
        
        // Generate random color for avatar
        const colors = ['#667eea', '#764ba2', '#f093fb', '#4facfe', '#43e97b', '#fa709a', '#30cfd0', '#a8edea'];
        const randomColor = colors[Math.floor(Math.random() * colors.length)];
        setAvatarColor(randomColor);
      } catch (err) {
        console.error('Failed to parse user data:', err);
      }
    }
  }, []);

  const getInitials = () => {

    if (!user) {
      console.log('No user found');
      return 'NA';
    }
    console.log('User object:', user);
    const firstInitial = user.given_name?.charAt(0) || user.firstName?.charAt(0) || '';
    const lastInitial = user.family_name?.charAt(0) || user.lastName?.charAt(0) || '';
    const initials = (firstInitial + lastInitial).toUpperCase();
    console.log('Generated initials:', initials);
    return initials || 'NA';
  };

  const toggleNav = () => {
    setIsExpanded(!isExpanded);
  };

  // Filter nav items based on user's assignor role
  const getFilteredNavItems = () => {
    if (!user) return [];
    
    // If user has AssignorId, show all nav items
    if (user.AssignorId) {
      return navItems;
    }
    
    // If user doesn't have AssignorId, only show Games-related items
    return navItems.filter(item => 
      item.label.toLowerCase().includes('game') || 
      item.href.toLowerCase().includes('game')
    );
  };

  const filteredNavItems = getFilteredNavItems();

  const handleAvatarClick = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = async () => {
    handleMenuClose();
    
    try {
      const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
      const token = localStorage.getItem('accessToken');
      
      if (token) {
        await fetch(`${API_BASE_URL}/auth/logout`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
      }
      
      // Clear local storage
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      
      // Wait 3 seconds then redirect
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (error) {
      console.error('Logout error:', error);
      // Clear storage and redirect anyway
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    }
  };

  return (
    <div className="layout-container">
      {/* Fixed Header Bar */}
      <header className={`layout-header ${isExpanded ? 'nav-expanded' : ''}`}>
        <img 
          src={rounderImage} 
          alt="" 
          className={`header-rounder ${isExpanded ? 'nav-expanded' : 'nav-collapsed'}`}
        />
        <div className="header-spacer"></div>
        <div 
          className="user-avatar" 
          style={{ backgroundColor: avatarColor }}
          onClick={handleAvatarClick}
          aria-controls={open ? 'user-menu' : undefined}
          aria-haspopup="true"
          aria-expanded={open ? 'true' : undefined}
        >
         {user ? getInitials() : 'NA'}
        </div>
        <Menu
          id="user-menu"
          anchorEl={anchorEl}
          open={open}
          onClose={handleMenuClose}
          MenuListProps={{
            'aria-labelledby': 'user-avatar',
          }}
          anchorOrigin={{
            vertical: 'bottom',
            horizontal: 'right',
          }}
          transformOrigin={{
            vertical: 'top',
            horizontal: 'right',
          }}
        >
          <MenuItem onClick={handleLogout}>Logout</MenuItem>
        </Menu>
      </header>

      {/* Collapsible Navigation Bar */}
      <nav className={`nav-sidebar ${isExpanded ? 'expanded' : 'collapsed'}`}>
        <div className="nav-header">
          {isExpanded ? (
            <img src={whistlersDarkLogo} alt="Whistlers" className="nav-logo-full" />
          ) : (
            <img src={whistlersIcon} alt="Whistlers" className="nav-logo" />
          )}
        </div>
        
        <ul className="nav-list">
          {filteredNavItems.map((item, index) => {
            const isActive = location.pathname === item.href;
            return (
              <li key={index} className="nav-item">
                <a 
                  href={item.href} 
                  className={`nav-link ${isActive ? 'active' : ''}`}
                  title={item.label}
                >
                  {item.icon && <span className="nav-icon">{item.icon}</span>}
                  {isExpanded && <span className="nav-label">{item.label}</span>}
                </a>
              </li>
            );
          })}
        </ul>

        {/* Toggle Button at Bottom */}
        <div className="nav-footer">
          <button 
            className={isExpanded ? "nav-collapse-btn" : "nav-expand-btn"}
            onClick={toggleNav}
            aria-label={isExpanded ? 'Collapse navigation' : 'Expand navigation'}
          >
            {isExpanded ? (
              <>
                <span className="nav-btn-text">Collapse</span>
              </>
            ) : (
              <>
                <span className="nav-btn-text"><ChevronRight />
                </span>
              </>
            )}
          </button>
        </div>
      </nav>

      {/* Main Content Area */}
      <main className={`main-content ${isExpanded ? 'nav-expanded' : 'nav-collapsed'}`}>
        {children}
      </main>
    </div>
  );
}

OneColumnWithNavLayout.propTypes = {
  children: PropTypes.node.isRequired,
  navItems: PropTypes.arrayOf(
    PropTypes.shape({
      label: PropTypes.string.isRequired,
      href: PropTypes.string.isRequired,
      icon: PropTypes.node,
    })
  ),
};

export default OneColumnWithNavLayout;
