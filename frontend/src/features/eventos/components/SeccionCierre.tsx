import { useState } from 'react'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { DialogoConfirmacion } from '@/shared/components/ui/DialogoConfirmacion'
import { extraerError } from '@/shared/api/errores'
import { cerrarEvento, descartarEvento } from '../api/eventos'
import type { EventoDetalle } from '../types/eventos'

interface Props {
  evento: EventoDetalle
  onCambio: (evento: EventoDetalle, mensaje: string) => void
}

/**
 * HU-049: cierre y descarte de un evento abierto o en investigación (solo el Responsable SST).
 * - Un evento no se cierra sin causas y medidas correctivas (RN-28): el botón explica qué falta.
 * - Un reporte falso, duplicado o inválido se descarta con motivo opcional, sin investigarlo.
 */
export function SeccionCierre({ evento, onCambio }: Props) {
  const [dialogo, setDialogo] = useState<'cerrar' | 'descartar'>()
  const [motivo, setMotivo] = useState('')
  const [ocupado, setOcupado] = useState(false)
  const [error, setError] = useState<string>()
  const falta = [
    !evento.investigacion && 'registrar la investigación',
    evento.totalMedidas === 0 && 'agregar al menos una medida correctiva',
  ].filter(Boolean)

  const ejecutar = async (accion: () => Promise<EventoDetalle>, mensaje: string) => {
    setOcupado(true)
    setError(undefined)
    try {
      const e = await accion()
      setDialogo(undefined)
      onCambio(e, mensaje)
    } catch (causa) {
      setDialogo(undefined)
      setError(extraerError(causa).mensaje)
    } finally {
      setOcupado(false)
    }
  }

  return (
    <section aria-labelledby="titulo-cierre" className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <header className="grid gap-1">
        <h2 id="titulo-cierre" className="text-md font-semibold">
          Cierre
        </h2>
        <p className="text-text-secondary">
          Se cierra con su investigación y al menos una medida correctiva. Las medidas siguen en el plan de trabajo hasta completarse.
        </p>
      </header>
      {error && <Aviso tipo="error">{error}</Aviso>}
      {falta.length > 0 && <p className="text-sm text-text-secondary">{`Para cerrarlo falta ${falta.join(' y ')}.`}</p>}
      <div className="flex flex-col items-stretch gap-3 sm:flex-row sm:items-center">
        <Boton disabled={falta.length > 0} onClick={() => setDialogo('cerrar')}>
          Cerrar el evento
        </Boton>
        <Boton variante="secundario" onClick={() => setDialogo('descartar')}>
          Descartar el reporte
        </Boton>
      </div>

      <DialogoConfirmacion
        abierto={dialogo === 'cerrar'}
        titulo="¿Cerrar el evento?"
        textoConfirmar="Cerrar el evento"
        textoCargando="Cerrando…"
        cargando={ocupado}
        onConfirmar={() => void ejecutar(() => cerrarEvento(evento.id), 'El evento quedó cerrado.')}
        onCancelar={() => setDialogo(undefined)}
      >
        Ya no se podrá editar la investigación ni agregar medidas. Las medidas siguen en el plan de trabajo hasta completarse.
      </DialogoConfirmacion>
      <DialogoConfirmacion
        abierto={dialogo === 'descartar'}
        titulo="¿Descartar el reporte?"
        textoConfirmar="Descartar el reporte"
        textoCargando="Descartando…"
        peligro
        cargando={ocupado}
        onConfirmar={() => void ejecutar(() => descartarEvento(evento.id, motivo), 'El reporte quedó descartado.')}
        onCancelar={() => setDialogo(undefined)}
      >
        <div className="grid gap-4">
          <p>Para reportes falsos, duplicados o inválidos. No se investiga y queda fuera de las estadísticas.</p>
          <AreaTexto
            etiqueta="Motivo"
            ayuda="Opcional. Por ejemplo: duplicado del reporte del lunes."
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
          />
        </div>
      </DialogoConfirmacion>
    </section>
  )
}
