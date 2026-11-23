import { ArrowDown, ArrowUp, Check, Plus, Trash2, X } from 'lucide-react'
import type { InputHTMLAttributes, ReactNode } from 'react'
import { useFieldArray, useWatch, type Control, type FieldErrors, type UseFormRegister, type UseFormSetValue } from 'react-hook-form'
import { BotonIcono } from '@/shared/components/ui/BotonIcono'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { MensajeCampo } from '@/shared/components/ui/MensajeCampo'
import { MAXIMO_OPCIONES, type BorradorFormulario, type BorradorValidado } from '../schemas/borrador'
import type { TipoRespuestaCatalogo } from '../types/encuestas'

interface EditorPreguntaProps {
  indice: number
  numero: number
  control: Control<BorradorFormulario, unknown, BorradorValidado>
  register: UseFormRegister<BorradorFormulario>
  setValue: UseFormSetValue<BorradorFormulario>
  errores?: NonNullable<FieldErrors<BorradorFormulario>['preguntas']>[number]
  tipos: TipoRespuestaCatalogo[]
  puedeSubir: boolean
  puedeBajar: boolean
  onSubir: () => void
  onBajar: () => void
  onEliminar: () => void
}

function Casilla({ children, ...props }: InputHTMLAttributes<HTMLInputElement> & { children: ReactNode }) {
  return (
    <label className="casilla min-h-control-touch justify-self-start text-sm sm:min-h-auto">
      <input type="checkbox" {...props} />
      <span aria-hidden className="casilla-caja">
        <Check strokeWidth={2.5} />
      </span>
      {children}
    </label>
  )
}

/**
 * Edición de una pregunta del borrador: texto, tipo, si es obligatoria y, en las de selección, sus
 * opciones. Si la pregunta viene del catálogo con un peligro asociado (HU-033), cada opción puede
 * marcar si indica exposición a ese peligro.
 */
export function EditorPregunta(props: EditorPreguntaProps) {
  const { indice, numero, control, register, setValue, errores, tipos } = props
  const base = `preguntas.${indice}` as const
  const tipo = useWatch({ control, name: `${base}.tipoRespuestaCodigo` })
  const peligroId = useWatch({ control, name: `${base}.peligroId` })
  const opciones = useFieldArray({ control, name: `${base}.opciones`, keyName: 'clave' })
  const esSeleccion = tipo !== 'respuesta_breve'
  const errorOpciones = errores?.opciones?.root?.message ?? errores?.opciones?.message

  const cambiarTipo = (nuevo: string) => {
    if (nuevo === 'respuesta_breve') {
      // Una respuesta breve no lleva opciones ni peligro (reglas del backend).
      setValue(`${base}.opciones`, [], { shouldDirty: true })
      setValue(`${base}.peligroId`, null, { shouldDirty: true })
    } else if (opciones.fields.length === 0) {
      setValue(
        `${base}.opciones`,
        [
          { id: null, texto: '', indicaExposicion: false },
          { id: null, texto: '', indicaExposicion: false },
        ],
        { shouldDirty: true },
      )
    }
  }

  return (
    <div className="grid gap-4 rounded-lg border border-border-default p-4">
      <div className="flex items-center justify-between gap-2">
        <span className="text-sm font-semibold">Pregunta {numero}</span>
        <div className="flex items-center gap-1">
          <BotonIcono etiqueta={`Subir la pregunta ${numero}`} disabled={!props.puedeSubir} onClick={props.onSubir}>
            <ArrowUp size={20} strokeWidth={1.5} />
          </BotonIcono>
          <BotonIcono etiqueta={`Bajar la pregunta ${numero}`} disabled={!props.puedeBajar} onClick={props.onBajar}>
            <ArrowDown size={20} strokeWidth={1.5} />
          </BotonIcono>
          <BotonIcono etiqueta={`Eliminar la pregunta ${numero}`} onClick={props.onEliminar}>
            <Trash2 size={20} strokeWidth={1.5} />
          </BotonIcono>
        </div>
      </div>

      <CampoTexto etiqueta="Texto de la pregunta" required autoComplete="off" registro={register(`${base}.texto`)} error={errores?.texto} />

      <div className="grid gap-3">
        <CampoSelect
          etiqueta="Tipo de respuesta"
          required
          registro={register(`${base}.tipoRespuestaCodigo`, { onChange: (e) => cambiarTipo(e.target.value) })}
          opciones={tipos.map((t) => ({ valor: t.codigo, texto: t.nombre }))}
          error={errores?.tipoRespuestaCodigo}
          ayuda={peligroId && esSeleccion ? 'Si la cambias a respuesta breve, se quita el peligro asociado.' : undefined}
        />
        <Casilla {...register(`${base}.obligatoria`)}>Obligatoria: no se puede enviar la encuesta sin responderla</Casilla>
      </div>

      {esSeleccion && (
        <fieldset className="grid gap-2" aria-invalid={errorOpciones ? true : undefined}>
          <legend className="mb-2 text-sm font-medium">
            Opciones
            {peligroId && <span className="font-regular text-text-secondary"> · marca las que indican exposición al peligro</span>}
          </legend>
          {opciones.fields.map((opcion, j) => (
            <div key={opcion.clave} className="flex items-start gap-2">
              <span aria-hidden className="flex h-control-touch w-6 shrink-0 items-center text-text-secondary tabular-nums sm:h-control">
                {j + 1}.
              </span>
              <div className="grid flex-1 gap-2">
                <CampoTexto
                  etiqueta={`Opción ${j + 1} de la pregunta ${numero}`}
                  etiquetaOculta
                  required
                  autoComplete="off"
                  registro={register(`${base}.opciones.${j}.texto`)}
                  error={errores?.opciones?.[j]?.texto}
                />
                {peligroId && (
                  <Casilla {...register(`${base}.opciones.${j}.indicaExposicion`)}>Indica exposición al peligro</Casilla>
                )}
              </div>
              <BotonIcono
                etiqueta={`Quitar la opción ${j + 1} de la pregunta ${numero}`}
                disabled={opciones.fields.length <= 2}
                onClick={() => opciones.remove(j)}
              >
                <X size={20} strokeWidth={1.5} />
              </BotonIcono>
            </div>
          ))}
          <MensajeCampo id={`${base}-opciones-mensaje`} error={errorOpciones} />
          <div>
            <button
              type="button"
              className="inline-flex min-h-control-touch items-center gap-2 text-sm font-medium text-text-link hover:text-text-link-hover disabled:cursor-not-allowed disabled:text-text-disabled sm:min-h-control"
              disabled={opciones.fields.length >= MAXIMO_OPCIONES}
              onClick={() => opciones.append({ id: null, texto: '', indicaExposicion: false })}
            >
              <Plus aria-hidden size={16} strokeWidth={1.5} />
              Agregar opción
            </button>
          </div>
        </fieldset>
      )}
    </div>
  )
}
