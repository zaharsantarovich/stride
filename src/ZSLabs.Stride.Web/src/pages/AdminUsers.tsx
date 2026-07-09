import { useNavigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { ApiError } from '../api/client'
import { createUser, getUsers, updateUser } from '../api/users'
import type { User } from '../api/contracts'
import { useAuth } from '../hooks/useAuth'

export function AdminUsersPage() {
  const navigate = useNavigate()
  const { currentUser, logout } = useAuth()
  const [users, setUsers] = useState<User[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [editingUserId, setEditingUserId] = useState<number | null>(null)
  const [createForm, setCreateForm] = useState({ username: '', password: '', email: '' })
  const [editForm, setEditForm] = useState({ username: '', password: '', email: '' })

  useEffect(() => {
    void loadUsers()
  }, [])

  async function loadUsers() {
    setIsLoading(true)

    try {
      setUsers(await getUsers())
      setErrorMessage(null)
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to load users.')
    } finally {
      setIsLoading(false)
    }
  }

  async function handleSignOut() {
    await logout()
    navigate('/sign-in', { replace: true })
  }

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    try {
      const created = await createUser({
        username: createForm.username,
        password: createForm.password,
        email: createForm.email || null,
      })
      setUsers((current) => [...current, created].sort((left, right) => left.username.localeCompare(right.username)))
      setCreateForm({ username: '', password: '', email: '' })
      setErrorMessage(null)
    } catch (error) {
      setErrorMessage(error instanceof ApiError ? error.message : 'Unable to create user.')
    }
  }

  async function handleUpdate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (editingUserId === null) {
      return
    }

    try {
      const updated = await updateUser(editingUserId, {
        username: editForm.username,
        password: editForm.password || null,
        email: editForm.email || null,
      })

      setUsers((current) =>
        current
          .map((user) => (user.id === updated.id ? updated : user))
          .sort((left, right) => left.username.localeCompare(right.username)),
      )
      setEditingUserId(null)
      setEditForm({ username: '', password: '', email: '' })
      setErrorMessage(null)
    } catch (error) {
      setErrorMessage(error instanceof ApiError ? error.message : 'Unable to update user.')
    }
  }

  function beginEdit(user: User) {
    setEditingUserId(user.id)
    setEditForm({ username: user.username, password: '', email: user.email ?? '' })
  }

  return (
    <main className="mx-auto min-h-screen w-full max-w-6xl px-6 py-10">
      <section className="rounded-[2rem] border border-stride-border bg-stride-surface/95 p-8 shadow-board">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-stride-accent">Admin area</p>
            <h1 className="mt-3 font-display text-4xl text-stride-ink">User management</h1>
          </div>
          <button
            className="inline-flex items-center justify-center rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink"
            onClick={() => void handleSignOut()}
            type="button"
          >
            Sign out {currentUser?.username ? `(${currentUser.username})` : ''}
          </button>
        </div>
        <p className="mt-4 max-w-2xl text-base leading-7 text-stride-muted">
          Only the seeded admin can reach this area. Create family members here, update their details, and keep duplicate usernames out of the system.
        </p>
        {errorMessage !== null ? (
          <p className="mt-6 rounded-xl border border-stride-danger/30 bg-white px-4 py-3 text-sm text-stride-danger">{errorMessage}</p>
        ) : null}
        <div className="mt-8 grid gap-6 lg:grid-cols-[0.95fr_1.05fr]">
          <form className="rounded-[1.5rem] border border-stride-border bg-stride-panel p-6" onSubmit={handleCreate}>
            <h2 className="font-display text-2xl text-stride-ink">Create user</h2>
            <div className="mt-5 space-y-4">
              <input
                className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                placeholder="Username"
                required
                value={createForm.username}
                onChange={(event) => setCreateForm((current) => ({ ...current, username: event.target.value }))}
              />
              <input
                className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                placeholder="Password"
                required
                type="password"
                value={createForm.password}
                onChange={(event) => setCreateForm((current) => ({ ...current, password: event.target.value }))}
              />
              <input
                className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                placeholder="Email (optional)"
                type="email"
                value={createForm.email}
                onChange={(event) => setCreateForm((current) => ({ ...current, email: event.target.value }))}
              />
            </div>
            <button className="mt-5 rounded-xl bg-stride-accent px-4 py-3 text-sm font-semibold text-white" type="submit">
              Create regular user
            </button>
          </form>
          <div className="rounded-[1.5rem] border border-stride-border bg-white/80 p-6">
            <div className="flex items-center justify-between">
              <h2 className="font-display text-2xl text-stride-ink">Existing users</h2>
              <button className="text-sm font-semibold text-stride-accent" onClick={() => void loadUsers()} type="button">
                Refresh
              </button>
            </div>
            {isLoading ? <p className="mt-4 text-sm text-stride-muted">Loading users...</p> : null}
            <div className="mt-5 space-y-4">
              {users.map((user) => (
                <article key={user.id} className="rounded-2xl border border-stride-border bg-stride-surface px-4 py-4">
                  <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                    <div>
                      <p className="font-semibold text-stride-ink">{user.username}</p>
                      <p className="text-sm text-stride-muted">{user.email ?? 'No email'}</p>
                    </div>
                    <button
                      className="rounded-xl border border-stride-border px-3 py-2 text-sm font-semibold text-stride-ink"
                      onClick={() => beginEdit(user)}
                      type="button"
                    >
                      Edit
                    </button>
                  </div>
                  {editingUserId === user.id ? (
                    <form className="mt-4 grid gap-3" onSubmit={handleUpdate}>
                      <input
                        className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                        value={editForm.username}
                        onChange={(event) => setEditForm((current) => ({ ...current, username: event.target.value }))}
                        required
                      />
                      <input
                        className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                        type="password"
                        placeholder="New password (optional)"
                        value={editForm.password}
                        onChange={(event) => setEditForm((current) => ({ ...current, password: event.target.value }))}
                      />
                      <input
                        className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                        type="email"
                        value={editForm.email}
                        onChange={(event) => setEditForm((current) => ({ ...current, email: event.target.value }))}
                      />
                      <div className="flex gap-3">
                        <button className="rounded-xl bg-stride-accent px-4 py-3 text-sm font-semibold text-white" type="submit">
                          Save
                        </button>
                        <button
                          className="rounded-xl border border-stride-border px-4 py-3 text-sm font-semibold text-stride-ink"
                          onClick={() => setEditingUserId(null)}
                          type="button"
                        >
                          Cancel
                        </button>
                      </div>
                    </form>
                  ) : null}
                </article>
              ))}
              {!isLoading && users.length === 0 ? <p className="text-sm text-stride-muted">No regular users yet.</p> : null}
            </div>
          </div>
        </div>
      </section>
    </main>
  )
}