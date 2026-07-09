import { Outlet } from 'react-router-dom';
import Footer from '../components/layout/Footer';
import Navbar from '../components/layout/Navbar';

export default function DashboardLayout() {
  return (
    <div className="flex min-h-screen flex-col bg-bg-alt">
      <Navbar />
      <main id="main-content" className="flex-1 p-lg">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
