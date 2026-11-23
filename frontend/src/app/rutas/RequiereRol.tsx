import { Outlet } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { useTieneRol, type Rol } from '@/shared/sesion'

/**
 * Rutas solo para ciertos roles; va dentro de RutaPrivada. Sin el rol se muestra lo mismo que
 * una ruta inexistente, para no confirmar que existe (design-system.md § Estados de pantalla).
 * El backend vuelve a verificar el permiso en cada petición.
 */
export function RequiereRol({ roles }: { roles: readonly Rol[] }) {
  return useTieneRol(roles) ? <Outlet /> : <NotFoundPage />
}
