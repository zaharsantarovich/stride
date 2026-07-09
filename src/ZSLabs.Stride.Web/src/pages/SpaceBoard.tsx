import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type { CreateTaskRequest, Space, TaskPriority } from '../api/contracts'
import { getSpace } from '../api/spaces'
import { Board } from '../components/Board'
import { useAuth } from '../hooks/useAuth'
import { useTasks } from '../hooks/useTasks'

const priorities: TaskPriority[] = ['Critical', 'High', 'Medium', 'Low']

export function SpaceBoardPage() {
  const navigate = useNavigate()
  const { spaceId } = useParams()
  const parsedSpaceId = Number(spaceId)
  const { currentUser, logout } = useAuth()
  const { tasks, isLoading, errorMessage, addTask, saveTask, moveTask, removeTask, addSubtask, saveSubtask, removeSubtask, addTaskComment, addSubtaskComment, saveComment, removeComment, refreshTasks } = useTasks(parsedSpaceId)
  const [space, setSpace] = useState<Space | null>(null)
  const [pageError, setPageError] = useState<string | null>(null)
  const [createForm, setCreateForm] = useState<CreateTaskRequest>({ title: '', description: '', priority: 'Medium' })

  useEffect(() => {
    let isCancelled = false

    async function loadSpace() {
      try {
        const nextSpace = await getSpace(parsedSpaceId)
        if (!isCancelled) {
          setSpace(nextSpace)
          setPageError(null)
        }
      } catch (error) {
        if (!isCancelled) {
          setPageError(error instanceof Error ? error.message : 'Unable to load space.')
        }
      }
    }

    if (!Number.isNaN(parsedSpaceId)) {
      void loadSpace()
    }

    return () => {
      isCancelled = true
    }
  }, [parsedSpaceId])

  async function handleCreateTask(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    try {
      await addTask({
        title: createForm.title.trim(),
        description: createForm.description?.trim() || null,
        priority: createForm.priority,
      })
      setCreateForm({ title: '', description: '', priority: 'Medium' })
    } catch {
      return
    }
  }

  async function handleSignOut() {
    await logout()
    navigate('/sign-in', { replace: true })
  }

  if (Number.isNaN(parsedSpaceId)) {
    return <main className="p-8 text-stride-danger">Invalid space id.</main>
  }

  return (
    <main className="mx-auto min-h-screen w-full max-w-[96rem] px-4 py-6 sm:px-6 lg:px-8">
      <section className="rounded-[2rem] border border-stride-border bg-stride-surface/95 p-6 shadow-board sm:p-8">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
          <div>
            <button className="text-sm font-semibold text-stride-accent" onClick={() => navigate('/spaces')} type="button">
              Back to spaces
            </button>
            <p className="mt-4 text-sm font-semibold uppercase tracking-[0.25em] text-stride-accent">Space board</p>
            <h1 className="mt-3 font-display text-4xl text-stride-ink">{space?.name ?? 'Loading space...'}</h1>
            <p className="mt-3 max-w-3xl text-base leading-7 text-stride-muted">
              Five always-visible columns keep priorities readable, drag-and-drop updates status, and each task carries its own subtask and comment thread.
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink" onClick={() => void refreshTasks()} type="button">
              Refresh board
            </button>
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink" onClick={() => void handleSignOut()} type="button">
              Sign out {currentUser?.username ? `(${currentUser.username})` : ''}
            </button>
          </div>
        </div>
        <form className="mt-8 grid gap-3 rounded-[1.5rem] border border-stride-border bg-white/80 p-5 lg:grid-cols-[1.2fr_1fr_auto]" onSubmit={handleCreateTask}>
          <div className="grid gap-3 lg:col-span-2 lg:grid-cols-[1.2fr_1fr]">
            <input
              className="rounded-xl border border-stride-border bg-white px-4 py-3"
              placeholder="Task title"
              required
              value={createForm.title}
              onChange={(event) => setCreateForm((current) => ({ ...current, title: event.target.value }))}
            />
            <select
              className="rounded-xl border border-stride-border bg-white px-4 py-3"
              value={createForm.priority}
              onChange={(event) => setCreateForm((current) => ({ ...current, priority: event.target.value as TaskPriority }))}
            >
              {priorities.map((priority) => (
                <option key={priority} value={priority}>
                  {priority}
                </option>
              ))}
            </select>
            <textarea
              className="min-h-24 rounded-xl border border-stride-border bg-white px-4 py-3 lg:col-span-2"
              placeholder="Optional description"
              value={createForm.description ?? ''}
              onChange={(event) => setCreateForm((current) => ({ ...current, description: event.target.value }))}
            />
          </div>
          <button className="rounded-xl bg-stride-accent px-5 py-3 text-sm font-semibold text-white" type="submit">
            Create task
          </button>
        </form>
        {pageError !== null || errorMessage !== null ? (
          <p className="mt-6 rounded-xl border border-stride-danger/30 bg-white px-4 py-3 text-sm text-stride-danger">
            {pageError ?? errorMessage}
          </p>
        ) : null}
        {isLoading ? <p className="mt-6 text-sm text-stride-muted">Loading board...</p> : null}
        {!isLoading && tasks.length === 0 ? (
          <p className="mt-6 rounded-2xl border border-dashed border-stride-border px-4 py-8 text-sm text-stride-muted">
            No tasks yet. Create the first task and it will land in Backlog.
          </p>
        ) : null}
        <div className="mt-8">
          <Board
            tasks={tasks}
            currentUserId={currentUser?.id}
            onStatusChange={moveTask}
            onUpdateTask={saveTask}
            onDeleteTask={removeTask}
            onAddSubtask={addSubtask}
            onUpdateSubtask={saveSubtask}
            onDeleteSubtask={removeSubtask}
            onAddTaskComment={addTaskComment}
            onAddSubtaskComment={addSubtaskComment}
            onUpdateComment={saveComment}
            onDeleteComment={removeComment}
          />
        </div>
      </section>
    </main>
  )
}