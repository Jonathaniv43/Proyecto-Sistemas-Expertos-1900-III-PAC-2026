import { zodResolver } from '@hookform/resolvers/zod'
import { useCallback, useState } from 'react'
import { useForm, type FieldValues, type Path, type UseFormProps } from 'react-hook-form'
import type { z } from 'zod'
import { extraerError, type ErrorApi } from '@/shared/api/errores'

type Opciones<TEntrada extends FieldValues, TSalida extends FieldValues> = Omit<
  UseFormProps<TEntrada, unknown, TSalida>,
  'resolver' | 'mode'
>

/**
 * useForm con las convenciones de Norvia (16-arquitectura §9):
 * - el esquema Zod es la única fuente de reglas (espejo del validador del backend);
 * - `mode: 'onChange'`: el error aparece mientras se escribe, antes de enviar;
 * - `enviar(fn)` llama a la API con los datos ya validados y, si falla, reparte el error:
 *   los errores por campo del backend van a su campo y el resto queda en `errorGeneral`.
 */
export function useFormulario<TEsquema extends z.ZodType<FieldValues, FieldValues>>(
  esquema: TEsquema,
  opciones?: Opciones<z.input<TEsquema>, z.output<TEsquema>>,
) {
  type Entrada = z.input<TEsquema>
  type Salida = z.output<TEsquema>

  const form = useForm<Entrada, unknown, Salida>({
    ...opciones,
    mode: 'onChange',
    resolver: zodResolver(esquema),
  })
  const [errorGeneral, setErrorGeneral] = useState<string>()

  const { setError, getValues, handleSubmit } = form

  /** Coloca un ErrorApi en el formulario. Devuelve true si hubo errores por campo. */
  const aplicarErrorApi = useCallback(
    (error: ErrorApi) => {
      if (error.cancelada) return false
      const sinCampo: string[] = []
      let hayCampos = false

      for (const [ruta, mensajes] of Object.entries(error.campos)) {
        // Solo se marca el campo si existe en el formulario; si no, se suma al mensaje general.
        if (getValues(ruta as Path<Entrada>) === undefined) {
          sinCampo.push(...mensajes)
          continue
        }
        setError(ruta as Path<Entrada>, { type: 'servidor', message: mensajes[0] }, { shouldFocus: !hayCampos })
        hayCampos = true
      }

      setErrorGeneral(
        sinCampo.length > 0 ? sinCampo.join(' ') : hayCampos ? undefined : error.mensaje,
      )
      return hayCampos
    },
    [getValues, setError],
  )

  /** handleSubmit + manejo de errores de la API. Mientras corre, formState.isSubmitting es true. */
  const enviar = useCallback(
    (accion: (datos: Salida) => Promise<unknown>) =>
      handleSubmit(async (datos) => {
        setErrorGeneral(undefined)
        try {
          await accion(datos)
        } catch (causa) {
          aplicarErrorApi(extraerError(causa))
        }
      }),
    [handleSubmit, aplicarErrorApi],
  )

  return { ...form, enviar, errorGeneral, setErrorGeneral, aplicarErrorApi }
}
