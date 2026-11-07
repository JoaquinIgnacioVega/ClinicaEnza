import { AuthProvider } from './context/AuthContext';
import { LoginPage } from './pages/LoginPage';

export default function App() {
  return (
    <AuthProvider>
      <main className="min-h-screen bg-white">
        <LoginPage />
      </main>
    </AuthProvider>
  );
}
