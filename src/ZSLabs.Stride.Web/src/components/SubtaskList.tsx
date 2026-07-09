import { useState } from 'react'
import type { FormEvent } from 'react'
import type { CreateSubtaskRequest, Subtask, SubtaskStatus, UpdateSubtaskRequest } from '../api/contracts'
import { CommentList } from './CommentList'
import { formatLocalDateTime } from '../utils/formatLocalDateTime'

interface SubtaskListProps {
  subtasks: Subtask[]
  currentUserId?: number
  onAdd: (request: CreateSubtaskRequest) => Promise<void>
  onUpdate: (subtaskId: number, request: UpdateSubtaskRequest) => Promise<void>
  onDelete: (subtaskId: number) => Promise<void>
  onAddComment: (subtaskId: number, content: string) => Promise<void>
  onUpdateComment: (commentId: number, content: string) => Promise<void>
  onDeleteComment: (commentId: number) => Promise<void>
}

const statusOptions: SubtaskStatus[] = ['Todo', 'InProgress', 'Done']

export function SubtaskList({
  subtasks,
  currentUserId,
  onAdd,
  onUpdate,
  onDelete,
  onAddComment,
  onUpdateComment,
  onDeleteComment,
}: SubtaskListProps) {
  const [createForm, setCreateForm] = useState<CreateSubtaskRequest>({ title: '', description: '', status: 'Todo' })
  const [editingSubtaskId, setEditingSubtaskId] = useState<number | null>(null)
  const [editForm, setEditForm] = useState<UpdateSubtaskRequest>({ title: '', description: '', status: 'Todo' })

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (createForm.title.trim().length === 0) {
      return
    }

    await onAdd({ ...createForm, title: createForm.title.trim(), description: createForm.description?.trim() || null })
    setCreateForm({ title: '', description: '', status: 'Todo' })
  }

  async function handleUpdate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (editingSubtaskId === null) {
      return
    }

    await onUpdate(editingSubtaskId, {
      ...editForm,
      title: editForm.title?.trim(),
      description: editForm.description?.trim() || null,
    })
    setEditingSubtaskId(null)
  }

  return (
    <section className="space-y-3 rounded-2xl border border-stride-border/70 bg-stride-panel/70 p-4">
      <div className="flex items-center justify-between gap-3">
        <h4 className="font-semibold text-stride-ink">Subtasks</h4>
        <span className="text-xs uppercase tracking-[0.2em] text-stride-muted">{subtasks.length}</span>
      </div>
      <form className="grid gap-2 sm:grid-cols-[1.2fr_1fr_auto]" onSubmit={handleCreate}>
        <input
          className="rounded-xl border border-stride-border bg-white px-3 py-2"
          placeholder="Add subtask title"
          value={createForm.title}
          onChange={(event) => setCreateForm((current) => ({ ...current, title: event.target.value }))}
        />
        <select
          className="rounded-xl border border-stride-border bg-white px-3 py-2"
          value={createForm.status ?? 'Todo'}
          onChange={(event) => setCreateForm((current) => ({ ...current, status: event.target.value as SubtaskStatus }))}
        >
          {statusOptions.map((status) => (
            <option key={status} value={status}>
              {status}
            </option>
          ))}
        </select>
        <button className="rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white" type="submit">
          Add
        </button>
        <textarea
          className="min-h-20 rounded-xl border border-stride-border bg-white px-3 py-2 sm:col-span-3"
          placeholder="Optional subtask description"
          value={createForm.description ?? ''}
          onChange={(event) => setCreateForm((current) => ({ ...current, description: event.target.value }))}
        />
      </form>
      <div className="space-y-3">
        {subtasks.map((subtask) => (
          <article key={subtask.id} className="rounded-xl border border-stride-border bg-white px-4 py-4">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <p className="font-semibold text-stride-ink">{subtask.title}</p>
                <p className="text-sm text-stride-muted">{subtask.status} • {formatLocalDateTime(subtask.updatedAt ?? subtask.createdAt)}</p>
                {subtask.description ? <p className="mt-2 text-sm leading-6 text-stride-ink">{subtask.description}</p> : null}
              </div>
              <div className="flex gap-2 text-sm">
                <button
                  className="font-semibold text-stride-accent"
                  onClick={() => {
                    setEditingSubtaskId(subtask.id)
                    setEditForm({ title: subtask.title, description: subtask.description, status: subtask.status })
                  }}
                  type="button"
                >
                  Edit
                </button>
                <button className="font-semibold text-stride-danger" onClick={() => void onDelete(subtask.id)} type="button">
                  Delete
                </button>
              </div>
            </div>
            {editingSubtaskId === subtask.id ? (
              <form className="mt-4 grid gap-2" onSubmit={handleUpdate}>
                <input
                  className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2"
                  value={editForm.title ?? ''}
                  onChange={(event) => setEditForm((current) => ({ ...current, title: event.target.value }))}
                />
                <select
                  className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2"
                  value={editForm.status ?? 'Todo'}
                  onChange={(event) => setEditForm((current) => ({ ...current, status: event.target.value as SubtaskStatus }))}
                >
                  {statusOptions.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
                <textarea
                  className="min-h-20 rounded-xl border border-stride-border bg-stride-surface px-3 py-2"
                  value={editForm.description ?? ''}
                  onChange={(event) => setEditForm((current) => ({ ...current, description: event.target.value }))}
                />
                <div className="flex gap-2">
                  <button className="rounded-lg bg-stride-accent px-3 py-2 text-xs font-semibold text-white" type="submit">
                    Save
                  </button>
                  <button
                    className="rounded-lg border border-stride-border px-3 py-2 text-xs font-semibold text-stride-ink"
                    onClick={() => setEditingSubtaskId(null)}
                    type="button"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            ) : null}
            <div className="mt-4">
              <CommentList
                comments={subtask.comments}
                currentUserId={currentUserId}
                onAdd={(content) => onAddComment(subtask.id, content)}
                onUpdate={onUpdateComment}
                onDelete={onDeleteComment}
              />
            </div>
          </article>
        ))}
        {subtasks.length === 0 ? <p className="text-sm text-stride-muted">No subtasks yet.</p> : null}
      </div>
    </section>
  )
}