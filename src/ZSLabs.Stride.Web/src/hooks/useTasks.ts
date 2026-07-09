import { useEffect, useState } from 'react'
import { ApiError } from '../api/client'
import {
  createSubtask,
  createSubtaskComment,
  createTask,
  createTaskComment,
  deleteComment,
  deleteSubtask,
  deleteTask,
  getTasks,
  updateComment,
  updateSubtask,
  updateTask,
  updateTaskStatus,
} from '../api/tasks'
import type {
  CreateSubtaskRequest,
  CreateTaskRequest,
  Task,
  TaskPriority,
  TaskStatus,
  UpdateSubtaskRequest,
  UpdateTaskRequest,
} from '../api/contracts'

const statusOrder: TaskStatus[] = ['Backlog', 'Todo', 'InProgress', 'Done', 'Archived']
const priorityRank: Record<TaskPriority, number> = {
  Critical: 0,
  High: 1,
  Medium: 2,
  Low: 3,
}

function sortTasks(tasks: Task[]) {
  return [...tasks].sort((left, right) => {
    const statusDifference = statusOrder.indexOf(left.status) - statusOrder.indexOf(right.status)

    if (statusDifference !== 0) {
      return statusDifference
    }

    const priorityDifference = priorityRank[left.priority] - priorityRank[right.priority]

    if (priorityDifference !== 0) {
      return priorityDifference
    }

    return new Date(left.createdAt).getTime() - new Date(right.createdAt).getTime()
  })
}

export function useTasks(spaceId: number) {
  const [tasks, setTasks] = useState<Task[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    let isCancelled = false

    async function load() {
      setIsLoading(true)

      try {
        const nextTasks = await getTasks(spaceId)

        if (!isCancelled) {
          setTasks(sortTasks(nextTasks))
          setErrorMessage(null)
        }
      } catch (error) {
        if (!isCancelled) {
          setErrorMessage(error instanceof Error ? error.message : 'Unable to load tasks.')
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false)
        }
      }
    }

    void load()

    return () => {
      isCancelled = true
    }
  }, [spaceId])

  async function refreshTasks() {
    const nextTasks = await getTasks(spaceId)
    setTasks(sortTasks(nextTasks))
    setErrorMessage(null)
  }

  async function addTask(request: CreateTaskRequest) {
    try {
      const created = await createTask(spaceId, request)
      setTasks((current) => sortTasks([...current, created]))
      setErrorMessage(null)
      return created
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to create task.'
      setErrorMessage(message)
      throw error
    }
  }

  async function saveTask(taskId: number, request: UpdateTaskRequest) {
    try {
      const updated = await updateTask(taskId, request)
      setTasks((current) => sortTasks(current.map((task) => (task.id === updated.id ? updated : task))))
      setErrorMessage(null)
      return updated
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to update task.'
      setErrorMessage(message)
      throw error
    }
  }

  async function moveTask(taskId: number, status: TaskStatus) {
    const previousTasks = tasks
    setTasks((current) => sortTasks(current.map((task) => (task.id === taskId ? { ...task, status } : task))))

    try {
      const updated = await updateTaskStatus(taskId, status)
      setTasks((current) => sortTasks(current.map((task) => (task.id === updated.id ? updated : task))))
      setErrorMessage(null)
    } catch (error) {
      setTasks(previousTasks)
      const message = error instanceof ApiError ? error.message : 'Unable to move task.'
      setErrorMessage(message)
      throw error
    }
  }

  async function removeTask(taskId: number) {
    try {
      await deleteTask(taskId)
      setTasks((current) => current.filter((task) => task.id !== taskId))
      setErrorMessage(null)
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to delete task.'
      setErrorMessage(message)
      throw error
    }
  }

  async function addSubtask(taskId: number, request: CreateSubtaskRequest) {
    try {
      await createSubtask(taskId, request)
      await refreshTasks()
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to create subtask.'
      setErrorMessage(message)
      throw error
    }
  }

  async function saveSubtask(subtaskId: number, request: UpdateSubtaskRequest) {
    try {
      await updateSubtask(subtaskId, request)
      await refreshTasks()
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to update subtask.'
      setErrorMessage(message)
      throw error
    }
  }

  async function removeSubtask(subtaskId: number) {
    try {
      await deleteSubtask(subtaskId)
      await refreshTasks()
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to delete subtask.'
      setErrorMessage(message)
      throw error
    }
  }

  async function addTaskComment(taskId: number, content: string) {
    try {
      await createTaskComment(taskId, { content })
      await refreshTasks()
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to add comment.'
      setErrorMessage(message)
      throw error
    }
  }

  async function addSubtaskComment(subtaskId: number, content: string) {
    try {
      await createSubtaskComment(subtaskId, { content })
      await refreshTasks()
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to add comment.'
      setErrorMessage(message)
      throw error
    }
  }

  async function saveComment(commentId: number, content: string) {
    try {
      await updateComment(commentId, { content })
      await refreshTasks()
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to update comment.'
      setErrorMessage(message)
      throw error
    }
  }

  async function removeComment(commentId: number) {
    try {
      await deleteComment(commentId)
      await refreshTasks()
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to delete comment.'
      setErrorMessage(message)
      throw error
    }
  }

  return {
    tasks,
    isLoading,
    errorMessage,
    refreshTasks,
    addTask,
    saveTask,
    moveTask,
    removeTask,
    addSubtask,
    saveSubtask,
    removeSubtask,
    addTaskComment,
    addSubtaskComment,
    saveComment,
    removeComment,
  }
}