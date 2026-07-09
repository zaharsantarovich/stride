import { useEffect, useState } from 'react'
import { ApiError } from '../api/client'
import { createSpace, deleteSpace, getSpaces, updateSpace } from '../api/spaces'
import type { CreateSpaceRequest, Space, UpdateSpaceRequest } from '../api/contracts'

export function useSpaces() {
  const [spaces, setSpaces] = useState<Space[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    void refreshSpaces()
  }, [])

  async function refreshSpaces() {
    setIsLoading(true)

    try {
      setSpaces(await getSpaces())
      setErrorMessage(null)
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Unable to load spaces.')
    } finally {
      setIsLoading(false)
    }
  }

  async function addSpace(request: CreateSpaceRequest) {
    try {
      const created = await createSpace(request)
      setSpaces((current) => [...current, created].sort((left, right) => left.name.localeCompare(right.name)))
      setErrorMessage(null)
      return created
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to create space.'
      setErrorMessage(message)
      throw error
    }
  }

  async function saveSpace(spaceId: number, request: UpdateSpaceRequest) {
    try {
      const updated = await updateSpace(spaceId, request)
      setSpaces((current) => current.map((space) => (space.id === updated.id ? updated : space)))
      setErrorMessage(null)
      return updated
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to update space.'
      setErrorMessage(message)
      throw error
    }
  }

  async function removeSpace(spaceId: number) {
    try {
      await deleteSpace(spaceId)
      setSpaces((current) => current.filter((space) => space.id !== spaceId))
      setErrorMessage(null)
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to delete space.'
      setErrorMessage(message)
      throw error
    }
  }

  return {
    spaces,
    isLoading,
    errorMessage,
    refreshSpaces,
    addSpace,
    saveSpace,
    removeSpace,
  }
}