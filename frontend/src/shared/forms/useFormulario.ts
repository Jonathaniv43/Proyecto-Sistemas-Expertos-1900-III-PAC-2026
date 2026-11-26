import { zodResolver } from '@hookform/resolvers/zod'
import { useCallback, useState } from 'react'
import { useForm, type FieldValues, type Path, type UseFormProps } from 'react-hook-form'
import type { z } from 'zod'
import { extraerError, type ErrorApi } from '@/shared/api/errores'

/** Qué hacer con un error de negocio del backend según su `codigo` (p. ej. "empresas.rtn_duplicado"). */
export type ErrorPorCodigo<TEntrada extends FieldValues> = {
  /** Campo donde se muestra; sin campo, va al aviso general. */
  campo?: Path<TEntrada>
  /** Mensaje propio; si no, el `title` del backend. */
  mensaje?: string
}

type Opciones<TEntrada extends FieldValues, TSalida extends FieldValues> = Omit<
  UseFormProps<TEntrada, unknown, TSalida>,
  'resolver' | 'mode'
> & {
  erroresPorCodigo?: Record<string, ErrorPorCodigo<TEntrada>>
}

/**
 * useForm con las convenciones de Norvia (16-arquitectura §9):
 * - el esquema Zod es la única fuente de reglas (espejo del validador del backend);
 * - `mode: 'onChange'`: el error aparece mientras se escribe, antes de enviar;
 * - `enviar(fn)` llama a la API con los datos ya validados y, si falla, reparte el error:
 *   los errores por campo del backend van a su campo y el resto queda en `errorGeneral`;
 * - `erroresPorCodigo` lleva un error de negocio (409, etc.) a su campo: "RTN repetido" junto al RTN.
 */
export function useFormulario<TEsquema extends z.ZodType<FieldValues, FieldValues>>(
  esquema: TEsquema,
  opciones?: Opciones<z.input<TEsquema>, z.output<TEsquema>>,
) {
  type Entrada = z.input<TEsquema>
  type Salida = z.output<TEsquema>

  const { erroresPorCodigo, ...opcionesForm } = opciones ?? {}
  const form = useForm<Entrada, unknown, Salida>({
    ...opcionesForm,
    mode: 'onChange',
    resolver: zodResolver(esquema),
  })
  const [errorGeneral, setErrorGeneral] = useState<string>()

  const { setError, getValues, handleSubmit } = form

  /** Coloca un ErrorApi en el formulario. Devuelve true si hubo errores por campo. */
  const aplicarErrorApi = useCallback(
    (error: ErrorApi) => {
      if (error.cancelada) return false

      const porCodigo = error.codigo ? erroresPorCodigo?.[error.codigo] : undefined
      if (porCodigo) {
        const mensaje = porCodigo.mensaje ?? error.mensaje
        if (porCodigo.campo) {
          setError(porCodigo.campo, { type: 'servidor', message: mensaje }, { shouldFocus: true })
          setErrorGeneral(undefined)
          return true
        }
        setErrorGeneral(mensaje)
        return false
      }

      const sinCampo: string[] = []
      let hayCampos = false
      for (const [ruta, mensajes] of Object.entries(error.campos)) {
        // Solo se marca el campo si existe en el formulario. Las reglas de FluentValidation sobre
        // el objeto entero llegan con ruta vacía: van al mensaje general, igual que un campo desconocido.
        if (!ruta || getValues(ruta as Path<Entrada>) === undefined) {
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
    [erroresPorCodigo, getValues, setError],
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
