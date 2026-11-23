import { Aviso } from './Aviso'
import { MensajeErrorCampo } from './MensajeErrorCampo'
import type { AlertaErrorServidorProps } from '../types/errors.types'

/**
 * Banner de rechazo del backend + lista opcional por campo.
 * Usa Aviso tipo error del design system.
 */
export function AlertaErrorServidor({
  mensaje,
  erroresPorCampo,
  titulo = 'Respuesta del servidor',
  onCerrar,
  className = '',
}: AlertaErrorServidorProps) {
  if (!mensaje && (!erroresPorCampo || Object.keys(erroresPorCampo).length === 0)) {
    return null
  }

  const campos = erroresPorCampo
    ? Object.entries(erroresPorCampo).flatMap(([campo, msgs]) =>
        msgs.map((m) => ({ campo, mensaje: m })),
      )
    : []

  return (
    <div className={className}>
      {mensaje ? (
        <Aviso tipo="error" titulo={titulo}>
          <div className="flex items-start justify-between gap-3">
            <span>{mensaje}</span>
            {onCerrar ? (
              <button
                type="button"
                onClick={onCerrar}
                className="shrink-0 text-sm font-medium underline-offset-2 hover:underline"
              >
                Cerrar
              </button>
            ) : null}
          </div>
        </Aviso>
      ) : null}

      {campos.length > 0 ? (
        <ul className="mt-2 space-y-1">
          {campos.map(({ campo, mensaje: msg }) => (
            <li key={`${campo}-${msg}`}>
              <MensajeErrorCampo mensaje={`${campo}: ${msg}`} />
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  )
}
