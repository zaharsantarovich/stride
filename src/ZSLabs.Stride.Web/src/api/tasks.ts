import { apiRequest } from './client'
import type {
  Comment,
  CreateCommentRequest,
  CreateSubtaskRequest,
  CreateTaskRequest,
  Subtask,
  Task,
  TaskStatus,
  UpdateSubtaskRequest,
  UpdateTaskRequest,
} from './contracts'

export function getTasks(spaceId: number) {
  return apiRequest<Task[]>(`/spaces/${spaceId}/tasks`)
}

export function createTask(spaceId: number, request: CreateTaskRequest) {
  return apiRequest<Task>(`/spaces/${spaceId}/tasks`, 'POST', request)
}

export function updateTask(taskId: number, request: UpdateTaskRequest) {
  return apiRequest<Task>(`/tasks/${taskId}`, 'PUT', request)
}

export function updateTaskStatus(taskId: number, status: TaskStatus) {
  return apiRequest<Task>(`/tasks/${taskId}/status`, 'PATCH', { status })
}

export function deleteTask(taskId: number) {
  return apiRequest<void>(`/tasks/${taskId}`, 'DELETE')
}

export function createSubtask(taskId: number, request: CreateSubtaskRequest) {
  return apiRequest<Subtask>(`/tasks/${taskId}/subtasks`, 'POST', request)
}

export function updateSubtask(subtaskId: number, request: UpdateSubtaskRequest) {
  return apiRequest<Subtask>(`/subtasks/${subtaskId}`, 'PUT', request)
}

export function deleteSubtask(subtaskId: number) {
  return apiRequest<void>(`/subtasks/${subtaskId}`, 'DELETE')
}

export function createTaskComment(taskId: number, request: CreateCommentRequest) {
  return apiRequest<Comment>(`/tasks/${taskId}/comments`, 'POST', request)
}

export function createSubtaskComment(subtaskId: number, request: CreateCommentRequest) {
  return apiRequest<Comment>(`/subtasks/${subtaskId}/comments`, 'POST', request)
}

export function updateComment(commentId: number, request: CreateCommentRequest) {
  return apiRequest<Comment>(`/comments/${commentId}`, 'PUT', request)
}

export function deleteComment(commentId: number) {
  return apiRequest<void>(`/comments/${commentId}`, 'DELETE')
}