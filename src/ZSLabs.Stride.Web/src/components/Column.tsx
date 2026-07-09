import { useDroppable } from '@dnd-kit/core'
import type { CreateSubtaskRequest, Task, TaskStatus, UpdateSubtaskRequest, UpdateTaskRequest } from '../api/contracts'
import { TaskCard } from './TaskCard'

interface ColumnProps {
  status: TaskStatus
  title: string
  tasks: Task[]
  currentUserId?: number
  onUpdateTask: (taskId: number, request: UpdateTaskRequest) => Promise<unknown>
  onDeleteTask: (taskId: number) => Promise<void>
  onAddSubtask: (taskId: number, request: CreateSubtaskRequest) => Promise<void>
  onUpdateSubtask: (subtaskId: number, request: UpdateSubtaskRequest) => Promise<void>
  onDeleteSubtask: (subtaskId: number) => Promise<void>
  onAddTaskComment: (taskId: number, content: string) => Promise<void>
  onAddSubtaskComment: (subtaskId: number, content: string) => Promise<void>
  onUpdateComment: (commentId: number, content: string) => Promise<void>
  onDeleteComment: (commentId: number) => Promise<void>
}

export function Column({
  status,
  title,
  tasks,
  currentUserId,
  onUpdateTask,
  onDeleteTask,
  onAddSubtask,
  onUpdateSubtask,
  onDeleteSubtask,
  onAddTaskComment,
  onAddSubtaskComment,
  onUpdateComment,
  onDeleteComment,
}: ColumnProps) {
  const { setNodeRef, isOver } = useDroppable({
    id: `column-${status}`,
    data: { status },
  })

  return (
    <section
      ref={setNodeRef}
      className={`min-h-[24rem] min-w-[18rem] rounded-[1.75rem] border p-4 ${
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
            currentUserId={currentUserId}
            onUpdate={onUpdateTask}
            onDelete={onDeleteTask}
            onAddSubtask={onAddSubtask}
            onUpdateSubtask={onUpdateSubtask}
            onDeleteSubtask={onDeleteSubtask}
            onAddTaskComment={onAddTaskComment}
            onAddSubtaskComment={onAddSubtaskComment}
            onUpdateComment={onUpdateComment}
            onDeleteComment={onDeleteComment}
          />
        ))}
        {tasks.length === 0 ? <p className="rounded-2xl border border-dashed border-stride-border px-4 py-8 text-sm text-stride-muted">Drop a task here or create one above.</p> : null}
      </div>
    </section>
  )
}