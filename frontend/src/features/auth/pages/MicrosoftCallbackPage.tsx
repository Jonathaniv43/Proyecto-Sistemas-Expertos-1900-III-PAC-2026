import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { extraerError, sesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { clasesBoton } from '@/shared/components/ui/clases'
import { leerUltimaEmpresa, rutaLogin } from '@/shared/sesion'
import { iniciarSesionEmpresaMicrosoft, obtenerIdentidadMicrosoft } from '../api/microsoft'
import { VincularMicrosoftForm } from '../components/VincularMicrosoftForm'
import { REDIRECT_URI, tomarPendiente, type PropositoMicrosoft } from '@/shared/microsoft/flujo'
import { guardarIdentidad, type IdentidadMicrosoft } from '@/shared/microsoft/identidadGuardada'

type Fase =
  | { fase: 'cargando' }
  | { fase: 'error'; mensaje: string; proposito?: PropositoMicrosoft }
  | { fase: 'vincular'; identidad: IdentidadMicrosoft; proposito: PropositoMicrosoft; idEmpresa?: string }
  | { fase: 'sin_cuenta'; identidad: IdentidadMicrosoft }

const ERROR_GENERICO = 'No pudimos entrar con Microsoft. Intenta de nuevo o usa tu correo y contraseña.'

/**
 * Retorno de Microsoft (`/auth/microsoft/callback`, HU-119 y HU-120). Valida el `state`, canjea el código en el
 * backend y sigue según el estado de la cuenta:
 * - cuenta vinculada: login (o registro de otra empresa con los datos ya guardados);
 * - correo con contraseña: pide esa contraseña para vincular Microsoft (RN-83) y sigue;
 * - sin cuenta: ofrece registrar la empresa con el correo ya completado.
 */
export function MicrosoftCallbackPage() {
  const [parametros] = useSearchParams()
  const navegar = useNavigate()
  const [estado, setEstado] = useState<Fase>({ fase: 'cargando' })
  const iniciado = useRef(false)

  useEffect(() => {
    // El código sirve una sola vez: sin esta guarda, StrictMode lo canjearía dos veces en desarrollo.
    if (iniciado.current) return
    iniciado.current = true

    void (async () => {
      const rechazo = parametros.get('error')
      const codigo = parametros.get('code')
      const pendiente = tomarPendiente(parametros.get('state'))
      if (rechazo || !codigo || !pendiente) {
        setEstado({
          fase: 'error',
          mensaje: rechazo ? 'Cancelaste el acceso con Microsoft.' : 'El acceso con Microsoft venció. Empieza de nuevo.',
          proposito: pendiente?.proposito,
        })
        return
      }

      try {
        const identidad = await obtenerIdentidadMicrosoft({
          codigo,
          codeVerifier: pendiente.verifier,
          redirectUri: REDIRECT_URI,
        })
        await continuar(identidad, pendiente.proposito, pendiente.idEmpresa)
      } catch (causa) {
        setEstado({ fase: 'error', mensaje: extraerError(causa).mensaje || ERROR_GENERICO, proposito: pendiente.proposito })
      }
    })()
    // Se ejecuta una sola vez al volver de Microsoft.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  /** Cada estado de la cuenta lleva a un paso distinto del flujo que la persona había elegido. */
  async function continuar(identidad: IdentidadMicrosoft, proposito: PropositoMicrosoft, idEmpresa?: string) {
    if (identidad.estado === 'cuenta_con_contrasena') {
      setEstado({ fase: 'vincular', identidad, proposito, idEmpresa })
      return
    }

    if (proposito === 'registro') {
      guardarIdentidad(identidad)
      navegar('/registro', { replace: true })
      return
    }

    if (identidad.estado === 'sin_cuenta') {
      setEstado({ fase: 'sin_cuenta', identidad })
      return
    }
    await entrar(identidad.tokenIdentidad, idEmpresa)
  }

  async function entrar(tokenIdentidad: string, idEmpresa?: string) {
    const empresa = leerUltimaEmpresa()
    if (!idEmpresa || empresa?.idEmpresa !== idEmpresa) {
      navegar(rutaLogin(), { replace: true })
      return
    }
    const respuesta = await iniciarSesionEmpresaMicrosoft({ tokenIdentidad, idEmpresa })
    sesion.iniciar(respuesta, { razonSocial: empresa.razonSocial, rtn: empresa.rtn })
  }

  async function alVincular(identidad: IdentidadMicrosoft, proposito: PropositoMicrosoft, idEmpresa?: string) {
    // Ya es la misma persona de esa cuenta: se usan sus datos guardados, no los de Microsoft (RN-85).
    const vinculada: IdentidadMicrosoft = { ...identidad, estado: 'cuenta_vinculada', personaExistente: true }
    try {
      await continuar(vinculada, proposito, idEmpresa)
    } catch (causa) {
      setEstado({ fase: 'error', mensaje: extraerError(causa).mensaje || ERROR_GENERICO, proposito })
    }
  }

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      {estado.fase === 'cargando' && (
        <div role="status" aria-live="polite" className="grid gap-2">
          <h1 className="text-xl font-semibold">Entrando con Microsoft…</h1>
          <p className="text-text-secondary">Estamos validando tu cuenta.</p>
        </div>
      )}

      {estado.fase === 'error' && (
        <>
          <h1 className="text-xl font-semibold">No se pudo continuar</h1>
          <Aviso tipo="error">{estado.mensaje}</Aviso>
          <Link to={estado.proposito === 'registro' ? '/registro' : rutaLogin()} className={`${clasesBoton('primario')} justify-self-start`}>
            Volver a intentar
          </Link>
        </>
      )}

      {estado.fase === 'vincular' && (
        <>
          <h1 className="text-xl font-semibold">Confirma tu cuenta</h1>
          <VincularMicrosoftForm
            tokenIdentidad={estado.identidad.tokenIdentidad}
            email={estado.identidad.email}
            onVinculada={() => void alVincular(estado.identidad, estado.proposito, estado.idEmpresa)}
          />
        </>
      )}

      {estado.fase === 'sin_cuenta' && (
        <>
          <h1 className="text-xl font-semibold">Todavía no tienes una cuenta en Norvia</h1>
          <p className="text-text-secondary">
            No encontramos una cuenta con <strong className="font-medium text-text-primary">{estado.identidad.email}</strong>.
            Puedes registrar tu empresa con esta cuenta de Microsoft: el correo ya queda completado.
          </p>
          <button
            type="button"
            className={`${clasesBoton('primario')} justify-self-start`}
            onClick={() => {
              guardarIdentidad(estado.identidad)
              navegar('/registro', { replace: true })
            }}
          >
            Registrar mi empresa
          </button>
        </>
      )}
    </div>
  )
}
