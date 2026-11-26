import { useState } from 'react'
import { api, extraerError } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { useFormulario } from '@/shared/forms'
import { z } from '@/shared/validacion'

/** POST /api/auth/microsoft/codigo (anónimo): envía el código de 6 dígitos al correo de la cuenta (RN-86). */
const enviarCodigo = (tokenIdentidad: string) =>
  api.post<{ mensaje: string; minutosVigencia: number }>('/auth/microsoft/codigo', { tokenIdentidad }).then((r) => r.data)

/** POST /api/auth/microsoft/codigo/verificar: devuelve el token de identidad con el correo ya confirmado. */
const verificarCodigo = (datos: { tokenIdentidad: string; codigo: string }) =>
  api.post<{ tokenIdentidad: string }>('/auth/microsoft/codigo/verificar', datos).then((r) => r.data)

/** Espejo de VerificarCodigoMicrosoftValidator: 6 dígitos (se aceptan espacios al pegar). */
const codigoEsquema = z.object({
  codigo: z
    .string()
    .trim()
    .min(1, 'Escribe el código de 6 dígitos.')
    .transform((valor) => valor.replace(/\s/g, ''))
    .pipe(z.string().regex(/^\d{6}$/, 'El código tiene 6 dígitos.')),
})

interface VerificarCorreoMicrosoftProps {
  tokenIdentidad: string
  email: string
  /** El correo quedó confirmado: llega el token nuevo, que es el que se usa al registrar la empresa. */
  onVerificado: (tokenIdentidad: string) => void
  /** La persona prefiere empezar de nuevo (otra cuenta o correo y contraseña). */
  onCancelar: () => void
}

/**
 * RN-86: antes de crear una cuenta con Microsoft se confirma que el correo es de quien se registra, con un código
 * de 6 dígitos que llega a ese correo. Microsoft no siempre garantiza que el correo que informa sea del titular.
 */
export function VerificarCorreoMicrosoft({ tokenIdentidad, email, onVerificado, onCancelar }: VerificarCorreoMicrosoftProps) {
  const [enviado, setEnviado] = useState(false)
  const [enviando, setEnviando] = useState(false)
  const [errorEnvio, setErrorEnvio] = useState<string>()

  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(codigoEsquema, {
    defaultValues: { codigo: '' },
    erroresPorCodigo: {
      'auth.codigo_incorrecto': { campo: 'codigo' },
      'auth.demasiados_intentos': { mensaje: 'Hiciste demasiados intentos. Espera unos minutos y pide un código nuevo.' },
      'auth.identidad_invalida': { mensaje: 'La validación con Microsoft venció. Vuelve a empezar.' },
    },
  })

  async function pedirCodigo() {
    setErrorEnvio(undefined)
    setEnviando(true)
    try {
      await enviarCodigo(tokenIdentidad)
      setEnviado(true)
    } catch (causa) {
      setErrorEnvio(extraerError(causa).mensaje)
    } finally {
      setEnviando(false)
    }
  }

  return (
    <section aria-labelledby="titulo-verificar-correo" className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <h2 id="titulo-verificar-correo" className="text-md font-semibold">
        Confirma tu correo
      </h2>
      <p className="text-text-secondary">
        Para crear tu cuenta con Microsoft necesitamos comprobar que <strong className="font-medium text-text-primary">{email}</strong> es tu
        correo. Te enviaremos un código de 6 dígitos.
      </p>
      {errorEnvio && <Aviso tipo="error">{errorEnvio}</Aviso>}

      {!enviado ? (
        <Boton className="w-full sm:w-auto sm:justify-self-start" cargando={enviando} textoCargando="Enviando…" onClick={() => void pedirCodigo()}>
          Enviar código
        </Boton>
      ) : (
        <form
          noValidate
          className="grid gap-4"
          onSubmit={enviar(async ({ codigo }) => onVerificado((await verificarCodigo({ tokenIdentidad, codigo })).tokenIdentidad))}
        >
          <Aviso tipo="exito">Enviamos el código a {email}. Vence en unos 10 minutos.</Aviso>
          {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
          <CampoTexto
            etiqueta="Código de 6 dígitos"
            required
            inputMode="numeric"
            autoComplete="one-time-code"
            registro={register('codigo')}
            error={errors.codigo}
          />
          <div className="flex flex-col items-stretch gap-4 sm:flex-row sm:items-center">
            <Boton type="submit" cargando={isSubmitting} textoCargando="Verificando…">
              Verificar código
            </Boton>
            <Boton variante="secundario" cargando={enviando} textoCargando="Enviando…" onClick={() => void pedirCodigo()}>
              Reenviar código
            </Boton>
          </div>
        </form>
      )}

      <button type="button" className="justify-self-start text-sm font-medium text-text-link hover:text-text-link-hover hover:underline" onClick={onCancelar}>
        Usar otra cuenta o mi correo
      </button>
    </section>
  )
}
