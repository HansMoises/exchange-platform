import LoginForm from '../features/auth/components/LoginForm';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

export default function Login() {
  useDocumentTitle('Iniciar sesion');

  return (
    <div className="flex animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Iniciar sesion</h1>
      <LoginForm />
    </div>
  );
}
