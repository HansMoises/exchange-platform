import { Link, Outlet } from 'react-router-dom';

export default function AuthLayout() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-lg bg-bg-alt p-lg">
      <Link to="/" className="text-h3 text-primary">
        Intercambio Ayacucho
      </Link>
      <main id="main-content" className="w-full max-w-md">
        <Outlet />
      </main>
    </div>
  );
}
