import { useState } from 'react';
import PropTypes from 'prop-types';
import { ChevronLeft, ChevronRight } from '@mui/icons-material';
import whistlersIcon from '../../assets/images/WHISTLERS_ICON_dark.png';
import whistlersDarkLogo from '../../assets/images/WHISTLERS_LOGO_DARK.png';
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
