import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <span className="brand">Task Manager</span>
        <nav>
          <NavLink to="/dashboard">Dashboard</NavLink>
          <NavLink to="/tasks" end>Tasks</NavLink>
          <NavLink to="/tasks/new">New task</NavLink>
          <NavLink to="/profile">Profile</NavLink>
        </nav>
        <span className="user">
          {user?.fullName} ({user?.role})
          <button type="button" onClick={handleLogout}>Log out</button>
        </span>
      </header>
      <main className="content">
        <Outlet />
      </main>
    </div>
  )
}
