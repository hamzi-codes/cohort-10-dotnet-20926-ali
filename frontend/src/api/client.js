const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const TOKEN_KEY = 'tm_token'

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
}

/** Small fetch wrapper that adds the JWT and turns ProblemDetails errors into exceptions. */
export async function apiFetch(path, { method = 'GET', body, headers = {} } = {}) {
  const token = tokenStore.get()
  const response = await fetch(`${BASE_URL}${path}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  const isJson = response.headers.get('content-type')?.includes('json')
  const data = isJson ? await response.json() : null

  if (!response.ok) {
    const message = data?.detail ?? data?.title ?? `Request failed (${response.status})`
    const error = new Error(message)
    error.status = response.status
    throw error
  }
  return data
}

export const authApi = {
  login: (email, password) => apiFetch('/api/auth/login', { method: 'POST', body: { email, password } }),
  register: (fullName, email, password) =>
    apiFetch('/api/auth/register', { method: 'POST', body: { fullName, email, password } }),
}

export const tasksApi = {
  list: () => apiFetch('/api/tasks'),
  summary: () => apiFetch('/api/tasks/summary'),
}
