import { useState } from 'react';
import PropTypes from 'prop-types';
import { Menu as MenuIcon, ChevronLeft } from '@mui/icons-material';
import './OneColumnWithNavLayout.css';

function OneColumnWithNavLayout({ children, navItems = [] }) {
  const [isExpanded, setIsExpanded] = useState(false);

  const toggleNav = () => {
    setIsExpanded(!isExpanded);
  };

  return (
    <div className="layout-container">
      {/* Collapsible Navigation Bar */}
      <nav className={`nav-sidebar ${isExpanded ? 'expanded' : 'collapsed'}`}>
        <div className="nav-header">
          <button 
            className="nav-toggle-btn" 
            onClick={toggleNav}
            aria-label={isExpanded ? 'Collapse navigation' : 'Expand navigation'}
          >
            {isExpanded ? <ChevronLeft /> : <MenuIcon />}
          </button>
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
