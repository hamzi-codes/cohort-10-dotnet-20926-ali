import { useEffect, useState } from 'react'
import { tasksApi } from '../api/client'
import { useAuth } from '../auth/useAuth'

export default function DashboardPage() {
  const { isAdmin } = useAuth()
  const [summary, setSummary] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    tasksApi
      .summary()
      .then((data) => !cancelled && setSummary(data))
      .catch((err) => !cancelled && setError(err.message))
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <section>
      <h1>Dashboard</h1>
      <p className="muted">{isAdmin ? 'Showing all tasks (admin view).' : 'Showing your tasks.'}</p>
      {error && <p className="error">{error}</p>}
      <div className="stats">
        <Stat label="Pending" value={summary?.pending} />
        <Stat label="In progress" value={summary?.inProgress} />
        <Stat label="Completed" value={summary?.completed} />
        <Stat label="Total" value={summary?.total} />
      </div>
    </section>
  )
}

function Stat({ label, value }) {
  return (
    <div className="stat card">
      <span className="stat-value">{value ?? '–'}</span>
      <span className="stat-label">{label}</span>
    </div>
  )
}
