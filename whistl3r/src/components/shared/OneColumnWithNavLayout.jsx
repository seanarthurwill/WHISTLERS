import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { ChevronLeft, ChevronRight } from '@mui/icons-material';
import whistlersIcon from '../../assets/images/WHISTLERS_ICON_dark.png';
import whistlersDarkLogo from '../../assets/images/WHISTLERS_LOGO_DARK.png';
import './OneColumnWithNavLayout.css';

function OneColumnWithNavLayout({ children, navItems = [] }) {
  const [isExpanded, setIsExpanded] = useState(true);
  const [user, setUser] = useState(null);
  const [avatarColor, setAvatarColor] = useState('#667eea');

  useEffect(() => {
    // Get user from localStorage
    const userStr = localStorage.getItem('user');
    console.log('User string from localStorage:', userStr);
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

  return (
    <div className="layout-container">
      {/* Fixed Header Bar */}
      <header className="layout-header">
        <div className="header-spacer"></div>
        <div className="user-avatar" style={{ backgroundColor: avatarColor }}>
         {user ? getInitials() : 'NA'}
        </div>
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
          {navItems.map((item, index) => (
            <li key={index} className="nav-item">
              <a 
                href={item.href} 
                className="nav-link"
                title={item.label}
              >
                {item.icon && <span className="nav-icon">{item.icon}</span>}
                {isExpanded && <span className="nav-label">{item.label}</span>}
              </a>
            </li>
          ))}
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
