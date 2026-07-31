import { useId, useState } from 'react'
import type { FormEvent } from 'react'
import type { CreateTaskRequest, RegularUserLookup, Space, Subtask, Task, TaskPriority, TaskStatus, UpdateTaskRequest } from '../api/contracts'
import { fromDateInput, toDateInput } from '../utils/dateOnly'
import { ModalShell } from './ModalShell'

interface TaskModalProps {
  mode: 'create' | 'edit'
  task: Task | null
  space: Space
  currentUser: RegularUserLookup
  regularUsers: RegularUserLookup[]
  errorMessage: string | null
  onDismiss: () => void
  onSave: (taskRequest: CreateTaskRequest | UpdateTaskRequest) => Promise<void>
  onSelectSubtask: (subtask: Subtask) => void
  onAddSubtask: () => void
}

const taskStatuses: TaskStatus[] = ['Backlog', 'Todo', 'InProgress', 'Done', 'Archived']
const taskPriorities: TaskPriority[] = ['Critical', 'High', 'Medium', 'Low']

export function TaskModal({ mode, task, space, currentUser, regularUsers, errorMessage, onDismiss, onSave, onSelectSubtask, onAddSubtask }: TaskModalProps) {
  const titleId = useId()
  const [title, setTitle] = useState(task?.title ?? '')
  const [description, setDescription] = useState(task?.description ?? '')
  const [status, setStatus] = useState<TaskStatus>(task?.status ?? 'Backlog')
  const [priority, setPriority] = useState<TaskPriority>(task?.priority ?? 'Medium')
  const [assigneeId, setAssigneeId] = useState<number | null>(task?.assigneeId ?? null)
  const [dueDate, setDueDate] = useState(toDateInput(task?.dueDate))
  const [isSaving, setIsSaving] = useState(false)
  const assigneeOptions = space.isPublic ? regularUsers : [currentUser]
  const orderedSubtasks = [...(task?.subtasks ?? [])].sort(
    (left, right) => new Date(left.createdAt).getTime() - new Date(right.createdAt).getTime(),
  )

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSaving(true)

    try {
      await onSave({
        title: title.trim(),
        description: description.trim() || null,
        status,
        priority,
        assigneeId,
        dueDate: fromDateInput(dueDate),
      })
    } finally {
      setIsSaving(false)
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
          <section className="mt-6 border-t border-stride-border pt-5">
            <div className="flex items-center justify-between gap-3">
              <h3 className="font-display text-2xl text-stride-ink">Subtasks</h3>
              <button className="rounded-xl border border-stride-border bg-white px-3 py-2 text-sm font-semibold text-stride-ink disabled:opacity-60" disabled={mode === 'create'} type="button" onClick={onAddSubtask}>
                Add subtask
              </button>
            </div>
            {mode === 'create' ? <p className="mt-4 text-sm text-stride-muted">Save the task before adding subtasks.</p> : null}
            {mode === 'edit' && orderedSubtasks.length === 0 ? <p className="mt-4 text-sm text-stride-muted">No subtasks yet.</p> : null}
            {mode === 'edit' && orderedSubtasks.length > 0 ? (
              <div className="mt-4 overflow-x-auto">
                <table className="w-full min-w-[32rem] border-collapse text-left text-sm">
                  <thead className="border-b border-stride-border text-xs uppercase text-stride-muted">
                    <tr><th className="px-3 py-2">Title</th><th className="px-3 py-2">Status</th><th className="px-3 py-2">Assignee</th></tr>
                  </thead>
                  <tbody>
                    {orderedSubtasks.map((subtask) => (
                      <tr key={subtask.id} className="border-b border-stride-border/70 last:border-0">
                        <td className="px-3 py-3">
                          <a
                            aria-label={`Open subtask ${subtask.title} from task ${task?.title ?? ''}`}
                            className="font-semibold text-stride-accent underline decoration-1 underline-offset-4 focus:outline-none focus:ring-2 focus:ring-stride-accent"
                            href={`/spaces/${space.id}?subtask=${subtask.id}`}
                            onClick={(event) => { event.preventDefault(); onSelectSubtask(subtask) }}
                          >
                            {subtask.title}
                          </a>
                        </td>
                        <td className="px-3 py-3 text-stride-ink">{subtask.status}</td>
                        <td className="px-3 py-3 text-stride-muted">{subtask.assigneeUsername ?? 'Unassigned'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : null}
          </section>
    </ModalShell>
  )
}
