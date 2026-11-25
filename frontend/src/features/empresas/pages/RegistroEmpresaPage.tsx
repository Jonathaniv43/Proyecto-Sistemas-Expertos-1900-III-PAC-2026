import { useEffect, useState } from 'react'
import { useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { ElegirMetodo } from '@/shared/microsoft/ElegirMetodo'
import { guardarIdentidad, leerIdentidad, olvidarIdentidad, type IdentidadMicrosoft } from '@/shared/microsoft/identidadGuardada'
import { VerificarCorreoMicrosoft } from '@/shared/microsoft/VerificarCorreoMicrosoft'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { Volver } from '@/shared/components/ui/Volver'
import { estadoVolver } from '@/shared/components/ui/estadoVolver'
import { listarRubrosRegistro, registrarEmpresa } from '../api/registro'
import { RegistroCompletado } from '../components/RegistroCompletado'
import { RegistroEmpresaMicrosoft, RegistroMicrosoftCompletado } from '../components/RegistroEmpresaMicrosoft'
import { registroEmpresaEsquema } from '../schemas/registroEmpresa'
import type { RegistrarEmpresaResponse } from '../types/registro'

/** Con 5 opciones o menos se usan radios en vez de select (design-system.md § Select). */
const MAXIMO_RADIOS = 5

const TIPOS_DOCUMENTO = [
  { valor: 'dni', texto: 'DNI' },
  { valor: 'pasaporte', texto: 'Pasaporte' },
] as const

/**
 * HU-017: registro de la empresa y de quien la representa, que queda como Administrador de empresa.
 * Los datos del perfil (HU-026) se completan después de iniciar sesión.
 */
export function RegistroEmpresaPage() {
  const [registro, setRegistro] = useState<RegistrarEmpresaResponse>()
  // Pantalla previa (design-system.md § Elegir cómo entrar): con Microsoft hay una identidad ya validada; con correo se abre el formulario de siempre.
  const [identidad, setIdentidad] = useState<IdentidadMicrosoft | null>(leerIdentidad)
  const [conCorreo, setConCorreo] = useState(false)
  const [conMicrosoft, setConMicrosoft] = useState(false)
  const rubros = usePeticion(listarRubrosRegistro)
  const cargandoRubrosVisible = useCargaVisible(rubros.cargando)

  const {
    register,
    control,
    trigger,
    getFieldState,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(registroEmpresaEsquema, {
    defaultValues: {
      razonSocial: '',
      rtn: '',
      rubroCodigo: '',
      primerNombre: '',
      segundoNombre: '',
      primerApellido: '',
      segundoApellido: '',
      tipoDocumento: 'dni',
      numeroDocumento: '',
      email: '',
    },
    erroresPorCodigo: {
      'empresas.rtn_duplicado': { campo: 'rtn' },
      'empresas.email_duplicado': { campo: 'email' },
      'empresas.documento_duplicado': { campo: 'numeroDocumento' },
      'empresas.rubro_invalido': { campo: 'rubroCodigo', mensaje: 'Ese rubro ya no está disponible. Elige otro.' },
      'empresas.tipo_documento_invalido': { campo: 'tipoDocumento' },
    },
  })

  const { ejecutar: cargarRubros } = rubros
  useEffect(() => {
    void cargarRubros()
  }, [cargarRubros])

  // Al cambiar el tipo de documento, el número ya escrito se vuelve a validar con la regla nueva.
  const tipoDocumento = useWatch({ control, name: 'tipoDocumento' })
  useEffect(() => {
    if (getFieldState('numeroDocumento').isDirty) void trigger('numeroDocumento')
  }, [tipoDocumento, getFieldState, trigger])

  if (registro) {
    return (
      <div className="mx-auto grid max-w-layout-narrow gap-6">
        <Volver a="/" texto="Volver a la página principal" titulo="Registro completado" />
        <h1 className="text-xl font-semibold">Registro completado</h1>
        {conMicrosoft ? <RegistroMicrosoftCompletado registro={registro} /> : <RegistroCompletado registro={registro} />}
      </div>
    )
  }

  if (identidad) {
    const olvidar = () => {
      olvidarIdentidad()
      setIdentidad(null)
    }
    // RN-86: una cuenta nueva con Microsoft exige confirmar el correo con el código de 6 dígitos.
    const falta = identidad.estado === 'sin_cuenta' && !identidad.correoVerificado
    return (
      <div className="mx-auto grid max-w-layout-narrow gap-6">
        <Volver a="/" texto="Volver a la página principal" titulo="Registra tu empresa" />
        <header className="grid gap-2">
          <h1 className="text-xl font-semibold">Registra tu empresa</h1>
          <p className="text-text-secondary">
            Tu cuenta de Microsoft ya quedó validada. Completa los datos de la empresa y quedarás como su administrador.
          </p>
        </header>
        {falta ? (
          <VerificarCorreoMicrosoft
            tokenIdentidad={identidad.tokenIdentidad}
            email={identidad.email}
            onCancelar={olvidar}
            onVerificado={(tokenIdentidad) => {
              const confirmada = { ...identidad, tokenIdentidad, correoVerificado: true }
              guardarIdentidad(confirmada)
              setIdentidad(confirmada)
            }}
          />
        ) : (
        <RegistroEmpresaMicrosoft
          identidad={identidad}
          onCancelar={olvidar}
          onRegistrada={(r) => {
            olvidarIdentidad()
            setConMicrosoft(true)
            setRegistro(r)
          }}
        />
        )}
      </div>
    )
  }

  if (!conCorreo) {
    return (
      <div className="mx-auto grid max-w-layout-narrow gap-6">
        <Volver a="/" texto="Volver a la página principal" titulo="Registra tu empresa" />
        <header className="grid gap-2">
          <h1 className="text-xl font-semibold">Registra tu empresa</h1>
          <p className="text-text-secondary">Elige cómo quieres crear tu cuenta. Quien registra la empresa queda como su administrador.</p>
        </header>
        <ElegirMetodo proposito="registro" onCorreo={() => setConCorreo(true)} />
        <p className="text-text-secondary">
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" state={estadoVolver('/registro', 'Volver al registro')}>
            Inicia sesión
          </Link>
        </p>
      </div>
    )
  }

  const opcionesRubro = (rubros.datos ?? []).map((r) => ({ valor: r.codigo, texto: r.nombre }))

  return (
    <div className="mx-auto grid max-w-layout-narrow gap-6">
      <Volver a="/" texto="Volver a la página principal" titulo="Registra tu empresa" />
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Registra tu empresa</h1>
        <p className="text-text-secondary">
          Crea la cuenta de tu empresa en Norvia. Quien la registra queda como su administrador. Los demás datos de la
          empresa los completas después, desde su perfil.
        </p>
      </header>

      <form
        noValidate
        className="grid gap-8 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async (datos) => setRegistro(await registrarEmpresa(datos)))}
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
          ) : rubros.cargando || !rubros.datos ? (
            <div aria-busy="true" aria-label="Cargando rubros" className="grid gap-2">
              {cargandoRubrosVisible && (
                <>
                  <div className="h-lh w-1/4 animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
                  <div className="h-control w-full animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
                </>
              )}
            </div>
          ) : opcionesRubro.length <= MAXIMO_RADIOS ? (
            <GrupoRadio
              etiqueta="Rubro"
              required
              registro={register('rubroCodigo')}
              opciones={opcionesRubro}
              error={errors.rubroCodigo}
            />
          ) : (
            <CampoSelect
              etiqueta="Rubro"
              required
              registro={register('rubroCodigo')}
              opciones={opcionesRubro}
              error={errors.rubroCodigo}
            />
          )}
        </fieldset>

        <fieldset className="grid gap-4">
          <legend className="mb-1 text-md font-semibold">Tus datos</legend>
          <p className="mb-3 text-text-secondary">Serás el administrador de la empresa en Norvia.</p>
          <div className="grid gap-4 sm:grid-cols-2">
            <CampoTexto
              etiqueta="Primer nombre"
              required
              autoComplete="given-name"
              registro={register('primerNombre')}
              error={errors.primerNombre}
            />
            <CampoTexto
              etiqueta="Segundo nombre"
              autoComplete="additional-name"
              registro={register('segundoNombre')}
              error={errors.segundoNombre}
            />
            <CampoTexto
              etiqueta="Primer apellido"
              required
              autoComplete="family-name"
              registro={register('primerApellido')}
              error={errors.primerApellido}
            />
            <CampoTexto
              etiqueta="Segundo apellido"
              autoComplete="off"
              registro={register('segundoApellido')}
              error={errors.segundoApellido}
            />
          </div>
          <GrupoRadio
            etiqueta="Tipo de documento"
            required
            registro={register('tipoDocumento')}
            opciones={TIPOS_DOCUMENTO}
            error={errors.tipoDocumento}
          />
          <CampoTexto
            etiqueta="Número de documento"
            required
            inputMode={tipoDocumento === 'dni' ? 'numeric' : 'text'}
            autoComplete="off"
            registro={register('numeroDocumento')}
            largoExacto={tipoDocumento === 'dni' ? 13 : undefined}
            error={errors.numeroDocumento}
            ayuda={tipoDocumento === 'dni' ? '13 dígitos.' : 'Letras, números y guiones.'}
          />
          <CampoTexto
            etiqueta="Correo"
            required
            type="email"
            autoComplete="email"
            registro={register('email')}
            error={errors.email}
            ayuda="Aquí te enviaremos tu contraseña temporal. Es el correo con el que vas a iniciar sesión."
          />
        </fieldset>

        <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-text-secondary">
            ¿Ya tienes cuenta?{' '}
            <Link to="/login" state={estadoVolver('/registro', 'Volver al registro')}>
              Inicia sesión
            </Link>
          </p>
          <Boton type="submit" cargando={isSubmitting} textoCargando="Registrando…">
            Registrar empresa
          </Boton>
        </div>
      </form>
    </div>
  )
}
