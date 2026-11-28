import { TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState, type ReactNode } from 'react'
import { sesion as sesionApi } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { useTieneRol, ROLES } from '@/shared/sesion'
import { actualizarPerfil, obtenerPerfil } from '../api/perfil'
import { listarRubrosRegistro } from '../api/registro'
import { perfilEmpresaEsquema } from '../schemas/perfilEmpresa'
import type { ActualizarPerfilRequest, PerfilEmpresa } from '../types/perfil'
import type { RubroRegistro } from '../types/registro'

const MAXIMO_RADIOS = 5

/**
 * HU-026: perfil de la empresa. La administración completa y actualiza razón social, rubro, número
 * de trabajadores permanentes, dirección y actividad económica. El RTN no se edita: con él se inicia
 * sesión. El Responsable SST lo ve sin poder cambiarlo.
 */
export function PerfilEmpresaPage() {
  const puedeEditar = useTieneRol([ROLES.adminEmpresa])
  const perfil = usePeticion(obtenerPerfil)
  const rubros = usePeticion(listarRubrosRegistro)
  const cargandoVisible = useCargaVisible(perfil.cargando || rubros.cargando)
  const [actual, setActual] = useState<PerfilEmpresa>()

  const { ejecutar: cargarPerfil } = perfil
  const { ejecutar: cargarRubros } = rubros
  const cargar = useCallback(() => {
    void cargarPerfil().then((datos) => datos && setActual(datos))
    if (puedeEditar) void cargarRubros()
  }, [cargarPerfil, cargarRubros, puedeEditar])
  useEffect(cargar, [cargar])

  const error = perfil.error ?? (puedeEditar ? rubros.error : undefined)
  const listo = actual && (!puedeEditar || rubros.datos)

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Perfil de la empresa</h1>
        <p className="text-text-secondary">
          Con estos datos Norvia sabe qué le aplica a tu empresa; el rubro define las categorías de riesgo de las encuestas.
        </p>
      </header>

      {error && !listo ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar el perfil de la empresa</h2>
          <p className="text-text-secondary">{error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !listo ? (
        <div aria-busy="true" aria-label="Cargando el perfil" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2, 3].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : puedeEditar ? (
        <FormularioPerfil perfil={actual} rubros={rubros.datos!} onGuardado={setActual} />
      ) : (
        <PerfilSoloLectura perfil={actual} />
      )}
    </div>
  )
}

function AvisoIncompleto({ perfil, puedeEditar }: { perfil: PerfilEmpresa; puedeEditar: boolean }) {
  if (perfil.perfilCompleto) return null
  return (
    <Aviso tipo="advertencia" titulo="El perfil está incompleto">
      {puedeEditar
        ? 'Completa el rubro y el número de trabajadores permanentes.'
        : 'Falta el rubro o el número de trabajadores permanentes. Pide a quien administra la empresa que lo complete.'}
    </Aviso>
  )
}

