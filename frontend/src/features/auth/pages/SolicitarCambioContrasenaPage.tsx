import { MailCheck } from 'lucide-react'
import { useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { useFormulario } from '@/shared/forms'
import { Volver } from '@/shared/components/ui/Volver'
import { leerEstadoVolver } from '@/shared/components/ui/estadoVolver'
import { leerUltimaEmpresa, rutaLogin, useSesion } from '@/shared/sesion'
import { solicitarCambioContrasena } from '../api/contrasena'
import { solicitarCambioEsquema } from '../schemas/cambioContrasena'

/** Vigencia del enlace (SolicitarCambioContrasenaServicio.MinutosVigencia). */
const MINUTOS_VIGENCIA = 30

/**
 * HU-022: antes de cambiar la contraseña se confirma que la persona puede entrar a su correo: se le
 * envía un enlace y el cambio se hace desde ahí (/restablecer-contrasena).
 * - Con sesión (/cuenta/contrasena): el enlace va al correo de la cuenta, sin escribirlo.
 * - Sin sesión (/recuperar-contrasena, desde el login): se escribe el correo. La respuesta es la misma
 *   exista o no la cuenta, para no revelar qué correos están registrados.
 */
export function SolicitarCambioContrasenaPage() {
  const sesion = useSesion()
  const emailSesion = sesion.estado === 'activa' ? sesion.usuario.email : null
  const [enviadoA, setEnviadoA] = useState<string>()
  // Regreso: a donde se vino (p. ej. el login de la empresa) o, si se entró por URL, al padre fijo.
  const origen = leerEstadoVolver(useLocation().state)
  const padre = emailSesion
    ? { a: '/inicio', texto: 'Volver al inicio' }
    : { a: rutaLogin(leerUltimaEmpresa()?.idEmpresa), texto: 'Volver a iniciar sesión' }
  const regreso = origen ?? padre
  const titulo = emailSesion ? 'Cambiar mi contraseña' : '¿Olvidaste tu contraseña?'

  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(solicitarCambioEsquema, { defaultValues: { email: emailSesion ?? '' } })

  const pedir = enviar(async ({ email }) => {
    await solicitarCambioContrasena(email)
    setEnviadoA(email)
  })

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <Volver a={padre.a} texto={padre.texto} titulo={titulo} />
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">{titulo}</h1>
        <p className="text-text-secondary">
          Para proteger tu cuenta, te enviamos un enlace a tu correo y desde ahí eliges la contraseña nueva. Vale para
          todas las empresas donde tienes acceso.
        </p>
      </header>

      {enviadoA ? (
        <section className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6" aria-live="polite">
          <div className="flex items-start gap-3">
            <MailCheck aria-hidden size={24} strokeWidth={1.5} className="mt-1 shrink-0 text-success-icon" />
            <div className="grid gap-1">
              <h2 className="text-md font-semibold">Revisa tu correo</h2>
              <p className="text-text-secondary">
                {emailSesion
                  ? `Te enviamos un enlace a ${enviadoA}.`
                  : `Si ${enviadoA} tiene una cuenta en Norvia, le llegará un enlace.`}{' '}
                Vence en {MINUTOS_VIGENCIA} minutos y sirve una sola vez. Si no lo ves, revisa la carpeta de correo no deseado.
              </p>
            </div>
          </div>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
            <Boton variante="secundario" cargando={isSubmitting} textoCargando="Enviando…" onClick={() => void pedir()}>
              Enviar otro enlace
            </Boton>
            <Link to={regreso.a}>{regreso.texto}</Link>
          </div>
        </section>
      ) : (
        <form noValidate className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6" onSubmit={pedir}>
          {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
          {emailSesion ? (
            <p>
              Enviaremos el enlace a <strong className="font-semibold">{emailSesion}</strong>.
            </p>
          ) : (
            <CampoTexto etiqueta="Correo" required type="email" autoComplete="username" registro={register('email')} error={errors.email} />
          )}
          <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
            <Link to={regreso.a}>Cancelar</Link>
            <Boton type="submit" cargando={isSubmitting} textoCargando="Enviando…">
              Enviarme el enlace
            </Boton>
          </div>
        </form>
      )}
    </div>
  )
}
