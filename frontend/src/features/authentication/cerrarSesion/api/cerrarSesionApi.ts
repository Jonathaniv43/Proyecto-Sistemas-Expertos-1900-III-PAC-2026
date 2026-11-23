import { authClient } from '../../../../shared/api/authClient'

export async function postCerrarSesion(): Promise<void> {
  await authClient.post('/auth/logout', {})
}
