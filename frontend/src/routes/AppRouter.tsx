import { lazy, Suspense } from 'react';
import { Route, Routes } from 'react-router-dom';
import Loading from '../components/ui/Loading';
import AdminLayout from '../layouts/AdminLayout';
import AuthLayout from '../layouts/AuthLayout';
import DashboardLayout from '../layouts/DashboardLayout';
import PublicLayout from '../layouts/PublicLayout';
import ProtectedRoute from './ProtectedRoute';
import PublicRoute from './PublicRoute';
import RoleBasedRoute from './RoleBasedRoute';

// Cada pagina en su propio chunk (code splitting, Frontend.md SS12 paso 10).
const AdminAuditLogs = lazy(() => import('../pages/AdminAuditLogs'));
const AdminCategories = lazy(() => import('../pages/AdminCategories'));
const AdminDashboard = lazy(() => import('../pages/AdminDashboard'));
const AdminObjects = lazy(() => import('../pages/AdminObjects'));
const AdminReports = lazy(() => import('../pages/AdminReports'));
const AdminUsers = lazy(() => import('../pages/AdminUsers'));
const Dashboard = lazy(() => import('../pages/Dashboard'));
const EditObject = lazy(() => import('../pages/EditObject'));
const ExchangeDetail = lazy(() => import('../pages/ExchangeDetail'));
const Exchanges = lazy(() => import('../pages/Exchanges'));
const Favorites = lazy(() => import('../pages/Favorites'));
const ForgotPassword = lazy(() => import('../pages/ForgotPassword'));
const Landing = lazy(() => import('../pages/Landing'));
const Login = lazy(() => import('../pages/Login'));
const ObjectDetail = lazy(() => import('../pages/ObjectDetail'));
const Profile = lazy(() => import('../pages/Profile'));
const PublishObject = lazy(() => import('../pages/PublishObject'));
const Register = lazy(() => import('../pages/Register'));
const ResetPassword = lazy(() => import('../pages/ResetPassword'));
const Search = lazy(() => import('../pages/Search'));

export default function AppRouter() {
  return (
    <Suspense fallback={<Loading lineas={4} className="p-lg" />}>
      <Routes>
        {/* Publicas */}
        <Route element={<PublicLayout />}>
          <Route path="/" element={<Landing />} />
          <Route path="/search" element={<Search />} />
          <Route path="/objects/:id" element={<ObjectDetail />} />
        </Route>

        {/* Auth */}
        <Route element={<AuthLayout />}>
          <Route
            path="/login"
            element={
              <PublicRoute>
                <Login />
              </PublicRoute>
            }
          />
          <Route
            path="/register"
            element={
              <PublicRoute>
                <Register />
              </PublicRoute>
            }
          />
          <Route
            path="/forgot-password"
            element={
              <PublicRoute>
                <ForgotPassword />
              </PublicRoute>
            }
          />
          <Route
            path="/reset-password"
            element={
              <PublicRoute>
                <ResetPassword />
              </PublicRoute>
            }
          />
        </Route>

        {/* Protegidas (usuario autenticado) */}
        <Route
          element={
            <ProtectedRoute>
              <DashboardLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/publish" element={<PublishObject />} />
          <Route path="/objects/:id/edit" element={<EditObject />} />
          <Route path="/exchanges" element={<Exchanges />} />
          <Route path="/exchanges/:id" element={<ExchangeDetail />} />
          <Route path="/favorites" element={<Favorites />} />
          <Route path="/profile" element={<Profile />} />
        </Route>

        {/* Admin (rol Moderador/Administrador) */}
        <Route
          element={
            <RoleBasedRoute roles={['Administrador', 'Moderador']}>
              <AdminLayout />
            </RoleBasedRoute>
          }
        >
          <Route path="/admin" element={<AdminDashboard />} />
          <Route path="/admin/objects" element={<AdminObjects />} />
          <Route path="/admin/reports" element={<AdminReports />} />
          <Route
            path="/admin/users"
            element={
              <RoleBasedRoute roles={['Administrador']}>
                <AdminUsers />
              </RoleBasedRoute>
            }
          />
          <Route
            path="/admin/audit-logs"
            element={
              <RoleBasedRoute roles={['Administrador']}>
                <AdminAuditLogs />
              </RoleBasedRoute>
            }
          />
          <Route
            path="/admin/categories"
            element={
              <RoleBasedRoute roles={['Administrador']}>
                <AdminCategories />
              </RoleBasedRoute>
            }
          />
        </Route>
      </Routes>
    </Suspense>
  );
}
