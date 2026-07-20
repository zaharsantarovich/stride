import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { ApiError } from '../api/client'
import { useAuth } from '../hooks/useAuth'

export function SignInPage() {
  const navigate = useNavigate()
  const { login, clearError, errorMessage } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [localError, setLocalError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setLocalError(null)
    clearError()

    try {
      const currentUser = await login({ username, password })
      navigate(currentUser.role === 'Admin' ? '/users' : '/spaces', { replace: true })
    } catch (error) {
      setLocalError(error instanceof ApiError ? error.message : 'Unable to sign in.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-md items-center px-6 py-12">
      <section className="grid w-full gap-8 rounded-[2rem] border border-stride-border bg-stride-surface/90 p-8 shadow-board">
        <p className="text-sm font-semibold uppercase tracking-[0.3em] text-stride-accent">Stride</p>
        <form className="rounded-[1.5rem] border border-stride-border bg-stride-panel p-6" onSubmit={handleSubmit}>
          <h2 className="font-display text-2xl text-stride-ink">Sign in</h2>
          <div className="mt-6 space-y-4">
            <label className="block text-sm font-medium text-stride-ink">
              Username
              <input
                className="mt-2 w-full rounded-xl border border-stride-border bg-white px-4 py-3 text-base text-stride-ink outline-none transition focus:border-stride-accent"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
                autoComplete="username"
                required
              />
            </label>
            <label className="block text-sm font-medium text-stride-ink">
              Password
              <input
                className="mt-2 w-full rounded-xl border border-stride-border bg-white px-4 py-3 text-base text-stride-ink outline-none transition focus:border-stride-accent"
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                autoComplete="current-password"
                required
              />
            </label>
          </div>
          {(localError ?? errorMessage) !== null ? (
            <p className="mt-4 rounded-xl border border-stride-danger/30 bg-white/80 px-4 py-3 text-sm text-stride-danger">
              {localError ?? errorMessage}
            </p>
          ) : null}
          <button
            className="mt-6 inline-flex w-full items-center justify-center rounded-xl bg-stride-accent px-4 py-3 text-sm font-semibold text-white transition hover:bg-stride-accent-strong disabled:cursor-not-allowed disabled:opacity-70"
            disabled={isSubmitting}
            type="submit"
          >
            {isSubmitting ? 'Signing in...' : 'Sign in'}
          </button>
        </form>
      </section>
    </main>
  )
}