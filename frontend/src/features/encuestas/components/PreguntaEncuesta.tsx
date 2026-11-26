import { Check } from 'lucide-react'
import { useId } from 'react'
import type { UseFormRegisterReturn } from 'react-hook-form'
import { MensajeCampo } from '@/shared/components/ui/MensajeCampo'
import { Contador } from '@/shared/components/ui/Contador'
import type { PreguntaPreview } from '../types/encuestas'

/** Controles para responder (HU-031). Sin ellos, la pregunta es de solo lectura (vista previa). */
export interface RespuestaControlada {
  /** respuesta_breve y seleccion_unica: `register(...)` de React Hook Form. */
  registro?: UseFormRegisterReturn
  /** seleccion_multiple: ids marcados y cambio (con `useController`). */
  marcadas?: readonly string[]
  onMarcadas?: (ids: string[]) => void
  /** respuesta_breve: texto actual, para el contador. */
  texto?: string
  maxCaracteres?: number | null
  error?: string
}

interface PreguntaEncuestaProps {
  numero: number
  pregunta: PreguntaPreview
  respuesta?: RespuestaControlada
}

/**
 * Bloque de una pregunta (design-system.md § Pregunta de encuesta): enunciado numerado, * si es
 * obligatoria, la indicación como ayuda y el control según el tipo (respuesta_breve → área de texto,
 * seleccion_unica → radios, seleccion_multiple → casillas). El error va una vez, debajo del grupo.
 *
 * Sin `respuesta` es la vista previa (HU-029/HU-030): controles deshabilitados.
 * Con `respuesta` se responde (HU-031).
 */
export function PreguntaEncuesta({ numero, pregunta, respuesta }: PreguntaEncuestaProps) {
  const id = useId()
  const idAyuda = `${id}-ayuda`
  const idMensaje = `${id}-mensaje`
  const soloLectura = !respuesta
  const opciones = [...pregunta.opciones].sort((a, b) => a.orden - b.orden)
  const error = respuesta?.error
  const describe = [pregunta.instruccionUi ? idAyuda : null, error ? idMensaje : null].filter(Boolean).join(' ') || undefined

  return (
    <fieldset className="grid gap-3" aria-describedby={describe} aria-invalid={error ? true : undefined}>
      <legend className="contents">
        <span className="text-md font-semibold text-text-primary">
          {numero}. {pregunta.texto}
          {pregunta.obligatoria && (
            <>
              <span aria-hidden className="text-error-text">
                {' *'}
              </span>
              <span className="sr-only"> (obligatoria)</span>
            </>
          )}
        </span>
      </legend>
      {pregunta.instruccionUi && (
        <p id={idAyuda} className="text-text-secondary">
          {pregunta.instruccionUi}
        </p>
      )}

      {pregunta.tipoRespuestaCodigo === 'respuesta_breve' ? (
        <AreaRespuesta numero={numero} soloLectura={soloLectura} respuesta={respuesta} />
      ) : (
        <div className="grid justify-items-start sm:gap-3">
          {opciones.map((opcion) => {
            const valor = opcion.id ?? String(opcion.orden)
            if (pregunta.tipoRespuestaCodigo === 'seleccion_multiple') {
              const marcadas = respuesta?.marcadas ?? []
              return (
                <label
                  key={valor}
                  className={`casilla min-h-control-touch sm:min-h-auto ${soloLectura ? 'text-text-secondary' : ''}`}
                >
                  <input
                    type="checkbox"
                    disabled={soloLectura}
                    value={valor}
                    checked={soloLectura ? false : marcadas.includes(valor)}
                    onChange={(e) =>
                      respuesta?.onMarcadas?.(e.target.checked ? [...marcadas, valor] : marcadas.filter((m) => m !== valor))
                    }
                  />
                  <span aria-hidden className="casilla-caja">
                    <Check strokeWidth={2.5} />
                  </span>
                  {opcion.texto}
                </label>
              )
            }
            return (
              <label key={valor} className={`radio min-h-control-touch sm:min-h-auto ${soloLectura ? 'text-text-secondary' : ''}`}>
                {soloLectura ? <input type="radio" name={id} disabled /> : <input type="radio" value={valor} {...respuesta?.registro} />}
                <span aria-hidden className="radio-circulo" />
                {opcion.texto}
              </label>
            )
          })}
        </div>
      )}
      <MensajeCampo id={idMensaje} error={error} />
    </fieldset>
  )
}

function AreaRespuesta({ numero, soloLectura, respuesta }: { numero: number; soloLectura: boolean; respuesta?: RespuestaControlada }) {
  if (soloLectura) {
    return (
      <textarea
        disabled
        aria-label={`Respuesta a la pregunta ${numero}`}
        placeholder="Aquí escribirá su respuesta"
        className="area-texto w-full rounded-md border border-border-default bg-bg-disabled text-md text-text-disabled sm:text-sm"
      />
    )
  }
  const largo = respuesta?.texto?.length ?? 0
  const maximo = respuesta?.maxCaracteres ?? null
  return (
    <div className="grid gap-1">
      <textarea
        aria-label={`Respuesta a la pregunta ${numero}`}
        className={[
          'area-texto w-full rounded-md border bg-bg-surface text-md text-text-primary transition-[border-color,box-shadow] sm:text-sm',
          'focus-visible:outline-none focus:ring-3',
          respuesta?.error
            ? 'border-error-icon focus:ring-error-bg'
            : 'border-border-input hover:border-border-input-hover focus:border-focus-ring focus:ring-focus-halo',
        ].join(' ')}
        {...respuesta?.registro}
      />
      {maximo !== null && (
        <span className="justify-self-end">
          <Contador actual={largo} limite={maximo} />
        </span>
      )}
    </div>
  )
}
