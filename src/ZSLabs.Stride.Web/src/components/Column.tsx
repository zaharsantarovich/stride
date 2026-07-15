import { useDroppable } from '@dnd-kit/core'
import type { Task, TaskStatus } from '../api/contracts'
import { TaskCard } from './TaskCard'

interface ColumnProps {
  status: TaskStatus
  title: string
  tasks: Task[]
  onDeleteTask: (taskId: number) => Promise<void>
  onSelectTask: (task: Task) => void
}

export function Column({
  status,
  title,
  tasks,
  onDeleteTask,
  onSelectTask,
}: ColumnProps) {
  const { setNodeRef, isOver } = useDroppable({
    id: `column-${status}`,
    data: { status },
  })

  return (
    <section
      ref={setNodeRef}
      className={`min-h-[24rem] min-w-[14rem] flex-1 basis-0 rounded-[1.75rem] border p-4 ${
        isOver ? 'border-stride-accent bg-white/95' : 'border-stride-border bg-stride-surface/90'
      }`}
    >
      <div className="flex items-center justify-between gap-3">
        <h2 className="font-display text-2xl text-stride-ink">{title}</h2>
        <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-stride-muted">{tasks.length}</span>
      </div>
      <div className="mt-4 space-y-4">
        {tasks.map((task) => (
          <TaskCard
            key={task.id}
            task={task}
            onDelete={onDeleteTask}
            onSelect={onSelectTask}
          />
        ))}
        {tasks.length === 0 ? <p className="rounded-2xl border border-dashed border-stride-border px-4 py-8 text-sm text-stride-muted">Drop a task here or create one from the board header.</p> : null}
      </div>
    </section>
  )
}