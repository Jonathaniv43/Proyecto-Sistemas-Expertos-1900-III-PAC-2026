import { useSyncExternalStore } from 'react'
import { estadoSesion } from './estadoSesion'
import type { Rol } from './roles'

/** Estado de la sesión, actualizado al iniciar, renovar o terminar. */
export function useSesion() {
  return useSyncExternalStore(estadoSesion.suscribir, estadoSesion.obtener)
}

/** true si hay sesión activa y su rol es uno de `roles`. Solo decide qué mostrar; autoriza el backend. */
export function useTieneRol(roles: readonly Rol[]) {
  const actual = useSesion()
  return actual.estado === 'activa' && actual.usuario.rol !== null && roles.includes(actual.usuario.rol as Rol)
}
