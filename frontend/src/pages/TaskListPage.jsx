import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { tasksApi } from '../api/client'

export default function TaskListPage() {
  const [tasks, setTasks] = useState([])
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    tasksApi
      .list()
      .then((data) => !cancelled && setTasks(data))
      .catch((err) => !cancelled && setError(err.message))
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <section>
      <h1>Tasks</h1>
      {/* TODO: filters by status, priority, category and assignee. */}
      {error && <p className="error">{error}</p>}
      {tasks.length === 0 && !error ? (
        <p className="muted">No tasks yet. <Link to="/tasks/new">Create one</Link>.</p>
      ) : (
        <ul className="task-list">
          {tasks.map((task) => (
            <li key={task.id} className="card">
              <Link to={`/tasks/${task.id}`}>{task.title}</Link>
              <span className="muted"> · {task.status} · {task.priority}</span>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
