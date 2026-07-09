export type UserRole = 'Admin' | 'Regular'

export interface CurrentUser {
  id: number
  username: string
  email: string | null
  role: UserRole
}

export interface LoginRequest {
  username: string
  password: string
}

export interface User {
  id: number
  username: string
  email: string | null
  role: UserRole
  createdAt: string
}

export interface CreateUserRequest {
  username: string
  password: string
  email: string | null
}

export interface UpdateUserRequest {
  username?: string
  password?: string | null
  email?: string | null
}

export interface Space {
  id: number
  key: string
  name: string
  authorId: number
  isPublic: boolean
  createdAt: string
  updatedAt?: string | null
}

export interface CreateSpaceRequest {
  key: string
  name: string
  isPublic: boolean
}

export interface UpdateSpaceRequest {
  name?: string
  isPublic?: boolean | null
}

export type TaskStatus = 'Backlog' | 'Todo' | 'InProgress' | 'Done' | 'Archived'

export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Critical'

export type SubtaskStatus = 'Todo' | 'InProgress' | 'Done'

export interface Comment {
  id: number
  taskId: number | null
  subtaskId: number | null
  authorId: number
  content: string
  createdAt: string
  updatedAt?: string | null
}

export interface Subtask {
  id: number
  taskId: number
  title: string
  description: string | null
  status: SubtaskStatus
  authorId: number
  assigneeId: number | null
  dueDate: string | null
  createdAt: string
  updatedAt?: string | null
  comments: Comment[]
}

export interface Task {
  id: number
  spaceId: number
  title: string
  description: string | null
  status: TaskStatus
  priority: TaskPriority
  authorId: number
  assigneeId: number | null
  dueDate: string | null
  createdAt: string
  updatedAt?: string | null
  subtasks: Subtask[]
  comments: Comment[]
}

export interface CreateTaskRequest {
  title: string
  description?: string | null
  status?: TaskStatus
  priority: TaskPriority
  assigneeId?: number | null
  dueDate?: string | null
}

export interface UpdateTaskRequest {
  title?: string
  description?: string | null
  status?: TaskStatus
  priority?: TaskPriority
  assigneeId?: number | null
  dueDate?: string | null
}

export interface CreateSubtaskRequest {
  title: string
  description?: string | null
  status?: SubtaskStatus
  assigneeId?: number | null
  dueDate?: string | null
}

export interface UpdateSubtaskRequest {
  title?: string
  description?: string | null
  status?: SubtaskStatus
  assigneeId?: number | null
  dueDate?: string | null
}

export interface CreateCommentRequest {
  content: string
}

export interface ErrorResponse {
  message: string
}