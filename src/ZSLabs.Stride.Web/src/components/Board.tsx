import { DndContext, PointerSensor, closestCorners, useSensor, useSensors } from '@dnd-kit/core'
import type { DragEndEvent } from '@dnd-kit/core'
import type { CreateSubtaskRequest, Task, TaskStatus, UpdateSubtaskRequest, UpdateTaskRequest } from '../api/contracts'
import { Column } from './Column'

interface BoardProps {
  tasks: Task[]
  currentUserId?: number
  onStatusChange: (taskId: number, status: TaskStatus) => Promise<void>
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

const columns: Array<{ status: TaskStatus; title: string }> = [
  { status: 'Backlog', title: 'Backlog' },
  { status: 'Todo', title: 'Todo' },
  { status: 'InProgress', title: 'In Progress' },
  { status: 'Done', title: 'Done' },
  { status: 'Archived', title: 'Archived' },
]

export function Board({
  tasks,
  currentUserId,
  onStatusChange,
  onUpdateTask,
  onDeleteTask,
  onAddSubtask,
  onUpdateSubtask,
  onDeleteSubtask,
  onAddTaskComment,
  onAddSubtaskComment,
  onUpdateComment,
  onDeleteComment,
}: BoardProps) {
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 8 } }))

  async function handleDragEnd(event: DragEndEvent) {
    const taskId = event.active.data.current?.taskId as number | undefined
    const currentStatus = event.active.data.current?.status as TaskStatus | undefined
    const nextStatus = event.over?.data.current?.status as TaskStatus | undefined

    if (taskId === undefined || currentStatus === undefined || nextStatus === undefined || currentStatus === nextStatus) {
      return
    }

    await onStatusChange(taskId, nextStatus)
  }

  return (
    <DndContext sensors={sensors} collisionDetection={closestCorners} onDragEnd={(event) => void handleDragEnd(event)}>
      <div className="flex gap-4 overflow-x-auto pb-4">
        {columns.map((column) => (
          <Column
            key={column.status}
            status={column.status}
            title={column.title}
            tasks={tasks.filter((task) => task.status === column.status)}
            currentUserId={currentUserId}
            onUpdateTask={onUpdateTask}
            onDeleteTask={onDeleteTask}
            onAddSubtask={onAddSubtask}
            onUpdateSubtask={onUpdateSubtask}
            onDeleteSubtask={onDeleteSubtask}
            onAddTaskComment={onAddTaskComment}
            onAddSubtaskComment={onAddSubtaskComment}
            onUpdateComment={onUpdateComment}
            onDeleteComment={onDeleteComment}
          />
        ))}
      </div>
    </DndContext>
  )
}