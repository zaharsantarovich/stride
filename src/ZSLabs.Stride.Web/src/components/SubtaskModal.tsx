import { useId, useState } from 'react'
import type { FormEvent } from 'react'
import type { CreateSubtaskRequest, RegularUserLookup, Space, Subtask, SubtaskStatus, UpdateSubtaskRequest } from '../api/contracts'
import { useSubtask } from '../hooks/useSubtask'
import { fromDateInput, toDateInput } from '../utils/dateOnly'
import { formatLocalDateTime } from '../utils/formatLocalDateTime'
import { ConfirmationDialog } from './ConfirmationDialog'
import { ModalShell } from './ModalShell'

interface SubtaskModalProps {
  subtaskId: number | null
  parentTaskId: number
  space: Space
  currentUser: RegularUserLookup
  regularUsers: RegularUserLookup[]
  onDismiss: () => void
  onDeleted: () => void
  onOpenParent: () => void
  onRefreshTasks: () => Promise<unknown>
}

interface SubtaskEditorProps extends Omit<SubtaskModalProps, 'subtaskId' | 'parentTaskId' | 'onRefreshTasks'> {
  subtask: Subtask
  errorMessage: string | null
  isSaving: boolean
  isDeleting: boolean
  isMutatingComment: boolean
  onSave: (request: UpdateSubtaskRequest) => Promise<unknown>
  onDelete: () => Promise<void>
  onAddComment: (content: string) => Promise<unknown>
  onEditComment: (commentId: number, content: string) => Promise<unknown>
  onDeleteComment: (commentId: number) => Promise<void>
}

const subtaskStatuses: SubtaskStatus[] = ['Todo', 'InProgress', 'Done']

interface SubtaskCreateEditorProps extends Omit<SubtaskModalProps, 'subtaskId' | 'onDeleted' | 'onOpenParent' | 'onRefreshTasks'> {
  errorMessage: string | null
  isSaving: boolean
  onCreate: (request: CreateSubtaskRequest) => Promise<unknown>
}

function SubtaskCreateEditor({ parentTaskId, space, currentUser, regularUsers, errorMessage, isSaving, onDismiss, onCreate }: SubtaskCreateEditorProps) {
  const titleId = useId()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [status, setStatus] = useState<SubtaskStatus>('Todo')
  const [assigneeId, setAssigneeId] = useState<number | null>(null)
  const [dueDate, setDueDate] = useState('')
  const assigneeOptions = space.isPublic ? regularUsers : [currentUser]

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    try {
      await onCreate({
        title: title.trim(),
        description: description.trim() || null,
        status,
        assigneeId,
        dueDate: fromDateInput(dueDate),
      })
    } catch {
      // The hook keeps this draft and exposes the recoverable error.
    }
  }

  return (
    <ModalShell
      labelledBy={titleId}
      onDismiss={onDismiss}
      onSubmit={(event) => void handleSubmit(event)}
      header={(
        <>
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.22em] text-stride-accent">Create subtask · Parent task {parentTaskId}</p>
            <h2 id={titleId} className="mt-2 font-display text-3xl text-stride-ink">New subtask</h2>
          </div>
          <div className="flex gap-2">
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink" type="button" onClick={onDismiss}>Cancel</button>
            <button className="rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white disabled:opacity-60" disabled={isSaving} type="submit">{isSaving ? 'Saving...' : 'Create subtask'}</button>
          </div>
        </>
      )}
    >
          {errorMessage ? <p className="mb-5 rounded-xl border border-stride-danger/30 bg-stride-surface px-4 py-3 text-sm text-stride-danger">{errorMessage}</p> : null}
          <div className="grid gap-4 md:grid-cols-2">
            <label className="grid gap-2 text-sm font-semibold text-stride-ink md:col-span-2">Title<input className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" required value={title} onChange={(event) => setTitle(event.target.value)} /></label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">Status<select className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={status} onChange={(event) => setStatus(event.target.value as SubtaskStatus)}>{subtaskStatuses.map((option) => <option key={option} value={option}>{option}</option>)}</select></label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">Assignee<select className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={assigneeId ?? ''} onChange={(event) => setAssigneeId(event.target.value === '' ? null : Number(event.target.value))}><option value="">Unassigned</option>{assigneeOptions.map((user) => <option key={user.id} value={user.id}>{user.username}</option>)}</select></label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">Due date<input className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" type="date" value={dueDate} onChange={(event) => setDueDate(event.target.value)} /></label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink md:col-span-2">Description<textarea className="min-h-28 rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={description} onChange={(event) => setDescription(event.target.value)} /></label>
          </div>
          <section className="mt-6 border-t border-stride-border pt-5">
            <h3 className="font-display text-2xl text-stride-ink">Comments</h3>
            <p className="mt-4 text-sm text-stride-muted">No comments yet.</p>
            <label className="mt-4 grid gap-2 text-sm font-semibold text-stride-ink">New comment<textarea className="min-h-24 rounded-xl border border-stride-border bg-stride-surface px-4 py-3 font-normal" disabled value="" readOnly /></label>
            <button className="mt-3 rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white opacity-60" disabled type="button">Add comment</button>
          </section>
    </ModalShell>
  )
}

