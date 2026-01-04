import ThreeColumnLayout from '../shared/ThreeColumnLayout';
import RegisterContent from './RegisterContent';

function Register() {
  return (
    <div style={{ height: '100vh', width: '100vw' }}>
      <ThreeColumnLayout centerContent={<RegisterContent />} />
    </div>
  );
}

export default Register;
