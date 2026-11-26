import { Aviso } from '@/shared/components/ui/Aviso'
import type { Mensaje } from '../useCambioEstado'

/** El aviso de la última acción (éxito anunciado como estado; el error, como alerta). */
export function AvisoMensaje({ mensaje }: { mensaje: Mensaje | undefined }) {
  return (
    <>
      <div role="status" className="contents">
        {mensaje?.tipo === 'exito' && <Aviso tipo="exito">{mensaje.texto}</Aviso>}
      </div>
      {mensaje?.tipo === 'error' && <Aviso tipo="error">{mensaje.texto}</Aviso>}
    </>
  )
}
