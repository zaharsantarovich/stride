import { apiRequest } from './client'
import type { CreateSpaceRequest, Space, UpdateSpaceRequest } from './contracts'

export function getSpaces() {
  return apiRequest<Space[]>('/spaces')
}

export function getSpace(spaceId: number) {
  return apiRequest<Space>(`/spaces/${spaceId}`)
}

export function createSpace(request: CreateSpaceRequest) {
  return apiRequest<Space>('/spaces', 'POST', request)
}

export function updateSpace(spaceId: number, request: UpdateSpaceRequest) {
  return apiRequest<Space>(`/spaces/${spaceId}`, 'PUT', request)
}

export function deleteSpace(spaceId: number) {
  return apiRequest<void>(`/spaces/${spaceId}`, 'DELETE')
}