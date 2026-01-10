import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { LoadingProvider } from './contexts/LoadingContext';
import LoadingSpinner from './components/shared/LoadingSpinner';
import LandingPage from './components/LandingPage';
import Register from './components/registration/Register';
import Login from './components/login/Login';
import ForgotPassword from './components/login/ForgotPassword';
import ResetPassword from './components/login/ResetPassword';
import Dashboard from './components/dashboard/Dashboard';
import OpenGames from './components/games/OpenGames';
import DashboardSummary from './components/dashboard/DashboardSummary';
import './App.css';

function App() {
  return (
    <LoadingProvider>
      <Router>
        <div className="App">
          <LoadingSpinner />
          <Routes>
            <Route path="/" element={<LandingPage />} />
            <Route path="/register" element={<Register />} />
            <Route path="/login" element={<Login />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password" element={<ResetPassword />} />
            <Route path="/dashboard" element={<Dashboard />}>
              <Route index element={<DashboardSummary />} />
              <Route path="games" element={<OpenGames />} />
            </Route>
          </Routes>
        </div>
      </Router>
    </LoadingProvider>
  );
}

export default App;

