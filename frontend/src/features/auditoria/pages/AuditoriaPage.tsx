import { ChevronLeft, ChevronRight, History, Search, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoFecha } from '@/shared/components/ui/CampoFecha'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { hoyHonduras, textoAIso } from '@/shared/validacion'
import { listarAuditoria } from '../api/auditoria'
import { destinoDeRegistro, ENTIDADES_OPCIONES, etiquetaAccion, etiquetaEntidad, resumirDetalle } from '../etiquetas'
import type { FiltrosAuditoria, RegistroAuditoria } from '../types/auditoria'

const TAMANO_PAGINA = 25
const FILTROS_VACIOS = { buscar: '', entidad: '' }

const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

const fechaHora = (iso: string) =>
  new Date(iso).toLocaleString('es-HN', {
    timeZone: 'America/Tegucigalpa',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })

/**
 * HU-099: consulta cronológica de solo lectura del registro inalterable de auditoría de la empresa.
 * Solo acceden administrador de empresa y responsable SST; el aislamiento y la inalterabilidad los garantiza la API.
 */
export function AuditoriaPage() {
  const peticion = usePeticion(listarAuditoria)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { register, control, reset } = useForm({ defaultValues: FILTROS_VACIOS })
  const valores = useWatch({ control })
  const [buscar, setBuscar] = useState('')
  const [desdeTexto, setDesdeTexto] = useState('')
  const [hastaTexto, setHastaTexto] = useState('')
  const [pagina, setPagina] = useState(1)
  const hoy = hoyHonduras()

  useEffect(() => {
    const t = setTimeout(() => {
      setBuscar((valores.buscar ?? '').trim())
      setPagina(1)
    }, 300)
    return () => clearTimeout(t)
  }, [valores.buscar])

  const desdeIso = desdeTexto ? textoAIso(desdeTexto) : null
  const hastaIso = hastaTexto ? textoAIso(hastaTexto) : null
  const errorDesde = desdeTexto && !desdeIso ? 'Usa el formato dd/mm/aaaa.' : undefined
  const errorHasta =
    hastaTexto && !hastaIso
      ? 'Usa el formato dd/mm/aaaa.'
      : desdeIso && hastaIso && hastaIso < desdeIso
        ? 'La fecha final no puede ser anterior a la inicial.'
        : undefined

  const rangoValido = !errorDesde && !errorHasta
  const claveFiltros = JSON.stringify({
    buscar: buscar || undefined,
    entidad: valores.entidad || undefined,
    desde: desdeIso ?? undefined,
    hasta: hastaIso ?? undefined,
    pagina,
  })

  const filtros = useMemo<FiltrosAuditoria>(() => {
    const parsed = JSON.parse(claveFiltros) as FiltrosAuditoria
    return { ...parsed, tamano: TAMANO_PAGINA }
  }, [claveFiltros])

  const hayFiltros = Boolean(buscar || valores.entidad || desdeTexto || hastaTexto)

  const { ejecutar } = peticion
  const cargar = useCallback(() => {
    if (!rangoValido) return
    void ejecutar(filtros)
  }, [ejecutar, filtros, rangoValido])

  useEffect(cargar, [cargar])

  const quitarFiltros = () => {
    reset(FILTROS_VACIOS)
    setBuscar('')
    setDesdeTexto('')
    setHastaTexto('')
    setPagina(1)
  }

  const datos = peticion.datos

  return (
    <div className="grid w-full max-w-layout grid-cols-1 gap-6">
      <header className="grid gap-1">
        <h1 className="text-xl font-semibold">Auditoría de acciones</h1>
        <p className="text-text-secondary">
          Registro cronológico e inalterable de las acciones de gestión realizadas en la empresa.
        </p>
      </header>

      <form
        onSubmit={(e) => e.preventDefault()}
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:grid-cols-2 lg:grid-cols-4"
      >
        <CampoTexto
          etiqueta="Buscar"
          type="search"
          placeholder="Usuario, acción o detalle…"
          registro={register('buscar')}
        />
        <CampoSelect
          etiqueta="Módulo / entidad"
          todos="Todas"
          registro={register('entidad', {
            onChange: () => setPagina(1),
          })}
          opciones={ENTIDADES_OPCIONES}
        />
        <CampoFecha
          etiqueta="Desde"
          valor={desdeTexto}
          onChange={(v) => {
            setDesdeTexto(v)
            setPagina(1)
          }}
          maximo={hoy}
          error={errorDesde}
        />
        <CampoFecha
          etiqueta="Hasta"
          valor={hastaTexto}
          onChange={(v) => {
            setHastaTexto(v)
            setPagina(1)
          }}
          minimo={desdeIso ?? undefined}
          maximo={hoy}
          error={errorHasta}
        />
      </form>

      {peticion.error && datos && <Aviso tipo="error">{peticion.error.mensaje}</Aviso>}

      {peticion.error && !datos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar el registro de auditoría</h2>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !datos ? (
        <div aria-busy="true" aria-label="Cargando registros de auditoría" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2, 3].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : datos.items.length === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          {hayFiltros ? (
            <>
              <Search aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
              <h2 className="text-md font-semibold">No hay registros con esos filtros</h2>
              <p className="text-text-secondary">Prueba ajustando el texto, la entidad o el rango de fechas.</p>
              <button
                type="button"
                className="inline-flex min-h-control-touch items-center text-sm font-medium text-text-link hover:text-text-link-hover sm:min-h-control"
                onClick={quitarFiltros}
              >
                Quitar filtros
              </button>
            </>
          ) : (
            <>
              <History aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
              <h2 className="text-md font-semibold">Todavía no hay acciones registradas</h2>
              <p className="max-w-layout-narrow text-text-secondary">
                Aquí aparecerán los cambios en accesos, encuestas, matriz de riesgos, plan, eventos y documentos.
              </p>
            </>
          )}
        </section>
      ) : (
        <div className="grid gap-3" aria-busy={peticion.cargando}>
          <div className="flex flex-wrap items-center justify-between gap-2">
            <p className="text-xs text-text-secondary tabular-nums" aria-live="polite">
              {plural(datos.total, 'registro', 'registros')}, del más reciente al más antiguo
            </p>
            {hayFiltros && (
              <button
                type="button"
                className="text-xs font-medium text-text-link hover:text-text-link-hover"
                onClick={quitarFiltros}
              >
                Quitar filtros
              </button>
            )}
          </div>

          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col" className="max-sm:hidden">
                    Fecha y hora
                  </th>
                  <th scope="col">Acción</th>
                  <th scope="col" className="max-md:hidden">
                    Quién
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Entidad y detalle
                  </th>
                </tr>
              </thead>
              <tbody>
                {datos.items.map((item) => (
                  <FilaAuditoria key={item.id} item={item} />
                ))}
              </tbody>
            </table>
          </div>

          {datos.totalPaginas > 1 && (
            <nav aria-label="Paginación de auditoría" className="flex flex-wrap items-center justify-between gap-3 pt-2">
              <span className="text-sm text-text-secondary tabular-nums">
                Página {datos.pagina} de {datos.totalPaginas}
              </span>
              <div className="flex items-center gap-2">
                <Boton
                  variante="secundario"
                  disabled={datos.pagina <= 1 || peticion.cargando}
                  onClick={() => setPagina((p) => Math.max(1, p - 1))}
                >
                  <ChevronLeft aria-hidden size={16} strokeWidth={1.5} />
                  Anterior
                </Boton>
                <Boton
                  variante="secundario"
                  disabled={datos.pagina >= datos.totalPaginas || peticion.cargando}
                  onClick={() => setPagina((p) => Math.min(datos.totalPaginas, p + 1))}
                >
                  Siguiente
                  <ChevronRight aria-hidden size={16} strokeWidth={1.5} />
                </Boton>
              </div>
            </nav>
          )}
        </div>
      )}
    </div>
  )
}

