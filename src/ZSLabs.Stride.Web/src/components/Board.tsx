import { DndContext, PointerSensor, closestCorners, useSensor, useSensors } from '@dnd-kit/core'
import type { DragEndEvent } from '@dnd-kit/core'
import type { Task, TaskStatus } from '../api/contracts'
import { Column } from './Column'

interface BoardProps {
  tasks: Task[]
  onStatusChange: (taskId: number, status: TaskStatus) => Promise<void>
  onDeleteTask: (taskId: number) => Promise<void>
  onSelectTask: (task: Task) => void
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
  onStatusChange,
  onDeleteTask,
  onSelectTask,
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
      <div className="flex w-full gap-4 overflow-x-auto pb-4">
        {columns.map((column) => (
          <Column
            key={column.status}
            status={column.status}
            title={column.title}
            tasks={tasks.filter((task) => task.status === column.status)}
            onDeleteTask={onDeleteTask}
            onSelectTask={onSelectTask}
          />
        ))}
      </div>
    </DndContext>
  )
}