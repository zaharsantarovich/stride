import { useId, useState } from 'react'
import type { FormEvent } from 'react'
import type { CreateTaskRequest, RegularUserLookup, Space, SubtaskStatus, Task, TaskPriority, TaskStatus, UpdateTaskRequest } from '../api/contracts'
import type { SubtaskDraftSave } from '../hooks/useTasks'

interface TaskModalProps {
  mode: 'create' | 'edit'
  task: Task | null
  space: Space
  currentUser: RegularUserLookup
  regularUsers: RegularUserLookup[]
  errorMessage: string | null
  onDismiss: () => void
  onSave: (taskRequest: CreateTaskRequest | UpdateTaskRequest, subtasks: SubtaskDraftSave[]) => Promise<void>
}

type SubtaskDraft = SubtaskDraftSave & { localId: string }

const taskStatuses: TaskStatus[] = ['Backlog', 'Todo', 'InProgress', 'Done', 'Archived']
const taskPriorities: TaskPriority[] = ['Critical', 'High', 'Medium', 'Low']
const subtaskStatuses: SubtaskStatus[] = ['Todo', 'InProgress', 'Done']

function toDateInput(value: string | null | undefined) {
  if (!value) {
    return ''
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return ''
  }

  const offset = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offset).toISOString().slice(0, 10)
}

function fromDateInput(value: string) {
  return value.length === 0 ? null : new Date(`${value}T00:00:00`).toISOString()
}

function createEmptySubtask(): SubtaskDraft {
  return {
    localId: crypto.randomUUID(),
    title: '',
    description: null,
    status: 'Todo',
    assigneeId: null,
    dueDate: null,
  }
}

