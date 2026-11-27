import { useParams } from 'react-router-dom'

export default function TaskDetailPage() {
  const { id } = useParams()
  return (
    <section>
      <h1>Task #{id}</h1>
      {/* TODO: load task details, allow status change, edit and delete. */}
      <p className="muted">Task details will appear here.</p>
    </section>
  )
}
