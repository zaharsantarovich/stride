import { useDraggable } from '@dnd-kit/core'
import { CSS } from '@dnd-kit/utilities'
import { useState } from 'react'
import type { FormEvent } from 'react'
import type { CreateSubtaskRequest, Task, TaskPriority, TaskStatus, UpdateSubtaskRequest, UpdateTaskRequest } from '../api/contracts'
import { CommentList } from './CommentList'
import { SubtaskList } from './SubtaskList'
import { formatLocalDateTime } from '../utils/formatLocalDateTime'

interface TaskCardProps {
  task: Task
  currentUserId?: number
  onUpdate: (taskId: number, request: UpdateTaskRequest) => Promise<unknown>
  onDelete: (taskId: number) => Promise<void>
  onAddSubtask: (taskId: number, request: CreateSubtaskRequest) => Promise<void>
  onUpdateSubtask: (subtaskId: number, request: UpdateSubtaskRequest) => Promise<void>
  onDeleteSubtask: (subtaskId: number) => Promise<void>
  onAddTaskComment: (taskId: number, content: string) => Promise<void>
  onAddSubtaskComment: (subtaskId: number, content: string) => Promise<void>
  onUpdateComment: (commentId: number, content: string) => Promise<void>
  onDeleteComment: (commentId: number) => Promise<void>
}

const taskStatuses: TaskStatus[] = ['Backlog', 'Todo', 'InProgress', 'Done', 'Archived']
const taskPriorities: TaskPriority[] = ['Critical', 'High', 'Medium', 'Low']

export function TaskCard({
  task,
  currentUserId,
  onUpdate,
  onDelete,
  onAddSubtask,
  onUpdateSubtask,
  onDeleteSubtask,
  onAddTaskComment,
  onAddSubtaskComment,
  onUpdateComment,
  onDeleteComment,
}: TaskCardProps) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: `task-${task.id}`,
    data: {
      taskId: task.id,
      status: task.status,
    },
  })
  const [isEditing, setIsEditing] = useState(false)
  const [form, setForm] = useState<UpdateTaskRequest>({
    title: task.title,
    description: task.description,
    priority: task.priority,
    status: task.status,
  })

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    await onUpdate(task.id, {
      ...form,
      title: form.title?.trim(),
      description: form.description?.trim() || null,
    })
    setIsEditing(false)
  }

  return (
    <article
      ref={setNodeRef}
      style={{ transform: CSS.Translate.toString(transform), opacity: isDragging ? 0.65 : 1 }}
      className="rounded-[1.5rem] border border-stride-border bg-white p-4 shadow-sm"
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-stride-accent">{task.priority}</p>
          <h3 className="mt-2 font-display text-xl text-stride-ink">{task.title}</h3>
          <p className="mt-2 text-xs text-stride-muted">Created {formatLocalDateTime(task.createdAt)}</p>
        </div>
        <div className="flex items-center gap-3">
          <button
            className="rounded-lg border border-stride-border px-2 py-1 text-xs font-semibold text-stride-ink"
            type="button"
            {...listeners}
            {...attributes}
          >
            Drag
          </button>
          <button className="text-sm font-semibold text-stride-accent" onClick={() => setIsEditing((current) => !current)} type="button">
            {isEditing ? 'Close' : 'Edit'}
          </button>
          <button className="text-sm font-semibold text-stride-danger" onClick={() => void onDelete(task.id)} type="button">
            Delete
          </button>
        </div>
      </div>
      {task.description ? <p className="mt-3 text-sm leading-6 text-stride-ink">{task.description}</p> : null}
      <div className="mt-4 grid gap-2 text-xs text-stride-muted sm:grid-cols-2">
        <p>Status: {task.status}</p>
        <p>Comments: {task.comments.length}</p>
        <p>Subtasks: {task.subtasks.length}</p>
        <p>{task.updatedAt ? `Updated ${formatLocalDateTime(task.updatedAt)}` : 'Awaiting updates'}</p>
      </div>
      {isEditing ? (
        <form className="mt-4 grid gap-3" onSubmit={handleSubmit}>
          <input
            className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2"
            value={form.title ?? ''}
            onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))}
          />
          <textarea
            className="min-h-24 rounded-xl border border-stride-border bg-stride-surface px-3 py-2"
            value={form.description ?? ''}
            onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
          />
          <div className="grid gap-3 sm:grid-cols-2">
            <select
              className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2"
              value={form.priority ?? task.priority}
              onChange={(event) => setForm((current) => ({ ...current, priority: event.target.value as TaskPriority }))}
            >
              {taskPriorities.map((priority) => (
                <option key={priority} value={priority}>
                  {priority}
                </option>
              ))}
            </select>
            <select
              className="rounded-xl border border-stride-border bg-stride-surface px-3 py-2"
              value={form.status ?? task.status}
              onChange={(event) => setForm((current) => ({ ...current, status: event.target.value as TaskStatus }))}
            >
              {taskStatuses.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </div>
          <div className="flex gap-2">
            <button className="rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white" type="submit">
              Save task
            </button>
            <button
              className="rounded-xl border border-stride-border px-4 py-2 text-sm font-semibold text-stride-ink"
              onClick={() => setIsEditing(false)}
              type="button"
            >
              Cancel
            </button>
          </div>
        </form>
      ) : null}
      <div className="mt-4 space-y-4">
        <SubtaskList
          subtasks={task.subtasks}
          currentUserId={currentUserId}
          onAdd={(request) => onAddSubtask(task.id, request)}
          onUpdate={onUpdateSubtask}
          onDelete={onDeleteSubtask}
          onAddComment={onAddSubtaskComment}
          onUpdateComment={onUpdateComment}
          onDeleteComment={onDeleteComment}
        />
        <CommentList
          comments={task.comments}
          currentUserId={currentUserId}
          onAdd={(content) => onAddTaskComment(task.id, content)}
          onUpdate={onUpdateComment}
          onDelete={onDeleteComment}
        />
      </div>
    </article>
  )
}