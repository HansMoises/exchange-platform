import { Outlet } from 'react-router-dom';
import Sidebar from '../components/layout/Sidebar';

export default function AdminLayout() {
  return (
    <div className="flex min-h-screen bg-bg-alt">
      <Sidebar />
      <main id="main-content" className="flex-1 p-lg">
        <Outlet />
      </main>
    </div>
  );
}