export function TaskModal({ mode, task, space, currentUser, regularUsers, errorMessage, onDismiss, onSave }: TaskModalProps) {
  const titleId = useId()
  const [title, setTitle] = useState(task?.title ?? '')
  const [description, setDescription] = useState(task?.description ?? '')
  const [status, setStatus] = useState<TaskStatus>(task?.status ?? 'Backlog')
  const [priority, setPriority] = useState<TaskPriority>(task?.priority ?? 'Medium')
  const [assigneeId, setAssigneeId] = useState<number | null>(task?.assigneeId ?? null)
  const [dueDate, setDueDate] = useState(toDateInput(task?.dueDate))
  const [subtasks, setSubtasks] = useState<SubtaskDraft[]>(() =>
    task?.subtasks.map((subtask) => ({
      localId: `subtask-${subtask.id}`,
      id: subtask.id,
      title: subtask.title,
      description: subtask.description,
      status: subtask.status,
      assigneeId: subtask.assigneeId,
      dueDate: subtask.dueDate,
    })) ?? [],
  )
  const [isSaving, setIsSaving] = useState(false)
  const assigneeOptions = space.isPublic ? regularUsers : [currentUser]

  function updateSubtask(localId: string, next: Partial<SubtaskDraft>) {
    setSubtasks((current) => current.map((subtask) => (subtask.localId === localId ? { ...subtask, ...next } : subtask)))
  }

  function removeSubtask(localId: string) {
    setSubtasks((current) =>
      current.flatMap((subtask) => {
        if (subtask.localId !== localId) {
          return [subtask]
        }

        return subtask.id === undefined ? [] : [{ ...subtask, isDeleted: true }]
      }),
    )
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSaving(true)

    try {
      await onSave(
        {
          title: title.trim(),
          description: description.trim() || null,
          status,
          priority,
          assigneeId,
          dueDate: fromDateInput(dueDate),
        },
        subtasks.map((subtask) => ({
          id: subtask.id,
          title: subtask.title,
          description: subtask.description?.trim() || null,
          status: subtask.status,
          assigneeId: subtask.assigneeId,
          dueDate: subtask.dueDate,
          isDeleted: subtask.isDeleted,
        })),
      )
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 bg-stride-ink/35 md:grid md:place-items-center md:p-6" role="presentation" onMouseDown={onDismiss}>
      <form
        aria-labelledby={titleId}
        className="flex h-svh w-full flex-col overflow-hidden bg-white shadow-board md:max-h-[90vh] md:w-[90vw] md:max-w-[960px] md:rounded-[1.75rem] md:border md:border-stride-border"
        role="dialog"
        aria-modal="true"
        onMouseDown={(event) => event.stopPropagation()}
        onSubmit={(event) => void handleSubmit(event)}
      >
        <header className="flex flex-col gap-4 border-b border-stride-border bg-stride-surface px-5 py-4 md:flex-row md:items-start md:justify-between md:px-6">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.22em] text-stride-accent">{mode === 'create' ? 'Create task' : 'Edit task'}</p>
            <h2 id={titleId} className="mt-2 font-display text-3xl text-stride-ink">
              {mode === 'create' ? 'New task' : task?.title ?? 'Task'}
            </h2>
          </div>
          <div className="flex gap-2">
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink" type="button" onClick={onDismiss}>
              Cancel
            </button>
            <button className="rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white disabled:opacity-60" disabled={isSaving} type="submit">
              {isSaving ? 'Saving...' : mode === 'create' ? 'Create task' : 'Save changes'}
            </button>
          </div>
        </header>
        <div className="min-h-0 flex-1 overflow-y-auto px-5 py-5 md:px-6">
          {errorMessage ? <p className="mb-5 rounded-xl border border-stride-danger/30 bg-stride-surface px-4 py-3 text-sm text-stride-danger">{errorMessage}</p> : null}
          <div className="grid gap-4 md:grid-cols-2">
            <label className="grid gap-2 text-sm font-semibold text-stride-ink md:col-span-2">
              Title
              <input className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" required value={title} onChange={(event) => setTitle(event.target.value)} />
            </label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">
              Priority
              <select className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={priority} onChange={(event) => setPriority(event.target.value as TaskPriority)}>
                {taskPriorities.map((option) => (
                  <option key={option} value={option}>{option}</option>
                ))}
              </select>
            </label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">
              Status
              <select className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={status} onChange={(event) => setStatus(event.target.value as TaskStatus)}>
                {taskStatuses.map((option) => (
                  <option key={option} value={option}>{option}</option>
                ))}
              </select>
            </label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">
              Assignee
              <select
                className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal"
                value={assigneeId ?? ''}
                onChange={(event) => setAssigneeId(event.target.value === '' ? null : Number(event.target.value))}
              >
                <option value="">Unassigned</option>
                {assigneeOptions.map((user) => (
                  <option key={user.id} value={user.id}>{user.username}</option>
                ))}
              </select>
            </label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink">
              Due date
              <input className="rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" type="date" value={dueDate} onChange={(event) => setDueDate(event.target.value)} />
            </label>
            <label className="grid gap-2 text-sm font-semibold text-stride-ink md:col-span-2">
              Description
              <textarea className="min-h-28 rounded-xl border border-stride-border bg-white px-4 py-3 font-normal" value={description} onChange={(event) => setDescription(event.target.value)} />
            </label>
          </div>
          <section className="mt-6 rounded-2xl border border-stride-border bg-stride-panel/70 p-4">
            <div className="flex items-center justify-between gap-3">
              <h3 className="font-display text-2xl text-stride-ink">Subtasks</h3>
              <button className="rounded-xl border border-stride-border bg-white px-3 py-2 text-sm font-semibold text-stride-ink" type="button" onClick={() => setSubtasks((current) => [...current, createEmptySubtask()])}>
                Add subtask
              </button>
            </div>
            <div className="mt-4 grid gap-3">
              {subtasks.filter((subtask) => !subtask.isDeleted).map((subtask) => (
                <article key={subtask.localId} className="grid gap-3 rounded-xl border border-stride-border bg-white p-4">
                  <div className="grid gap-3 md:grid-cols-[1fr_auto]">
                    <input className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2" placeholder="Subtask title" value={subtask.title} onChange={(event) => updateSubtask(subtask.localId, { title: event.target.value })} />
                    <button className="rounded-xl border border-stride-border px-3 py-2 text-sm font-semibold text-stride-danger" type="button" onClick={() => removeSubtask(subtask.localId)}>
                      Remove
                    </button>
                  </div>
                  <div className="grid gap-3 md:grid-cols-3">
                    <select className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2" value={subtask.status} onChange={(event) => updateSubtask(subtask.localId, { status: event.target.value as SubtaskStatus })}>
                      {subtaskStatuses.map((option) => (
                        <option key={option} value={option}>{option}</option>
                      ))}
                    </select>
                    <select className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2" value={subtask.assigneeId ?? ''} onChange={(event) => updateSubtask(subtask.localId, { assigneeId: event.target.value === '' ? null : Number(event.target.value) })}>
                      <option value="">Unassigned</option>
                      {assigneeOptions.map((user) => (
                        <option key={user.id} value={user.id}>{user.username}</option>
                      ))}
                    </select>
                    <input className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2" type="date" value={toDateInput(subtask.dueDate)} onChange={(event) => updateSubtask(subtask.localId, { dueDate: fromDateInput(event.target.value) })} />
                  </div>
                  <textarea className="min-h-20 rounded-xl border border-stride-border bg-stride-surface px-3 py-2" placeholder="Optional subtask description" value={subtask.description ?? ''} onChange={(event) => updateSubtask(subtask.localId, { description: event.target.value })} />
                </article>
              ))}
              {subtasks.filter((subtask) => !subtask.isDeleted).length === 0 ? <p className="text-sm text-stride-muted">No subtasks yet.</p> : null}
            </div>
          </section>
        </div>
      </form>
    </div>
  )
}
