import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type { CreateTaskRequest, RegularUserLookup, Space, Task, UpdateTaskRequest } from '../api/contracts'
import { getSpace } from '../api/spaces'
import { getRegularUsers } from '../api/users'
import { Board } from '../components/Board'
import { TaskModal } from '../components/TaskModal'
import { useAuth } from '../hooks/useAuth'
import type { SubtaskDraftSave } from '../hooks/useTasks'
import { useTasks } from '../hooks/useTasks'

type ModalState =
  | { mode: 'create'; task: null }
  | { mode: 'edit'; task: Task }

export function SpaceBoardPage() {
  const navigate = useNavigate()
  const { spaceId } = useParams()
  const parsedSpaceId = Number(spaceId)
  const { currentUser, logout } = useAuth()
  const { tasks, isLoading, errorMessage, addTaskWithSubtasks, saveTaskWithSubtasks, moveTask, removeTask, refreshTasks } = useTasks(parsedSpaceId)
  const [space, setSpace] = useState<Space | null>(null)
  const [regularUsers, setRegularUsers] = useState<RegularUserLookup[]>([])
  const [pageError, setPageError] = useState<string | null>(null)
  const [modalState, setModalState] = useState<ModalState | null>(null)

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

  useEffect(() => {
    let isCancelled = false

    async function loadUsers() {
      try {
        const users = await getRegularUsers()
        if (!isCancelled) {
          setRegularUsers(users)
        }
      } catch (error) {
        if (!isCancelled) {
          setPageError(error instanceof Error ? error.message : 'Unable to load assignees.')
        }
      }
    }

    if (currentUser?.role === 'Regular') {
      void loadUsers()
    }

    return () => {
      isCancelled = true
    }
  }, [currentUser?.role])

  async function handleModalSave(taskRequest: CreateTaskRequest | UpdateTaskRequest, subtasks: SubtaskDraftSave[]) {
    if (modalState?.mode === 'edit') {
      await saveTaskWithSubtasks(modalState.task.id, taskRequest as UpdateTaskRequest, subtasks)
      setModalState(null)
      return
    }

    await addTaskWithSubtasks(taskRequest as CreateTaskRequest, subtasks)
    setModalState(null)
  }

  async function handleSignOut() {
    await logout()
    navigate('/sign-in', { replace: true })
  }

  if (Number.isNaN(parsedSpaceId)) {
    return <main className="p-8 text-stride-danger">Invalid space id.</main>
  }

  return (
    <main className="min-h-screen w-full px-4 py-6 sm:px-6 lg:px-8 2xl:px-10">
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
            <button className="rounded-xl bg-stride-accent px-4 py-2 text-sm font-semibold text-white" onClick={() => setModalState({ mode: 'create', task: null })} type="button">
              Create Task
            </button>
            <button className="rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink" onClick={() => void handleSignOut()} type="button">
              Sign out {currentUser?.username ? `(${currentUser.username})` : ''}
            </button>
          </div>
        </div>
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
            onStatusChange={moveTask}
            onDeleteTask={removeTask}
            onSelectTask={(task) => setModalState({ mode: 'edit', task })}
          />
        </div>
        {modalState !== null && space !== null && currentUser !== null ? (
          <TaskModal
            mode={modalState.mode}
            task={modalState.task}
            space={space}
            currentUser={{ id: currentUser.id, username: currentUser.username }}
            regularUsers={regularUsers}
            errorMessage={errorMessage}
            onDismiss={() => setModalState(null)}
            onSave={handleModalSave}
          />
        ) : null}
      </section>
    </main>
  )
}