import ThreeColumnLayout from '../shared/ThreeColumnLayout';
import LoginContent from './LoginContent';
import { Box } from '@mui/material';

function Login() {
  return (
    <Box sx={{ height: '100vh', overflow: 'hidden' }}>
      <ThreeColumnLayout centerContent={<LoginContent />} />
    </Box>
  );
}

export default Login;
