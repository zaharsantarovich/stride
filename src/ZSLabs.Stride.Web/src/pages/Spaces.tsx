import { useNavigate } from 'react-router-dom'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { useSpaces } from '../hooks/useSpaces'
import { useAuth } from '../hooks/useAuth'

export function SpacesPage() {
  const navigate = useNavigate()
  const { currentUser, logout } = useAuth()
  const { spaces, isLoading, errorMessage, addSpace, saveSpace, removeSpace, refreshSpaces } = useSpaces()
  const [editingSpaceId, setEditingSpaceId] = useState<number | null>(null)
  const [createForm, setCreateForm] = useState({ key: '', name: '', isPublic: false })
  const [editForm, setEditForm] = useState({ name: '', isPublic: false })

  async function handleSignOut() {
    await logout()
    navigate('/sign-in', { replace: true })
  }

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    await addSpace(createForm)
    setCreateForm({ key: '', name: '', isPublic: false })
  }

  async function handleUpdate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (editingSpaceId === null) {
      return
    }

    await saveSpace(editingSpaceId, {
      name: editForm.name,
      isPublic: editForm.isPublic,
    })
    setEditingSpaceId(null)
  }

  function beginEdit(spaceId: number, name: string, isPublic: boolean) {
    setEditingSpaceId(spaceId)
    setEditForm({ name, isPublic })
  }

  return (
    <main className="mx-auto min-h-screen w-full max-w-6xl px-6 py-10">
      <section className="rounded-[2rem] border border-stride-border bg-stride-surface/95 p-8 shadow-board">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-stride-accent">Regular user area</p>
            <h1 className="mt-3 font-display text-4xl text-stride-ink">Spaces</h1>
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
          Create private or public spaces, revisit shared work, and edit public spaces collaboratively. Only the author can toggle the public flag.
        </p>
        {errorMessage !== null ? (
          <p className="mt-6 rounded-xl border border-stride-danger/30 bg-white px-4 py-3 text-sm text-stride-danger">{errorMessage}</p>
        ) : null}
        <div className="mt-8 grid gap-6 lg:grid-cols-[0.95fr_1.05fr]">
          <form className="rounded-[1.5rem] border border-stride-border bg-stride-panel p-6" onSubmit={handleCreate}>
            <h2 className="font-display text-2xl text-stride-ink">Create space</h2>
            <div className="mt-5 space-y-4">
              <input
                className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                placeholder="Unique key"
                required
                value={createForm.key}
                onChange={(event) => setCreateForm((current) => ({ ...current, key: event.target.value }))}
              />
              <input
                className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                placeholder="Display name"
                required
                value={createForm.name}
                onChange={(event) => setCreateForm((current) => ({ ...current, name: event.target.value }))}
              />
              <label className="flex items-center gap-3 text-sm font-medium text-stride-ink">
                <input
                  checked={createForm.isPublic}
                  onChange={(event) => setCreateForm((current) => ({ ...current, isPublic: event.target.checked }))}
                  type="checkbox"
                />
                Public space
              </label>
            </div>
            <button className="mt-5 rounded-xl bg-stride-accent px-4 py-3 text-sm font-semibold text-white" type="submit">
              Create space
            </button>
          </form>
          <div className="rounded-[1.5rem] border border-stride-border bg-white/80 p-6">
            <div className="flex items-center justify-between">
              <h2 className="font-display text-2xl text-stride-ink">Visible spaces</h2>
              <button className="text-sm font-semibold text-stride-accent" onClick={() => void refreshSpaces()} type="button">
                Refresh
              </button>
            </div>
            {isLoading ? <p className="mt-4 text-sm text-stride-muted">Loading spaces...</p> : null}
            <div className="mt-5 space-y-4">
              {spaces.map((space) => {
                const canToggleVisibility = currentUser?.id === space.authorId

                return (
                  <article key={space.id} className="rounded-2xl border border-stride-border bg-stride-surface px-4 py-4">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                      <div>
                        <p className="font-semibold text-stride-ink">{space.name}</p>
                        <p className="text-sm text-stride-muted">Key: {space.key}</p>
                        <p className="text-sm text-stride-muted">{space.isPublic ? 'Public' : 'Private'}</p>
                      </div>
                      <div className="flex gap-2">
                        <button
                          className="rounded-xl bg-stride-accent px-3 py-2 text-sm font-semibold text-white"
                          onClick={() => navigate(`/spaces/${space.id}`)}
                          type="button"
                        >
                          Open board
                        </button>
                        <button
                          className="rounded-xl border border-stride-border px-3 py-2 text-sm font-semibold text-stride-ink"
                          onClick={() => beginEdit(space.id, space.name, space.isPublic)}
                          type="button"
                        >
                          Edit
                        </button>
                        <button
                          className="rounded-xl border border-stride-danger/30 px-3 py-2 text-sm font-semibold text-stride-danger"
                          onClick={() => void removeSpace(space.id)}
                          type="button"
                        >
                          Delete
                        </button>
                      </div>
                    </div>
                    {editingSpaceId === space.id ? (
                      <form className="mt-4 grid gap-3" onSubmit={handleUpdate}>
                        <input
                          className="w-full rounded-xl border border-stride-border bg-white px-4 py-3"
                          required
                          value={editForm.name}
                          onChange={(event) => setEditForm((current) => ({ ...current, name: event.target.value }))}
                        />
                        <label className="flex items-center gap-3 text-sm font-medium text-stride-ink">
                          <input
                            checked={editForm.isPublic}
                            disabled={!canToggleVisibility}
                            onChange={(event) => setEditForm((current) => ({ ...current, isPublic: event.target.checked }))}
                            type="checkbox"
                          />
                          Public space
                        </label>
                        {!canToggleVisibility ? (
                          <p className="text-xs text-stride-muted">Only the author can change the public flag.</p>
                        ) : null}
                        <div className="flex gap-3">
                          <button className="rounded-xl bg-stride-accent px-4 py-3 text-sm font-semibold text-white" type="submit">
                            Save
                          </button>
                          <button
                            className="rounded-xl border border-stride-border px-4 py-3 text-sm font-semibold text-stride-ink"
                            onClick={() => setEditingSpaceId(null)}
                            type="button"
                          >
                            Cancel
                          </button>
                        </div>
                      </form>
                    ) : null}
                  </article>
                )
              })}
              {!isLoading && spaces.length === 0 ? <p className="text-sm text-stride-muted">No spaces yet.</p> : null}
            </div>
          </div>
        </div>
      </section>
    </main>
  )
}