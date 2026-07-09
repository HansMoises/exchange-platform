import ForgotPasswordForm from '../features/auth/components/ForgotPasswordForm';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

export default function ForgotPassword() {
  useDocumentTitle('Recuperar contrasena');

  return (
    <div className="flex animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Recuperar contrasena</h1>
      <ForgotPasswordForm />
    </div>
  );
}
