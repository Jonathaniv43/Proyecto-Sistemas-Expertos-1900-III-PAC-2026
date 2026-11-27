import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { useFormulario } from '@/shared/forms'
import { DESCRIPCIONES_ROL_EMPRESA, ETIQUETAS_ROL, ROLES_EMPRESA, useSesion } from '@/shared/sesion'
import { Volver } from '@/shared/components/ui/Volver'
import { crearVinculo } from '../api/vinculos'
import { PersonaAgregada } from '../components/PersonaAgregada'
import { agregarPersonaEsquema } from '../schemas/agregarPersona'
import type { CrearVinculoResponse } from '../types/vinculos'

const OPCIONES_ROL = ROLES_EMPRESA.map((rol) => ({
  valor: rol,
  texto: ETIQUETAS_ROL[rol],
  descripcion: DESCRIPCIONES_ROL_EMPRESA[rol],
}))

const VALORES_INICIALES = {
  email: '',
  primerNombre: '',
  segundoNombre: '',
  primerApellido: '',
  segundoApellido: '',
  rolCodigo: undefined,
}

/**
 * HU-020: el administrador da acceso a la empresa a una persona, con un rol. Si el correo ya tiene
 * cuenta en Norvia se reutiliza (también si trabaja en otras empresas); si no, se crea la cuenta y le
 * llega un código de 6 dígitos por correo para activarla y elegir su contraseña (HU-114, RN-86).
 */
export function AgregarPersonaPage() {
  const sesion = useSesion()
  const empresa = sesion.estado === 'activa' ? sesion.usuario.empresa : null
  const [resultado, setResultado] = useState<CrearVinculoResponse>()

  const {
    register,
    reset,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(agregarPersonaEsquema, {
    defaultValues: VALORES_INICIALES,
    erroresPorCodigo: {
      'vinculos.duplicado': { campo: 'email', mensaje: 'Esa persona ya tiene acceso a esta empresa.' },
      'vinculos.rol_invalido': { campo: 'rolCodigo', mensaje: 'Ese rol no está disponible. Elige otro.' },
    },
  })

  if (resultado) {
    return (
      <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
        <Volver a="/personas" texto="Volver a personas" titulo="Persona agregada" />
        <h1 className="text-xl font-semibold">Persona agregada</h1>
        <PersonaAgregada
          persona={resultado}
          empresa={empresa}
          onAgregarOtra={() => {
            reset(VALORES_INICIALES)
            setResultado(undefined)
          }}
        />
      </div>
    )
  }

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <Volver a="/personas" texto="Volver a personas" titulo="Agregar una persona" />
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Agregar una persona</h1>
        <p className="text-text-secondary">
          Dale acceso a {empresa?.razonSocial ?? 'la empresa'} y elige qué podrá hacer. Si ya usa Norvia en otra empresa,
          escribe el mismo correo: entrará con su cuenta de siempre.
        </p>
      </header>

      <form
        noValidate
        className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async (datos) => setResultado(await crearVinculo(datos)))}
      >
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <CampoTexto
          etiqueta="Correo"
          required
          type="email"
          autoComplete="off"
          registro={register('email')}
          error={errors.email}
          ayuda="Con este correo entrará a Norvia."
        />
        <div className="grid gap-4 sm:grid-cols-2">
          <CampoTexto etiqueta="Primer nombre" required autoComplete="off" registro={register('primerNombre')} error={errors.primerNombre} />
          <CampoTexto etiqueta="Segundo nombre" autoComplete="off" registro={register('segundoNombre')} error={errors.segundoNombre} />
          <CampoTexto etiqueta="Primer apellido" required autoComplete="off" registro={register('primerApellido')} error={errors.primerApellido} />
          <CampoTexto etiqueta="Segundo apellido" autoComplete="off" registro={register('segundoApellido')} error={errors.segundoApellido} />
        </div>
        <GrupoRadio
          etiqueta="Rol en la empresa"
          required
          registro={register('rolCodigo')}
          opciones={OPCIONES_ROL}
          error={errors.rolCodigo}
        />
        <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
          <Link to="/personas">Cancelar</Link>
          <Boton type="submit" cargando={isSubmitting} textoCargando="Agregando…">
            Agregar persona
          </Boton>
        </div>
      </form>
    </div>
  )
}
