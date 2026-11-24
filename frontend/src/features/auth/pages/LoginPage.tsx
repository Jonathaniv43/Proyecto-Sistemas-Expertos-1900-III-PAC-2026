import { Link, useLocation, useSearchParams } from 'react-router-dom'
import { sesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { useFormulario } from '@/shared/forms'
import type { EstadoLogin } from '@/shared/sesion'
import { iniciarSesionEmpresa, resolverEmpresaPorRtn } from '../api/sesion'
import { AvisoMotivo } from '../components/AvisoMotivo'
import { loginEmpresaEsquema } from '../schemas/login'
import { leerUltimoRtn, recordarRtn } from '../ultimoRtn'

/** Un solo mensaje para cualquier dato incorrecto: no revela si falló el RTN, el correo o la contraseña. */
const NO_SE_PUDO = 'No pudimos iniciar sesión. Revisa el RTN, el correo y la contraseña.'

/**
 * HU-018: inicio de sesión en una empresa. Cada ingreso es a una empresa (por su RTN); para otra
 * empresa se inicia otra sesión (HU-019). Al entrar, SoloSinSesion lleva a la ruta pedida o al inicio.
 */
export function LoginPage() {
  const { state } = useLocation()
  const [parametros] = useSearchParams()
  const motivo = (state as EstadoLogin | null)?.motivo

  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(loginEmpresaEsquema, {
    defaultValues: { rtn: parametros.get('rtn') ?? leerUltimoRtn(), email: '', password: '' },
    erroresPorCodigo: {
      'empresas.rtn_no_encontrado': { mensaje: NO_SE_PUDO },
      'auth.credenciales_invalidas': { mensaje: NO_SE_PUDO },
    },
  })

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6 sm:py-8">
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Iniciar sesión</h1>
        <p className="text-text-secondary">Entra a tu empresa con su RTN, tu correo y tu contraseña.</p>
      </header>

      <AvisoMotivo motivo={motivo} />

      <form
        noValidate
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async ({ rtn, email, password }) => {
          const empresa = await resolverEmpresaPorRtn(rtn)
          const respuesta = await iniciarSesionEmpresa({ email, password, idEmpresa: empresa.idEmpresa })
          recordarRtn(empresa.rtn)
          sesion.iniciar(respuesta, { razonSocial: empresa.razonSocial, rtn: empresa.rtn })
        })}
      >
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <CampoTexto
          etiqueta="RTN de la empresa"
          required
          inputMode="numeric"
          autoComplete="organization"
          registro={register('rtn')}
          error={errors.rtn}
          ayuda="14 dígitos. Si trabajas en varias empresas, usa el RTN de la empresa a la que quieres entrar."
        />
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
          <Link to="/recuperar-contrasena">¿Olvidaste tu contraseña?</Link>
        </p>
        <p>
          ¿Recibiste una contraseña temporal? <Link to="/verificar-cuenta">Activa tu cuenta</Link>
        </p>
        <p>
          ¿Tu empresa aún no está en Norvia? <Link to="/registro">Regístrala</Link>
        </p>
      </div>
    </div>
  )
}