function FormularioPerfil({
  perfil,
  rubros,
  onGuardado,
}: {
  perfil: PerfilEmpresa
  rubros: RubroRegistro[]
  onGuardado: (perfil: PerfilEmpresa) => void
}) {
  const [guardado, setGuardado] = useState(false)
  const valores = (p: PerfilEmpresa) => ({
    razonSocial: p.razonSocial,
    rubroCodigo: rubros.find((r) => r.nombre === p.rubroNombre)?.codigo ?? '',
    trabajadoresPermanentes: p.trabajadoresPermanentes?.toString() ?? '',
    direccion: p.direccion ?? '',
    actividadEconomica: p.actividadEconomica ?? '',
  })
  const original = valores(perfil)

  const {
    register,
    reset,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(perfilEmpresaEsquema, {
    defaultValues: original,
    erroresPorCodigo: {
      'empresas.rubro_invalido': { campo: 'rubroCodigo', mensaje: 'Ese rubro ya no está disponible. Elige otro.' },
    },
  })

  const opcionesRubro = rubros.map((r) => ({ valor: r.codigo, texto: r.nombre }))
  const campoRubro = {
    etiqueta: 'Rubro',
    required: true,
    registro: register('rubroCodigo', { onChange: () => setGuardado(false) }),
    opciones: opcionesRubro,
    error: errors.rubroCodigo,
    ayuda: 'Cambiarlo cambia las categorías de riesgo que aparecen al crear una encuesta nueva.',
  }
  const registrar = (campo: 'razonSocial' | 'trabajadoresPermanentes' | 'direccion' | 'actividadEconomica') =>
    register(campo, { onChange: () => setGuardado(false) })

  return (
    <form
      noValidate
      className="grid gap-6"
      onSubmit={enviar(async (datos) => {
        // Solo se envía lo que cambia (PATCH parcial).
        const cambios: ActualizarPerfilRequest = {}
        if (datos.razonSocial !== original.razonSocial) cambios.razonSocial = datos.razonSocial
        if (datos.rubroCodigo !== original.rubroCodigo) cambios.rubroCodigo = datos.rubroCodigo
        if (String(datos.trabajadoresPermanentes) !== original.trabajadoresPermanentes)
          cambios.trabajadoresPermanentes = datos.trabajadoresPermanentes
        if (datos.direccion !== original.direccion) cambios.direccion = datos.direccion
        if (datos.actividadEconomica !== original.actividadEconomica) cambios.actividadEconomica = datos.actividadEconomica
        if (Object.keys(cambios).length === 0) return
        const nuevo = await actualizarPerfil(cambios)
        sesionApi.actualizarRazonSocial(nuevo.razonSocial)
        onGuardado(nuevo)
        reset(valores(nuevo))
        setGuardado(true)
      })}
    >
      <div role="status" className="contents">{guardado && <Aviso tipo="exito">Se guardaron los cambios del perfil.</Aviso>}</div>
      <AvisoIncompleto perfil={perfil} puedeEditar />
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}

      <section className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6" aria-labelledby="titulo-identificacion">
        <h2 id="titulo-identificacion" className="text-md font-semibold">
          Identificación
        </h2>
        <CampoTexto etiqueta="Razón social" required autoComplete="organization" registro={registrar('razonSocial')} error={errors.razonSocial} />
        <dl>
          <Dato etiqueta="RTN" ayuda="No se puede cambiar: con él se inicia sesión en la empresa.">
            <span className="tabular-nums">{perfil.rtn}</span>
          </Dato>
        </dl>
      </section>

      <section className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6" aria-labelledby="titulo-actividad">
        <h2 id="titulo-actividad" className="text-md font-semibold">
          Actividad
        </h2>
        {opcionesRubro.length <= MAXIMO_RADIOS ? <GrupoRadio {...campoRubro} /> : <CampoSelect {...campoRubro} />}
        <CampoTexto
          etiqueta="Trabajadores permanentes"
          required
          inputMode="numeric"
          autoComplete="off"
          registro={registrar('trabajadoresPermanentes')}
          error={errors.trabajadoresPermanentes}
          ayuda="Personas con contrato permanente, sin contar temporales."
        />
        <CampoTexto
          etiqueta="Actividad económica"
          autoComplete="off"
          registro={registrar('actividadEconomica')}
          error={errors.actividadEconomica}
          ayuda="Por ejemplo: Venta de materiales de construcción."
        />
        <CampoTexto etiqueta="Dirección" autoComplete="street-address" registro={registrar('direccion')} error={errors.direccion} />
      </section>

      <div className="flex justify-end">
        <Boton type="submit" disabled={!isDirty} cargando={isSubmitting} textoCargando="Guardando…">
          Guardar cambios
        </Boton>
      </div>
    </form>
  )
}

function PerfilSoloLectura({ perfil }: { perfil: PerfilEmpresa }) {
  const vacio = <span className="text-text-tertiary">Sin registrar</span>
  return (
    <>
      <AvisoIncompleto perfil={perfil} puedeEditar={false} />
      <dl className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
        <Dato etiqueta="Razón social">{perfil.razonSocial}</Dato>
        <Dato etiqueta="RTN">
          <span className="tabular-nums">{perfil.rtn}</span>
        </Dato>
        <Dato etiqueta="Rubro">{perfil.rubroNombre ?? vacio}</Dato>
        <Dato etiqueta="Trabajadores permanentes">
          {perfil.trabajadoresPermanentes !== null ? <span className="tabular-nums">{perfil.trabajadoresPermanentes}</span> : vacio}
        </Dato>
        <Dato etiqueta="Actividad económica">{perfil.actividadEconomica ?? vacio}</Dato>
        <Dato etiqueta="Dirección">{perfil.direccion ?? vacio}</Dato>
      </dl>
      <p className="text-text-secondary">Solo quien administra la empresa puede cambiar estos datos.</p>
    </>
  )
}

/** Dato de solo lectura con su etiqueta (como un campo, sin control). */
function Dato({ etiqueta, ayuda, children }: { etiqueta: string; ayuda?: string; children: ReactNode }) {
  return (
    <div className="grid gap-1">
      <dt className="text-sm font-medium">{etiqueta}</dt>
      <dd>{children}</dd>
      {ayuda && <dd className="text-xs text-text-secondary">{ayuda}</dd>}
    </div>
  )
}
