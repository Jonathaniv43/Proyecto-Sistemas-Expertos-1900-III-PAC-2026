import { renovarAccessToken } from '../../refrescarSesion/lib/renovarAccessToken'
import { sesionStore } from '../store/sesionStore'
import type { InterceptoresSesion } from '../../../../shared/api/types/interceptoresSesion.types'

/** Lo que main.tsx entrega a shared. shared no importa este feature. */
export function proveerInterceptoresSesion(): InterceptoresSesion {
  return {
    obtenerToken: () => sesionStore.obtenerAccessToken(),
    renovar: () => renovarAccessToken(),
  }
}
