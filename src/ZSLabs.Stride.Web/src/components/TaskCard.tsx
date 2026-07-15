import { useDraggable } from '@dnd-kit/core'
import { CSS } from '@dnd-kit/utilities'
import type { KeyboardEvent, MouseEvent } from 'react'
import type { Task } from '../api/contracts'

interface TaskCardProps {
  task: Task
  onDelete: (taskId: number) => Promise<void>
  onSelect: (task: Task) => void
}

export function TaskCard({ task, onDelete, onSelect }: TaskCardProps) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: `task-${task.id}`,
    data: {
      taskId: task.id,
      status: task.status,
    },
  })

  function handleKeyDown(event: KeyboardEvent<HTMLElement>) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault()
      onSelect(task)
    }
  }

  function stopCardSelection(event: MouseEvent<HTMLButtonElement>) {
    event.stopPropagation()
  }

  return (
    <article
      ref={setNodeRef}
      style={{ transform: CSS.Translate.toString(transform), opacity: isDragging ? 0.65 : 1 }}
      className="cursor-pointer rounded-[1.25rem] border border-stride-border bg-white p-4 shadow-sm transition hover:border-stride-accent/60 hover:shadow-board focus:outline-none focus:ring-2 focus:ring-stride-accent"
      role="button"
      tabIndex={0}
      onClick={() => onSelect(task)}
      onKeyDown={handleKeyDown}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-stride-accent">{task.priority}</p>
          <h3 className="mt-2 break-words font-display text-xl text-stride-ink">{task.title}</h3>
        </div>
        <div className="flex shrink-0 items-center gap-2">
          <button
            className="rounded-lg border border-stride-border px-2 py-1 text-xs font-semibold text-stride-ink"
            type="button"
            onClick={stopCardSelection}
            {...listeners}
            {...attributes}
          >
            Drag
          </button>
          <button
            className="rounded-lg border border-stride-border px-2 py-1 text-xs font-semibold text-stride-danger"
            onClick={(event) => {
              stopCardSelection(event)
              void onDelete(task.id)
            }}
            type="button"
          >
            Delete
          </button>
        </div>
      </div>
      <div className="mt-4 grid gap-2 text-xs text-stride-muted">
        <p>Assigned: {task.assigneeUsername ?? 'Unassigned'}</p>
      </div>
      {task.subtasks.length > 0 ? (
        <ul className="mt-4 grid gap-1.5 text-sm text-stride-ink">
          {task.subtasks.map((subtask) => (
            <li key={subtask.id} className="break-words rounded-lg bg-stride-surface px-3 py-2">
              {subtask.title}
            </li>
          ))}
        </ul>
      ) : (
        <p className="mt-4 text-sm text-stride-muted">No subtasks</p>
      )}
    </article>
  )
}