function FilaAuditoria({ item }: { item: RegistroAuditoria }) {
  const infoAccion = etiquetaAccion(item.accion)
  const fragmentos = resumirDetalle(item.detalle)
  const destino = destinoDeRegistro(item)

  return (
    <tr>
      <td className="whitespace-nowrap text-sm text-text-secondary tabular-nums max-sm:hidden">{fechaHora(item.fecha)}</td>
      <td className="envolver">
        <div className="flex flex-wrap items-center gap-2">
          <Insignia tipo={infoAccion.tono}>{infoAccion.texto}</Insignia>
        </div>
        <div className="mt-1 grid gap-1 text-xs text-text-secondary md:hidden">
          <span className="font-medium text-text-primary">{item.actorNombre}</span>
          {item.actorCorreo && <span>{item.actorCorreo}</span>}
        </div>
        <div className="mt-1 grid gap-1 text-xs text-text-secondary sm:hidden">
          <span>{etiquetaEntidad(item.entidad)}</span>
          {fragmentos.length > 0 && <span>{fragmentos.join(' · ')}</span>}
          <span className="tabular-nums">{fechaHora(item.fecha)}</span>
          {destino && (
            <Link to={destino.to} state={destino.state} className="font-medium text-text-link hover:text-text-link-hover">
              {destino.texto}
            </Link>
          )}
        </div>
      </td>
      <td className="envolver max-md:hidden">
        <div className="text-sm font-medium text-text-primary">{item.actorNombre}</div>
        {item.actorCorreo && <div className="text-xs text-text-secondary">{item.actorCorreo}</div>}
      </td>
      <td className="envolver max-sm:hidden">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <span className="text-sm font-medium text-text-primary">{etiquetaEntidad(item.entidad)}</span>
          {destino && (
            <Link to={destino.to} state={destino.state} className="text-xs font-medium text-text-link hover:text-text-link-hover">
              {destino.texto}
            </Link>
          )}
        </div>
        {fragmentos.length > 0 && <div className="mt-1 text-sm text-text-secondary">{fragmentos.join(' · ')}</div>}
      </td>
    </tr>
  )
}
