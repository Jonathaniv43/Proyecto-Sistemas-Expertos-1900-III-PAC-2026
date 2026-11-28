import { Lock } from 'lucide-react'
import { useEffect, useId, useState } from 'react'
import { useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import type { IdentidadMicrosoft } from '@/shared/microsoft/identidadGuardada'
import { esIdentidadVencida } from '@/shared/microsoft/flujo'
import { AvisoMicrosoftVencido } from '@/shared/microsoft/MicrosoftVencido'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton, clasesControl } from '@/shared/components/ui/clases'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { Etiqueta, MensajeCampo } from '@/shared/components/ui/MensajeCampo'
import { useFormulario } from '@/shared/forms'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { recordarEmpresa, rutaLogin } from '@/shared/sesion'
import { listarRubrosRegistro, registrarEmpresaMicrosoft } from '../api/registro'
import { registroEmpresaMicrosoftEsquema } from '../schemas/registroEmpresaMicrosoft'
import type { RegistrarEmpresaResponse } from '../types/registro'

const MAXIMO_RADIOS = 5

const TIPOS_DOCUMENTO = [
  { valor: '', texto: 'Ninguno por ahora' },
  { valor: 'dni', texto: 'DNI' },
  { valor: 'pasaporte', texto: 'Pasaporte' },
] as const

interface RegistroEmpresaMicrosoftProps {
  identidad: IdentidadMicrosoft
  /** La persona prefiere empezar de nuevo (otra cuenta o correo y contraseña). */
  onCancelar: () => void
  onRegistrada: (registro: RegistrarEmpresaResponse) => void
}

/**
 * HU-119: el mismo formulario de registro, pero con el correo bloqueado (viene de Microsoft, RN-84), sin
 * contraseña y con los nombres prellenados y editables. Si la persona ya existe en Norvia, sus nombres se
 * muestran sin poder editarse y solo se completan los datos vacíos (RN-85).
 */
