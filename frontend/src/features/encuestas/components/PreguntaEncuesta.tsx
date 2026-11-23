import { Check } from 'lucide-react'
import { useId } from 'react'
import type { PreguntaPreview } from '../types/encuestas'

interface PreguntaEncuestaProps {
  numero: number
  pregunta: PreguntaPreview
}

/**
 * Bloque de una pregunta (design-system.md § Pregunta de encuesta): enunciado numerado, * si es
 * obligatoria, la indicación como ayuda y el control según el tipo (respuesta_breve → área de texto,
 * seleccion_unica → radios, seleccion_multiple → casillas).
 *
 * Por ahora es de solo lectura (vista previa: los controles están deshabilitados). Responder la
 * encuesta (HU-031) reutiliza esta misma estructura con los controles habilitados.
 */
export function PreguntaEncuesta({ numero, pregunta }: PreguntaEncuestaProps) {
  const id = useId()
  const idAyuda = `${id}-ayuda`
  const opciones = [...pregunta.opciones].sort((a, b) => a.orden - b.orden)

  return (
    <fieldset className="grid gap-3" aria-describedby={pregunta.instruccionUi ? idAyuda : undefined}>
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
        <textarea
          disabled
          aria-label={`Respuesta a la pregunta ${numero}`}
          placeholder="Aquí escribirá su respuesta"
          className="area-texto w-full rounded-md border border-border-default bg-bg-disabled text-md text-text-disabled sm:text-sm"
        />
      ) : (
        <div className="grid justify-items-start sm:gap-3">
          {opciones.map((opcion) =>
            pregunta.tipoRespuestaCodigo === 'seleccion_multiple' ? (
              <label key={opcion.orden} className="casilla min-h-control-touch text-text-secondary sm:min-h-auto">
                <input type="checkbox" disabled />
                <span aria-hidden className="casilla-caja">
                  <Check strokeWidth={2.5} />
                </span>
                {opcion.texto}
              </label>
            ) : (
              <label key={opcion.orden} className="radio min-h-control-touch text-text-secondary sm:min-h-auto">
                <input type="radio" name={id} disabled />
                <span aria-hidden className="radio-circulo" />
                {opcion.texto}
              </label>
            ),
          )}
        </div>
      )}
    </fieldset>
  )
}
