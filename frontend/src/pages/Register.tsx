import RegisterForm from '../features/auth/components/RegisterForm';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

export default function Register() {
  useDocumentTitle('Crear cuenta');

  return (
    <div className="flex animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Crear cuenta</h1>
      <RegisterForm />
    </div>
  );
}
