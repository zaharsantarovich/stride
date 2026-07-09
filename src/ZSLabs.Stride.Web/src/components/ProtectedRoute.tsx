import { Navigate, Outlet } from 'react-router-dom'
import type { UserRole } from '../api/contracts'
import { useAuth } from '../hooks/useAuth'

interface ProtectedRouteProps {
  allowedRoles: UserRole[]
}

export function ProtectedRoute({ allowedRoles }: ProtectedRouteProps) {
  const { currentUser, isLoading } = useAuth()

  if (isLoading) {
    return <div className="flex min-h-screen items-center justify-center text-stride-muted">Loading...</div>
  }

  if (currentUser === null) {
    return <Navigate to="/sign-in" replace />
  }

  if (!allowedRoles.includes(currentUser.role)) {
    return <Navigate to={currentUser.role === 'Admin' ? '/users' : '/spaces'} replace />
  }

  return <Outlet />
}