export function RegistroEmpresaMicrosoft({ identidad, onCancelar, onRegistrada }: RegistroEmpresaMicrosoftProps) {
  const idCorreo = useId()
  const rubros = usePeticion(listarRubrosRegistro)
  const { ejecutar: cargarRubros } = rubros
  useEffect(() => {
    void cargarRubros()
  }, [cargarRubros])

  const personaExistente = identidad.personaExistente
  const {
    register,
    control,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(registroEmpresaMicrosoftEsquema, {
    defaultValues: {
      razonSocial: '',
      rtn: '',
      rubroCodigo: '',
      primerNombre: identidad.primerNombre,
      segundoNombre: identidad.segundoNombre ?? '',
      primerApellido: identidad.primerApellido,
      segundoApellido: identidad.segundoApellido ?? '',
      tipoDocumento: '',
      numeroDocumento: '',
    },
    erroresPorCodigo: {
      'empresas.rtn_duplicado': { campo: 'rtn' },
      'empresas.documento_duplicado': { campo: 'numeroDocumento' },
      'empresas.rubro_invalido': { campo: 'rubroCodigo', mensaje: 'Ese rubro ya no está disponible. Elige otro.' },
    },
  })

  const tipoDocumento = useWatch({ control, name: 'tipoDocumento' })
  const opcionesRubro = (rubros.datos ?? []).map((r) => ({ valor: r.codigo, texto: r.nombre }))
  const [vencida, setVencida] = useState(false)

  if (vencida) return <AvisoMicrosoftVencido proposito="registro" />

  return (
    <form
      noValidate
      className="grid gap-8 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async (datos) => {
        try {
          onRegistrada(
            await registrarEmpresaMicrosoft({
              ...datos,
              tokenIdentidad: identidad.tokenIdentidad,
              tipoDocumento: datos.tipoDocumento || undefined,
            }),
          )
        } catch (causa) {
          if (!esIdentidadVencida(causa)) throw causa
          setVencida(true)
        }
      })}
    >
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}

      <fieldset className="grid gap-4">
        <legend className="mb-4 text-md font-semibold">Datos de la empresa</legend>
        <CampoTexto
          etiqueta="Razón social"
          required
          autoComplete="organization"
          registro={register('razonSocial')}
          error={errors.razonSocial}
          ayuda="El nombre legal de la empresa, como aparece en su RTN."
        />
        <CampoTexto
          etiqueta="RTN"
          required
          inputMode="numeric"
          autoComplete="off"
          registro={register('rtn')}
          largoExacto={14}
          error={errors.rtn}
          ayuda="14 dígitos. Con este número vas a iniciar sesión."
        />
        {rubros.error ? (
          <div className="grid gap-2">
            <Aviso tipo="error" titulo="No se pudo cargar la lista de rubros">
              {rubros.error.mensaje}
            </Aviso>
            <Boton variante="secundario" className="justify-self-start" onClick={() => void cargarRubros()}>
              Reintentar
            </Boton>
          </div>
        ) : !rubros.datos ? (
          <div aria-busy="true" aria-label="Cargando rubros" className="h-control w-full animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
        ) : opcionesRubro.length <= MAXIMO_RADIOS ? (
          <GrupoRadio etiqueta="Rubro" required registro={register('rubroCodigo')} opciones={opcionesRubro} error={errors.rubroCodigo} />
        ) : (
          <CampoSelect etiqueta="Rubro" required registro={register('rubroCodigo')} opciones={opcionesRubro} error={errors.rubroCodigo} />
        )}
      </fieldset>

      <fieldset className="grid gap-4">
        <legend className="mb-1 text-md font-semibold">Tus datos</legend>
        <p className="mb-3 text-text-secondary">Serás el administrador de la empresa en Norvia.</p>

        <div className="grid content-start gap-2">
          <Etiqueta htmlFor={idCorreo}>Correo</Etiqueta>
          <div className="relative">
            <input
              id={idCorreo}
              type="email"
              value={identidad.email}
              disabled
              aria-describedby={`${idCorreo}-mensaje`}
              className={clasesControl(false)}
              style={{ paddingRight: 'var(--control-height-touch)' }}
            />
            <span aria-hidden className="pointer-events-none absolute inset-y-0 right-3 flex items-center text-text-disabled">
              <Lock size={16} strokeWidth={1.5} />
            </span>
          </div>
          <MensajeCampo id={`${idCorreo}-mensaje`} ayuda="Viene de tu cuenta de Microsoft." />
        </div>

        {personaExistente ? (
          <Aviso tipo="info">
            Ya tienes datos guardados en Norvia como <strong className="font-medium">{[identidad.primerNombre, identidad.segundoNombre, identidad.primerApellido, identidad.segundoApellido].filter(Boolean).join(' ')}</strong>.
            Usaremos esos datos; si quieres corregirlos, hazlo desde tu perfil.
          </Aviso>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2">
            <CampoTexto etiqueta="Primer nombre" required autoComplete="given-name" registro={register('primerNombre')} error={errors.primerNombre} />
            <CampoTexto etiqueta="Segundo nombre" autoComplete="additional-name" registro={register('segundoNombre')} error={errors.segundoNombre} />
            <CampoTexto etiqueta="Primer apellido" required autoComplete="family-name" registro={register('primerApellido')} error={errors.primerApellido} />
            <CampoTexto etiqueta="Segundo apellido" autoComplete="off" registro={register('segundoApellido')} error={errors.segundoApellido} />
          </div>
        )}

        <GrupoRadio etiqueta="Tipo de documento" registro={register('tipoDocumento')} opciones={TIPOS_DOCUMENTO} error={errors.tipoDocumento} />
        {tipoDocumento && (
          <CampoTexto
            etiqueta="Número de documento"
            inputMode={tipoDocumento === 'dni' ? 'numeric' : 'text'}
            autoComplete="off"
            registro={register('numeroDocumento')}
            largoExacto={tipoDocumento === 'dni' ? 13 : undefined}
            error={errors.numeroDocumento}
            ayuda={tipoDocumento === 'dni' ? '13 dígitos.' : 'Letras, números y guiones.'}
          />
        )}
        <p className="text-xs text-text-secondary">Puedes completar tu documento y tu teléfono después, desde tu perfil.</p>
      </fieldset>

      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
        <button type="button" className="text-left text-sm font-medium text-text-link hover:text-text-link-hover hover:underline" onClick={onCancelar}>
          Usar otra cuenta o mi correo
        </button>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Registrando…">
          Registrar empresa
        </Boton>
      </div>
    </form>
  )
}

/** Confirmación del registro con Microsoft: no hay contraseña temporal ni correo de activación. */
export function RegistroMicrosoftCompletado({ registro }: { registro: RegistrarEmpresaResponse }) {
  // La empresa recién registrada queda como la del login, para mostrar su nombre en /login?empresa=….
  useEffect(() => recordarEmpresa(registro), [registro])

  return (
    <section className="grid gap-6" aria-labelledby="titulo-registro-microsoft">
      <Aviso tipo="exito" titulo="Tu empresa quedó registrada">
        {registro.razonSocial} ya está en Norvia y tú eres su administrador. Tu cuenta ya está activa: no necesitas contraseña.
      </Aviso>
      <div className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-6">
        <h2 id="titulo-registro-microsoft" className="text-md font-semibold">
          Entra a tu empresa
        </h2>
        <p className="text-text-secondary">
          Inicia sesión con Microsoft usando <strong className="font-medium text-text-primary">{registro.email}</strong>.
        </p>
        <dl className="grid gap-1">
          <dt className="text-xs text-text-secondary">Para iniciar sesión usarás el RTN de tu empresa</dt>
          <dd className="font-medium tabular-nums">{registro.rtn}</dd>
        </dl>
        <Link to={rutaLogin(registro.idEmpresa)} className={`${clasesBoton('primario')} justify-self-start`}>
          Iniciar sesión
        </Link>
      </div>
    </section>
  )
}