function SubtaskEditor({ subtask, space, currentUser, regularUsers, errorMessage, isSaving, isDeleting, isMutatingComment, onDismiss, onDeleted, onOpenParent, onSave, onDelete, onAddComment, onEditComment, onDeleteComment }: SubtaskEditorProps) {
  const titleId = useId()
  const [title, setTitle] = useState(subtask.title)
  const [description, setDescription] = useState(subtask.description ?? '')
  const [status, setStatus] = useState<SubtaskStatus>(subtask.status)
  const [assigneeId, setAssigneeId] = useState<number | null>(subtask.assigneeId)
  const [dueDate, setDueDate] = useState(toDateInput(subtask.dueDate))
  const [newComment, setNewComment] = useState('')
  const [editingCommentId, setEditingCommentId] = useState<number | null>(null)
  const [editingComment, setEditingComment] = useState('')
  const [isDeleteConfirmationOpen, setIsDeleteConfirmationOpen] = useState(false)
  const assigneeOptions = space.isPublic ? regularUsers : [currentUser]

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    try {
      await onSave({
        title: title.trim(),
        description: description.trim() || null,
        status,
        assigneeId,
        dueDate: fromDateInput(dueDate),
      })
    } catch {
      // The hook keeps the error visible while this draft remains intact.
    }
  }

  async function handleDelete() {
    try {
      await onDelete()
      setIsDeleteConfirmationOpen(false)
      onDeleted()
    } catch {
      // The hook displays the recoverable error in this modal.
    }
  }

  async function handleAddComment() {
    const content = newComment.trim()

    if (content.length === 0) {
      return
    }

    try {
      await onAddComment(content)
      setNewComment('')
    } catch {
      // Keep the draft available for correction or retry.
    }
  }

  async function handleEditComment(commentId: number) {
    const content = editingComment.trim()

    if (content.length === 0) {
      return
    }

    try {
      await onEditComment(commentId, content)
      setEditingCommentId(null)
      setEditingComment('')
    } catch {
      // Keep the edit available for correction or retry.
    }
  }

  async function handleDeleteComment(commentId: number) {
    if (!window.confirm('Delete this comment?')) {
      return
    }

    try {
      await onDeleteComment(commentId)
    } catch {
      // The hook displays the recoverable error.
    }
  }

  return (
    <ModalShell
      labelledBy={titleId}
      onDismiss={onDismiss}
      onSubmit={(event) => void handleSubmit(event)}
      header={(
        <>
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.22em] text-stride-accent">Edit subtask</p>
            <h2 id={titleId} className="mt-2 font-display text-3xl text-stride-ink">{subtask.title}</h2>
          </div>
          <div className="flex flex-wrap gap-2">
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-accent" type="button" onClick={onOpenParent}>Open parent task</button>
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-danger disabled:opacity-60" disabled={isDeleting || isSaving} type="button" onClick={() => setIsDeleteConfirmationOpen(true)}>
              {isDeleting ? 'Deleting...' : 'Delete'}
            </button>
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink" type="button" onClick={onDismiss}>Cancel</button>
            <button className="rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white disabled:opacity-60" disabled={isSaving || isDeleting} type="submit">
              {isSaving ? 'Saving...' : 'Save changes'}
            </button>
          </div>
        </>
      )}
    >
          {errorMessage ? <p className="mb-5 rounded-xl border border-stride-danger/30 bg-stride-surface px-4 py-3 text-sm text-stride-danger">{errorMessage}</p> : null}
          <div className="grid gap-4 md:grid-cols-2">
            <label className="grid gap-2 text-sm font-semibold text-stride-ink md:col-span-2">
              Title
              <input className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" required value={title} onChange={(event) => setTitle(event.target.value)} />
            </label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">
              Status
              <select className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={status} onChange={(event) => setStatus(event.target.value as SubtaskStatus)}>
                {subtaskStatuses.map((option) => <option key={option} value={option}>{option}</option>)}
              </select>
            </label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">
              Assignee
              <select className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={assigneeId ?? ''} onChange={(event) => setAssigneeId(event.target.value === '' ? null : Number(event.target.value))}>
                <option value="">Unassigned</option>
                {assigneeOptions.map((user) => <option key={user.id} value={user.id}>{user.username}</option>)}
              </select>
            </label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">
              Due date
              <input className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" type="date" value={dueDate} onChange={(event) => setDueDate(event.target.value)} />
            </label>
            <div className="grid gap-2 text-sm font-semibold text-stride-ink">
              Author
              <p className="rounded-xl border border-stride-border bg-stride-surface px-4 py-3 font-normal">{subtask.authorUsername}</p>
            </div>
            <div className="grid gap-2 text-sm font-semibold text-stride-ink">
              Created at
              <p className="rounded-xl border border-stride-border bg-stride-surface px-4 py-3 font-normal">{formatLocalDateTime(subtask.createdAt)}</p>
            </div>
            <div className="grid gap-2 text-sm font-semibold text-stride-ink">
              Updated at
              <p className="rounded-xl border border-stride-border bg-stride-surface px-4 py-3 font-normal">{subtask.updatedAt ? formatLocalDateTime(subtask.updatedAt) : 'Not updated'}</p>
            </div>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink md:col-span-2">
              Description
              <textarea className="min-h-28 rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={description} onChange={(event) => setDescription(event.target.value)} />
            </label>
          </div>
          <section className="mt-6 border-t border-stride-border pt-5">
            <h3 className="font-display text-2xl text-stride-ink">Comments</h3>
            <div className="mt-4 grid gap-3">
              {subtask.comments.map((comment) => (
                <article key={comment.id} className="rounded-xl border border-stride-border bg-stride-surface p-4">
                  {editingCommentId === comment.id ? (
                    <textarea className="min-h-24 w-full rounded-xl border border-stride-border bg-white px-4 py-3 text-sm text-stride-ink" value={editingComment} onChange={(event) => setEditingComment(event.target.value)} />
                  ) : <p className="whitespace-pre-wrap text-sm text-stride-ink">{comment.content}</p>}
                  <p className="mt-3 text-xs text-stride-muted">
                    {comment.authorUsername} · Created {formatLocalDateTime(comment.createdAt)}
                    {comment.updatedAt ? ` · Updated ${formatLocalDateTime(comment.updatedAt)}` : null}
                  </p>
                  {comment.authorId === currentUser.id ? (
                    <div className="mt-3 flex flex-wrap gap-2">
                      {editingCommentId === comment.id ? (
                        <>
                          <button className="rounded-lg bg-stride-accent px-3 py-2 text-xs font-semibold text-white disabled:opacity-60" disabled={isMutatingComment} type="button" onClick={() => void handleEditComment(comment.id)}>Save comment</button>
                          <button className="rounded-lg border border-stride-border bg-white px-3 py-2 text-xs font-semibold text-stride-ink" type="button" onClick={() => { setEditingCommentId(null); setEditingComment('') }}>Cancel edit</button>
                        </>
                      ) : (
                        <>
                          <button className="rounded-lg border border-stride-border bg-white px-3 py-2 text-xs font-semibold text-stride-ink" type="button" onClick={() => { setEditingCommentId(comment.id); setEditingComment(comment.content) }}>Edit</button>
                          <button className="rounded-lg border border-stride-border bg-white px-3 py-2 text-xs font-semibold text-stride-danger disabled:opacity-60" disabled={isMutatingComment} type="button" onClick={() => void handleDeleteComment(comment.id)}>Delete</button>
                        </>
                      )}
                    </div>
                  ) : null}
                </article>
              ))}
              {subtask.comments.length === 0 ? <p className="text-sm text-stride-muted">No comments yet.</p> : null}
            </div>
            <label className="mt-5 grid gap-2 text-sm font-semibold text-stride-ink">
              New comment
              <textarea className="min-h-24 rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={newComment} onChange={(event) => setNewComment(event.target.value)} />
            </label>
            <button className="mt-3 rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white disabled:opacity-60" disabled={isMutatingComment || newComment.trim().length === 0} type="button" onClick={() => void handleAddComment()}>
              {isMutatingComment ? 'Saving...' : 'Add comment'}
            </button>
          </section>
          {isDeleteConfirmationOpen ? (
            <ConfirmationDialog
              confirmLabel="Delete subtask"
              isConfirming={isDeleting}
              message={`This will permanently delete “${subtask.title}” and all of its comments. This action cannot be undone.`}
              title="Delete this subtask?"
              onCancel={() => setIsDeleteConfirmationOpen(false)}
              onConfirm={() => void handleDelete()}
            />
          ) : null}
    </ModalShell>
  )
}

