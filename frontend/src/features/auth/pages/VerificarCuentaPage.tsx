import { useState } from 'react'
import { useWatch } from 'react-hook-form'
import { Link, useSearchParams } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { RequisitosContrasena } from '@/shared/components/ui/RequisitosContrasena'
import { Volver } from '@/shared/components/ui/Volver'
import { useFormulario } from '@/shared/forms'
import { rutaLogin } from '@/shared/sesion'
import { verificarCuenta } from '../api/verificarCuenta'
import { verificarCuentaEsquema } from '../schemas/verificarCuenta'

/**
 * Activa la cuenta creada en el registro (HU-017) o por un administrador: con el correo y la
 * contraseña temporal recibidos, la persona elige su contraseña definitiva (HU-114).
 * El enlace del correo apunta aquí; ?correo= rellena el correo y ?empresa= (id cifrado) se pasa al login.
 */
export function VerificarCuentaPage() {
  const [parametros] = useSearchParams()
  const [verificada, setVerificada] = useState(false)

  const {
    register,
    control,
    trigger,
    getFieldState,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(verificarCuentaEsquema, {
    defaultValues: {
      email: parametros.get('correo') ?? '',
      contrasenaTemporal: '',
      contrasenaNueva: '',
      contrasenaNuevaRepetida: '',
    },
    erroresPorCodigo: {
      'auth.credenciales_invalidas': {
        mensaje: 'El correo o la contraseña temporal no son correctos. Revisa el correo que te enviamos.',
      },
      'auth.cuenta_ya_verificada': { mensaje: 'Esta cuenta ya está activada. Puedes iniciar sesión.' },
    },
  })

  const nueva = useWatch({ control, name: 'contrasenaNueva' }) ?? ''

  if (verificada) {
    return (
      <div className="mx-auto grid max-w-layout-narrow gap-6">
        <Volver a={rutaLogin(parametros.get('empresa'))} texto="Volver a iniciar sesión" titulo="Cuenta activada" />
        <h1 className="text-xl font-semibold">Cuenta activada</h1>
        <Aviso tipo="exito" titulo="Tu contraseña quedó guardada">
          Ya puedes iniciar sesión con tu correo y tu contraseña nueva.
        </Aviso>
        <Link
          to={rutaLogin(parametros.get('empresa'))}
          className={`${clasesBoton('primario')} justify-self-start`}
        >
          Iniciar sesión
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto grid max-w-layout-narrow gap-6">
      <Volver a={rutaLogin(parametros.get('empresa'))} texto="Volver a iniciar sesión" titulo="Activa tu cuenta" />
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Activa tu cuenta</h1>
        <p className="text-text-secondary">
          Escribe la contraseña temporal que te enviamos por correo y elige tu contraseña definitiva.
        </p>
      </header>

      <form
        noValidate
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async (datos) => {
          await verificarCuenta(datos)
          setVerificada(true)
        })}
      >
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <CampoTexto
          etiqueta="Correo"
          required
          type="email"
          autoComplete="username"
          registro={register('email')}
          error={errors.email}
        />
        <CampoTexto
          etiqueta="Contraseña temporal"
          required
          type="password"
          autoComplete="one-time-code"
          registro={register('contrasenaTemporal')}
          error={errors.contrasenaTemporal}
        />
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
        <Boton type="submit" className="justify-self-start" cargando={isSubmitting} textoCargando="Activando…">
          Activar cuenta
        </Boton>
      </form>
    </div>
  )
}
