import ResetPasswordForm from '../features/auth/components/ResetPasswordForm';
import { useDocumentTitle } from '../hooks/useDocumentTitle';

export default function ResetPassword() {
  useDocumentTitle('Restablecer contrasena');

  return (
    <div className="flex animate-fade-in flex-col gap-lg">
      <h1 className="font-display text-h2 font-bold text-primary">Restablecer contrasena</h1>
      <ResetPasswordForm />
    </div>
  );
}
