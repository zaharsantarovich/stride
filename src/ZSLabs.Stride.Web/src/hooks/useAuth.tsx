import { createContext, useContext, useEffect, useState } from 'react'
import type { PropsWithChildren } from 'react'
import { apiRequest, ApiError } from '../api/client'
import type { CurrentUser, LoginRequest } from '../api/contracts'

interface AuthContextValue {
  currentUser: CurrentUser | null
  isLoading: boolean
  errorMessage: string | null
  login: (request: LoginRequest) => Promise<CurrentUser>
  logout: () => Promise<void>
  refreshCurrentUser: () => Promise<void>
  clearError: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: PropsWithChildren) {
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  async function login(request: LoginRequest) {
    const user = await apiRequest<CurrentUser>('/auth/login', 'POST', request)
    setCurrentUser(user)
    setErrorMessage(null)
    return user
  }

  async function logout() {
    await apiRequest<void>('/auth/logout', 'POST')
    setCurrentUser(null)
    setErrorMessage(null)
  }

  async function refreshCurrentUser() {
    setIsLoading(true)

    try {
      const user = await apiRequest<CurrentUser>('/auth/me')
      setCurrentUser(user)
      setErrorMessage(null)
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        setCurrentUser(null)
        setErrorMessage(null)
      } else {
        setErrorMessage(error instanceof Error ? error.message : 'Unable to load session.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  function clearError() {
    setErrorMessage(null)
  }

  useEffect(() => {
    let isCancelled = false

    void apiRequest<CurrentUser>('/auth/me')
      .then((user) => {
        if (!isCancelled) {
          setCurrentUser(user)
          setErrorMessage(null)
        }
      })
      .catch((error: unknown) => {
        if (isCancelled) {
          return
        }

        if (error instanceof ApiError && error.status === 401) {
          setCurrentUser(null)
          setErrorMessage(null)
          return
        }

        setErrorMessage(error instanceof Error ? error.message : 'Unable to load session.')
      })
      .finally(() => {
        if (!isCancelled) {
          setIsLoading(false)
        }
      })

    return () => {
      isCancelled = true
    }
  }, [])

  return (
    <AuthContext.Provider
      value={{ currentUser, isLoading, errorMessage, login, logout, refreshCurrentUser, clearError }}
    >
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const context = useContext(AuthContext)

  if (context === null) {
    throw new Error('useAuth must be used within an AuthProvider.')
  }

  return context
}