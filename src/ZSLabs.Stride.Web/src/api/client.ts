export const API_BASE_URL = 'https://localhost:7080'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

export class ApiError extends Error {
  public readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export async function apiRequest<TResponse>(
  path: string,
  method: HttpMethod = 'GET',
  body?: unknown,
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  if (response.status === 204) {
    return undefined as TResponse
  }

  if (!response.ok) {
    const payload = (await response.json().catch(() => ({ message: 'Request failed.' }))) as {
      message?: string
    }

    throw new ApiError(payload.message ?? 'Request failed.', response.status)
  }

  return (await response.json()) as TResponse
}