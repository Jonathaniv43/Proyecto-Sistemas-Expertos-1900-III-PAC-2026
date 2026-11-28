import { useState } from 'react'
import { Link, Navigate, useLocation } from 'react-router-dom'
import { sesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { estadoVolver } from '@/shared/components/ui/estadoVolver'
import { useFormulario } from '@/shared/forms'
import { leerUltimaEmpresa, rutaActual, rutaLogin, type EmpresaLogin } from '@/shared/sesion'
import { iniciarSesionEmpresa } from '../api/sesion'
import { loginEmpresaEsquema } from '../schemas/login'
import { TarjetaEmpresa } from './BuscarEmpresa'
import { ElegirMetodo } from '@/shared/microsoft/ElegirMetodo'

/** Un solo mensaje para cualquier dato incorrecto: no revela si falló la empresa, el correo o la contraseña. */
const NO_SE_PUDO = 'No pudimos iniciar sesión. Revisa el correo y la contraseña.'

/**
 * Paso 2 del login (HU-018): `/login?empresa=…`. El `idEmpresa` llega cifrado (de por-rtn) y se
 * manda tal cual a POST /api/auth/login/empresa, que lo descifra. El nombre de la empresa es el que
 * guardó el paso 1; si este navegador no lo tiene (link de otro lado), se vuelve a buscar por RTN.
 */
export function CredencialesEmpresa({ idEmpresa }: { idEmpresa: string }) {
  const { state } = useLocation()
  const guardada = leerUltimaEmpresa()
  // El paso 1 avisa por qué se pide el RTN: el enlace no trae el nombre de la empresa (observación 15 del inventario).
  if (guardada?.idEmpresa !== idEmpresa)
    return <Navigate to={rutaLogin()} replace state={{ ...((state as object | null) ?? {}), enlaceSinEmpresa: true }} />
  return <FormularioCredenciales empresa={guardada} />
}

function FormularioCredenciales({ empresa }: { empresa: EmpresaLogin }) {
  const location = useLocation()
  const volverAqui = estadoVolver(rutaActual(location), 'Volver a iniciar sesión')
  // Pantalla previa (design-system.md § Elegir cómo entrar): Microsoft o correo y contraseña.
  const [conCorreo, setConCorreo] = useState(false)

  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(loginEmpresaEsquema, {
    defaultValues: { email: '', password: '' },
    erroresPorCodigo: {
      'auth.credenciales_invalidas': { mensaje: NO_SE_PUDO },
    },
  })

  return (
    <>
      <section
        aria-label="Empresa"
        className="flex items-start justify-between gap-3 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      >
        <TarjetaEmpresa empresa={empresa} />
        <Link to={rutaLogin()} state={location.state} className="shrink-0 text-sm font-medium">
          Buscar otra empresa
        </Link>
      </section>

      {!conCorreo ? (
        <ElegirMetodo proposito="login" idEmpresa={empresa.idEmpresa} onCorreo={() => setConCorreo(true)} />
      ) : (
        <>
      <form
        noValidate
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async ({ email, password }) => {
          const respuesta = await iniciarSesionEmpresa({ email, password, idEmpresa: empresa.idEmpresa })
          sesion.iniciar(respuesta, { razonSocial: empresa.razonSocial, rtn: empresa.rtn })
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

      <div className="grid gap-2 text-text-secondary">
        <p>
          <Link to="/recuperar-contrasena" state={volverAqui}>
            ¿Olvidaste tu contraseña?
          </Link>
        </p>
        <p>
          ¿Recibiste una contraseña temporal?{' '}
          <Link to={`/verificar-cuenta?${new URLSearchParams({ empresa: empresa.idEmpresa })}`} state={volverAqui}>
            Activa tu cuenta
          </Link>
        </p>
      </div>
        </>
      )}
    </>
  )
}
