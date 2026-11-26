import { Link2Off } from 'lucide-react'
import { useState } from 'react'
import { useWatch } from 'react-hook-form'
import { Link, useSearchParams } from 'react-router-dom'
import { extraerError, renovarSesion, sesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { RequisitosContrasena } from '@/shared/components/ui/RequisitosContrasena'
import { useFormulario } from '@/shared/forms'
import { Volver } from '@/shared/components/ui/Volver'
import { leerUltimaEmpresa, rutaLogin, useSesion } from '@/shared/sesion'
import { confirmarCambioContrasena } from '../api/contrasena'
import { restablecerContrasenaEsquema } from '../schemas/cambioContrasena'

/**
 * HU-022: el enlace del correo trae ?token=. Aquí se elige la contraseña nueva (política de HU-114).
 * Al guardarla, el backend cierra todas las sesiones de la cuenta, en todas sus empresas. Si en este
 * navegador había una sesión de esa cuenta, también se cierra aquí.
 */
export function RestablecerContrasenaPage() {
  const [parametros] = useSearchParams()
  const conSesion = useSesion().estado === 'activa'
  const token = parametros.get('token')?.trim() ?? ''
  const [estado, setEstado] = useState<'formulario' | 'cambiada' | 'enlace_invalido'>(token ? 'formulario' : 'enlace_invalido')

  const {
    register,
    control,
    trigger,
    getFieldState,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(restablecerContrasenaEsquema, {
    defaultValues: { contrasenaNueva: '', contrasenaNuevaRepetida: '' },
    erroresPorCodigo: {
      'auth.contrasena_igual_actual': { campo: 'contrasenaNueva', mensaje: 'La contraseña nueva debe ser distinta de la actual.' },
    },
  })
  const nueva = useWatch({ control, name: 'contrasenaNueva' }) ?? ''

  const volver = conSesion ? (
    <Volver a="/inicio" texto="Volver al inicio" titulo="Elige tu contraseña nueva" />
  ) : (
    <Volver a="/" texto="Volver a la página principal" titulo="Elige tu contraseña nueva" />
  )

  if (estado === 'enlace_invalido') {
    return (
      <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
        {volver}
        <section className="flex flex-col items-center gap-3 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <Link2Off aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h1 className="text-md font-semibold">El enlace ya no sirve</h1>
          <p className="max-w-layout-narrow text-text-secondary">
            Venció, ya se usó o está incompleto. Cada enlace dura 30 minutos y sirve una sola vez.
          </p>
          {/* Con sesión, el login (y /recuperar-contrasena) no se ofrecen: se pide desde la cuenta. */}
          <Link to={conSesion ? '/cuenta/contrasena' : '/recuperar-contrasena'} className={clasesBoton('primario')}>
            Pedir otro enlace
          </Link>
        </section>
      </div>
    )
  }

  if (estado === 'cambiada') {
    return (
      <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
        <Volver a="/" texto="Volver a la página principal" titulo="Contraseña cambiada" />
        <h1 className="text-xl font-semibold">Contraseña cambiada</h1>
        <Aviso tipo="exito" titulo="Tu contraseña nueva ya está activa">
          Vale en todas las empresas donde tienes acceso. Por seguridad cerramos tus sesiones abiertas: vuelve a iniciar
          sesión con la contraseña nueva.
        </Aviso>
        <Link to={rutaLogin(leerUltimaEmpresa()?.idEmpresa)} className={`${clasesBoton('primario')} justify-self-start`}>
          Iniciar sesión
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      {volver}
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Elige tu contraseña nueva</h1>
        <p className="text-text-secondary">Valdrá en todas las empresas donde tienes acceso.</p>
      </header>

      <form
        noValidate
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async ({ contrasenaNueva }) => {
          try {
            await confirmarCambioContrasena({ token, contrasenaNueva })
          } catch (causa) {
            if (extraerError(causa).codigo === 'auth.enlace_invalido') {
              setEstado('enlace_invalido')
              return
            }
            throw causa
          }
          // Si este navegador tenía una sesión de esta cuenta, el backend ya la cerró: se confirma al renovar.
          if (sesion.obtenerUsuario() && (await renovarSesion()) === 'rechazada') sesion.terminar('contrasena_cambiada')
          setEstado('cambiada')
        })}
      >
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <CampoTexto
          etiqueta="Contraseña nueva"
          required
          type="password"
          autoComplete="new-password"
          registro={register('contrasenaNueva', {
            // La confirmación se vuelve a comparar si ya se había escrito.
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
        <Boton type="submit" className="justify-self-start" cargando={isSubmitting} textoCargando="Guardando…">
          Guardar contraseña
        </Boton>
      </form>
    </div>
  )
}
