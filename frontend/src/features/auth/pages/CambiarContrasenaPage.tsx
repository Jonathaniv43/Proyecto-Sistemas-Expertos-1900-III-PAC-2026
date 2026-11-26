import { useWatch } from 'react-hook-form'
import { sesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { RequisitosContrasena } from '@/shared/components/ui/RequisitosContrasena'
import { useFormulario } from '@/shared/forms'
import { cambiarContrasena } from '../api/sesion'
import { cambiarContrasenaEsquema } from '../schemas/login'

/**
 * Primer ingreso con contraseña temporal (HU-114, HU-022): hay que elegir una contraseña propia
 * antes de usar Norvia. Al cambiarla, el backend cierra todas las sesiones y se vuelve a ingresar.
 */
export function CambiarContrasenaPage() {
  const {
    register,
    control,
    trigger,
    getFieldState,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(cambiarContrasenaEsquema, {
    defaultValues: { contrasenaActual: '', contrasenaNueva: '', contrasenaNuevaRepetida: '' },
    erroresPorCodigo: { 'auth.contrasena_actual_incorrecta': { campo: 'contrasenaActual' } },
  })
  const nueva = useWatch({ control, name: 'contrasenaNueva' }) ?? ''

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Elige tu contraseña</h1>
        <p className="text-text-secondary">
          Entraste con una contraseña temporal. Antes de continuar, cámbiala por una que solo tú conozcas.
        </p>
      </header>
      <form
        noValidate
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async ({ contrasenaActual, contrasenaNueva }) => {
          await cambiarContrasena({ contrasenaActual, contrasenaNueva })
          sesion.terminar('contrasena_cambiada')
        })}
      >
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <CampoTexto
          etiqueta="Contraseña temporal"
          required
          type="password"
          autoComplete="current-password"
          registro={register('contrasenaActual')}
          error={errors.contrasenaActual}
          ayuda="La que usaste para entrar."
        />
        <CampoTexto
          etiqueta="Contraseña nueva"
          required
          type="password"
          autoComplete="new-password"
          registro={register('contrasenaNueva', {
            onChange: () => {
              if (getFieldState('contrasenaNuevaRepetida').isDirty) void trigger('contrasenaNuevaRepetida')
            },
          })}
          error={errors.contrasenaNueva}
        >
          <RequisitosContrasena valor={nueva} />
        </CampoTexto>
        <CampoTexto
          etiqueta="Repite la contraseña nueva"
          required
          type="password"
          autoComplete="new-password"
          registro={register('contrasenaNuevaRepetida')}
          error={errors.contrasenaNuevaRepetida}
        />
        <Boton type="submit" className="w-full sm:w-auto sm:justify-self-start" cargando={isSubmitting} textoCargando="Guardando…">
          Guardar contraseña
        </Boton>
      </form>
    </div>
  )
}
