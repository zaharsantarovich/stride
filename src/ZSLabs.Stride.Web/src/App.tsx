import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AuthProvider } from './hooks/useAuth'
import { AdminUsersPage } from './pages/AdminUsers'
import { SignInPage } from './pages/SignIn'
import { SpaceBoardPage } from './pages/SpaceBoard'
import { SpacesPage } from './pages/Spaces'

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/sign-in" element={<SignInPage />} />
          <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
            <Route path="/users" element={<AdminUsersPage />} />
          </Route>
          <Route element={<ProtectedRoute allowedRoles={['Regular']} />}>
            <Route path="/spaces" element={<SpacesPage />} />
            <Route path="/spaces/:spaceId" element={<SpaceBoardPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/spaces" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
