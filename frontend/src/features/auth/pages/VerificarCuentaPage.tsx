import { useState } from 'react'
import { useWatch } from 'react-hook-form'
import { Link, useSearchParams } from 'react-router-dom'
import { extraerError } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { RequisitosContrasena } from '@/shared/components/ui/RequisitosContrasena'
import { Volver } from '@/shared/components/ui/Volver'
import { useFormulario } from '@/shared/forms'
import { rutaLogin } from '@/shared/sesion'
import { reenviarVerificacionCuenta, verificarCuenta } from '../api/verificarCuenta'
import { verificarCuentaEsquema } from '../schemas/verificarCuenta'

/**
 * Activa la cuenta creada en el registro (HU-017) o por un administrador (HU-020, HU-115): con el
 * código de 6 dígitos recibido por correo (mismo mecanismo que Microsoft, RN-86), la persona elige
 * su contraseña definitiva (HU-114). El enlace del correo apunta aquí; ?correo= rellena el correo y
 * ?empresa= (id cifrado) se pasa al login.
 */
export function VerificarCuentaPage() {
  const [parametros] = useSearchParams()
  const [verificada, setVerificada] = useState(false)
  const [reenviando, setReenviando] = useState(false)
  const [reenviado, setReenviado] = useState(false)
  const [errorReenvio, setErrorReenvio] = useState<string>()

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
      codigo: '',
      contrasenaNueva: '',
      contrasenaNuevaRepetida: '',
    },
    erroresPorCodigo: {
      'auth.credenciales_invalidas': { mensaje: 'No encontramos una cuenta con ese correo.' },
      'auth.cuenta_ya_verificada': { mensaje: 'Esta cuenta ya está activada. Puedes iniciar sesión.' },
      'auth.codigo_incorrecto': { campo: 'codigo' },
      'auth.demasiados_intentos': { mensaje: 'Hiciste demasiados intentos. Pide un código nuevo.' },
    },
  })

  const nueva = useWatch({ control, name: 'contrasenaNueva' }) ?? ''
  const email = useWatch({ control, name: 'email' }) ?? ''

  async function pedirCodigoNuevo() {
    setErrorReenvio(undefined)
    setReenviando(true)
    try {
      await reenviarVerificacionCuenta(email)
      setReenviado(true)
    } catch (causa) {
      setErrorReenvio(extraerError(causa).mensaje)
    } finally {
      setReenviando(false)
    }
  }

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
          Escribe el código de 6 dígitos que te enviamos por correo y elige tu contraseña definitiva.
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
          etiqueta="Código de 6 dígitos"
          required
          inputMode="numeric"
          autoComplete="one-time-code"
          registro={register('codigo')}
          error={errors.codigo}
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

      <div className="grid gap-2 justify-items-start">
        {reenviado && <Aviso tipo="exito">Te enviamos un código nuevo a {email}.</Aviso>}
        {errorReenvio && <Aviso tipo="error">{errorReenvio}</Aviso>}
        <Boton
          variante="secundario"
          disabled={!email}
          cargando={reenviando}
          textoCargando="Enviando…"
          onClick={() => void pedirCodigoNuevo()}
        >
          ¿No te llegó o venció? Pide un código nuevo
        </Boton>
      </div>
    </div>
  )
}
