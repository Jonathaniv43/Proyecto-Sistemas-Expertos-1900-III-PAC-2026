import { Link } from 'react-router-dom'
import { sesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { useFormulario } from '@/shared/forms'
import { iniciarSesionPlataforma } from '../api/sesion'
import { loginPlataformaEsquema } from '../schemas/login'

/** Ingreso del equipo de Norvia (StaffPlataforma), separado del de empresa (D23, RN-64). */
export function LoginPlataformaPage() {
  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(loginPlataformaEsquema, {
    defaultValues: { email: '', password: '' },
    erroresPorCodigo: {
      'auth.credenciales_invalidas': { mensaje: 'No pudimos iniciar sesión. Revisa el correo y la contraseña.' },
    },
  })

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6 sm:py-8">
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Acceso del equipo de Norvia</h1>
        <p className="text-text-secondary">Solo para la administración de la plataforma. Las empresas entran desde el inicio de sesión habitual.</p>
      </header>
      <form
        noValidate
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async (datos) => sesion.iniciar(await iniciarSesionPlataforma(datos), null))}
      >
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <CampoTexto etiqueta="Correo" required type="email" autoComplete="username" registro={register('email')} error={errors.email} />
        <CampoTexto
          etiqueta="Contraseña"
          required
          type="password"
          autoComplete="current-password"
          registro={register('password')}
          error={errors.password}
        />
        <Boton type="submit" className="w-full sm:w-auto sm:justify-self-start" cargando={isSubmitting} textoCargando="Ingresando…">
          Iniciar sesión
        </Boton>
      </form>
      <p>
        <Link to="/recuperar-contrasena">¿Olvidaste tu contraseña?</Link>
      </p>
    </div>
  )
}
