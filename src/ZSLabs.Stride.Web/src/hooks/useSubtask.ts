import { useEffect, useState } from 'react'
import { ApiError } from '../api/client'
import type { CreateSubtaskRequest, Subtask, UpdateSubtaskRequest } from '../api/contracts'
import { createSubtask, createSubtaskComment, deleteComment, deleteSubtask, getSubtask, updateComment, updateSubtask } from '../api/tasks'

function getErrorMessage(error: unknown, fallback: string) {
  return error instanceof ApiError || error instanceof Error ? error.message : fallback
}

function sortComments<T extends { createdAt: string }>(comments: T[]) {
  return [...comments].sort((left, right) => new Date(left.createdAt).getTime() - new Date(right.createdAt).getTime())
}

export function useSubtask(initialSubtaskId: number | null, parentTaskId: number, refreshTasks: () => Promise<unknown>) {
  const [subtask, setSubtask] = useState<Subtask | null>(null)
  const [subtaskId, setSubtaskId] = useState<number | null>(initialSubtaskId)
  const [isLoading, setIsLoading] = useState(initialSubtaskId !== null)
  const [isSaving, setIsSaving] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [isMutatingComment, setIsMutatingComment] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  async function load() {
    if (subtaskId === null) {
      return
    }

    setIsLoading(true)

    try {
      setSubtask(await getSubtask(subtaskId))
      setErrorMessage(null)
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Unable to load subtask.'))
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    let isCancelled = false

    async function loadInitialSubtask() {
      if (initialSubtaskId === null) {
        setIsLoading(false)
        return
      }

      setIsLoading(true)

      try {
        const nextSubtask = await getSubtask(initialSubtaskId)

        if (!isCancelled) {
          setSubtask(nextSubtask)
          setErrorMessage(null)
        }
      } catch (error) {
        if (!isCancelled) {
          setErrorMessage(getErrorMessage(error, 'Unable to load subtask.'))
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false)
        }
      }
    }

    void loadInitialSubtask()

    return () => {
      isCancelled = true
    }
  }, [initialSubtaskId])

  async function create(request: CreateSubtaskRequest) {
    setIsSaving(true)

    try {
      const created = await createSubtask(parentTaskId, request)
      setSubtaskId(created.id)
      setSubtask(created)
      await refreshTasks()
      setErrorMessage(null)
      return created
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Unable to create subtask.'))
      throw error
    } finally {
      setIsSaving(false)
    }
  }

  async function save(request: UpdateSubtaskRequest) {
    if (subtaskId === null) {
      throw new Error('Save the subtask before updating it.')
    }

    setIsSaving(true)

    try {
      const updated = await updateSubtask(subtaskId, request)
      setSubtask(updated)
      await refreshTasks()
      setErrorMessage(null)
      return updated
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Unable to update subtask.'))
      throw error
    } finally {
      setIsSaving(false)
    }
  }

  async function remove() {
    if (subtaskId === null) {
      throw new Error('Save the subtask before deleting it.')
    }

    setIsDeleting(true)

    try {
      await deleteSubtask(subtaskId)
      await refreshTasks()
      setErrorMessage(null)
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Unable to delete subtask.'))
      throw error
    } finally {
      setIsDeleting(false)
    }
  }

  async function addComment(content: string) {
    if (subtaskId === null) {
      throw new Error('Save the subtask before adding comments.')
    }

    setIsMutatingComment(true)

    try {
      const created = await createSubtaskComment(subtaskId, { content })
      setSubtask((current) => current === null ? current : { ...current, comments: sortComments([...current.comments, created]) })
      await refreshTasks()
      setErrorMessage(null)
      return created
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Unable to add comment.'))
      throw error
    } finally {
      setIsMutatingComment(false)
    }
  }

  async function editComment(commentId: number, content: string) {
    setIsMutatingComment(true)

    try {
      const updated = await updateComment(commentId, { content })
      setSubtask((current) => current === null ? current : {
        ...current,
        comments: sortComments(current.comments.map((comment) => comment.id === updated.id ? updated : comment)),
      })
      await refreshTasks()
      setErrorMessage(null)
      return updated
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Unable to update comment.'))
      throw error
    } finally {
      setIsMutatingComment(false)
    }
  }

  async function removeComment(commentId: number) {
    setIsMutatingComment(true)

    try {
      await deleteComment(commentId)
      setSubtask((current) => current === null ? current : {
        ...current,
        comments: current.comments.filter((comment) => comment.id !== commentId),
      })
      await refreshTasks()
      setErrorMessage(null)
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Unable to delete comment.'))
      throw error
    } finally {
      setIsMutatingComment(false)
    }
  }

  return {
    subtask,
    isLoading,
    isSaving,
    isDeleting,
    isMutatingComment,
    errorMessage,
    reload: load,
    create,
    save,
    remove,
    addComment,
    editComment,
    removeComment,
  }
}