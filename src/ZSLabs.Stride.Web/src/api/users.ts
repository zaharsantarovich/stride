import { apiRequest } from './client'
import type { CreateUserRequest, RegularUserLookup, UpdateUserRequest, User } from './contracts'

export function getUsers() {
  return apiRequest<User[]>('/users')
}

export function getRegularUsers() {
  return apiRequest<RegularUserLookup[]>('/regular-users')
}

export function createUser(request: CreateUserRequest) {
  return apiRequest<User>('/users', 'POST', request)
}

export function updateUser(userId: number, request: UpdateUserRequest) {
  return apiRequest<User>(`/users/${userId}`, 'PUT', request)
}