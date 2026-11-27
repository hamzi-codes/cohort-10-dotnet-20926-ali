import { useAuth } from '../auth/useAuth'

export default function ProfilePage() {
  const { user } = useAuth()
  return (
    <section>
      <h1>Profile</h1>
      <dl className="card profile">
        <dt>Name</dt>
        <dd>{user?.fullName}</dd>
        <dt>Email</dt>
        <dd>{user?.email}</dd>
        <dt>Role</dt>
        <dd>{user?.role}</dd>
      </dl>
    </section>
  )
}
