import { Outlet } from 'react-router-dom';
import OneColumnWithNavLayout from '../shared/OneColumnWithNavLayout';
import DashboardIcon from '@mui/icons-material/Dashboard';
import SportsIcon from '@mui/icons-material/Sports';
import PeopleIcon from '@mui/icons-material/People';
import BusinessIcon from '@mui/icons-material/Business';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import AssignmentIcon from '@mui/icons-material/Assignment';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import SettingsIcon from '@mui/icons-material/Settings';
import OpenGames from '../games/OpenGames';

function Dashboard() {
  const navItems = [
    { label: 'Games', href: '/dashboard/games', icon: <SportsIcon /> },    
    { label: 'Assignments', href: '/dashboard/assignments', icon: <AssignmentIcon /> },
    { label: 'Officials', href: '/dashboard/officials', icon: <PeopleIcon /> },
    { label: 'Administration', href: '/dashboard/administration', icon: <BusinessIcon /> },
    { label: 'Settings', href: '/dashboard/settings', icon: <SettingsIcon /> },
  ];

  return (
    <OneColumnWithNavLayout navItems={navItems}> 
      <Outlet />
    </OneColumnWithNavLayout>
  );
}

export default Dashboard;