export function SubtaskModal({ subtaskId, parentTaskId, onRefreshTasks, onDismiss, ...props }: SubtaskModalProps) {
  const { subtask, isLoading, isSaving, isDeleting, isMutatingComment, errorMessage, reload, create, save, remove, addComment, editComment, removeComment } = useSubtask(subtaskId, parentTaskId, onRefreshTasks)

  if (subtaskId === null && subtask === null) {
    return <SubtaskCreateEditor parentTaskId={parentTaskId} space={props.space} currentUser={props.currentUser} regularUsers={props.regularUsers} errorMessage={errorMessage} isSaving={isSaving} onDismiss={onDismiss} onCreate={create} />
  }

  if (isLoading || subtask === null) {
    return (
      <ModalShell
        onDismiss={onDismiss}
        header={(
          <>
            <h2 className="font-display text-3xl text-stride-ink">Subtask</h2>
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink" type="button" onClick={onDismiss}>Close</button>
          </>
        )}
      >
            {isLoading ? <p className="text-sm text-stride-muted">Loading subtask...</p> : null}
            {!isLoading && errorMessage ? (
              <div className="grid justify-items-start gap-4">
                <p className="rounded-xl border border-stride-danger/30 bg-stride-surface px-4 py-3 text-sm text-stride-danger">{errorMessage}</p>
                <button className="rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white" type="button" onClick={() => void reload()}>Retry</button>
              </div>
            ) : null}
      </ModalShell>
    )
  }

  return <SubtaskEditor key={`${subtask.id}-${subtask.updatedAt ?? 'new'}`} subtask={subtask} errorMessage={errorMessage} isSaving={isSaving} isDeleting={isDeleting} isMutatingComment={isMutatingComment} onDismiss={onDismiss} onSave={save} onDelete={remove} onAddComment={addComment} onEditComment={editComment} onDeleteComment={removeComment} {...props} />
}