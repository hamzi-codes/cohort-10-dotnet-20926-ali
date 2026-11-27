import { createContext, useCallback, useMemo, useState } from 'react'
import { authApi, tokenStore } from '../api/client'

const USER_KEY = 'tm_user'

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext(null)

function readStoredUser() {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw && tokenStore.get() ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(readStoredUser)

  const saveSession = useCallback((response) => {
    tokenStore.set(response.token)
    const sessionUser = {
      id: response.userId,
      fullName: response.fullName,
      email: response.email,
      role: response.role,
    }
    localStorage.setItem(USER_KEY, JSON.stringify(sessionUser))
    setUser(sessionUser)
  }, [])

  const login = useCallback(async (email, password) => saveSession(await authApi.login(email, password)), [saveSession])

  const register = useCallback(
    async (fullName, email, password) => saveSession(await authApi.register(fullName, email, password)),
    [saveSession],
  )

  const logout = useCallback(() => {
    tokenStore.clear()
    localStorage.removeItem(USER_KEY)
    setUser(null)
  }, [])

  const value = useMemo(
    () => ({ user, isAdmin: user?.role === 'Admin', login, register, logout }),
    [user, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
