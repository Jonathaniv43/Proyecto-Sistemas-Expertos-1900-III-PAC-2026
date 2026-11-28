import { zodResolver } from '@hookform/resolvers/zod'
import { ChartColumn, ShieldCheck, TriangleAlert } from 'lucide-react'
import { useEffect, useMemo, type ReactNode } from 'react'
import { useController, useForm, useWatch } from 'react-hook-form'
import { Indicador } from '@/shared/components/datos/Indicador'
import { ListaMedidores } from '@/shared/components/datos/ListaMedidores'
import { Boton } from '@/shared/components/ui/Boton'
import { Volver } from '@/shared/components/ui/Volver'
import { CampoFecha } from '@/shared/components/ui/CampoFecha'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { fechaCorta, hoyHonduras, isoATexto } from '@/shared/validacion'
import { obtenerEstadisticas } from '../api/eventos'
import { NOMBRE_TIPO } from '../etiquetas'
import { periodoEsquema, sumarMeses } from '../schemas/periodo'
import type { EstadisticasEventos } from '../types/eventos'

const formatoPorcentaje = new Intl.NumberFormat('es-HN', { maximumFractionDigits: 1 })
const porcentaje = (n: number, total: number) => (total === 0 ? 0 : (n * 100) / total)
const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`
const nombreMes = (anio: number, mes: number) =>
  new Date(Date.UTC(anio, mes - 1, 15)).toLocaleDateString('es-HN', { month: 'short', year: 'numeric', timeZone: 'UTC' })

/** Periodos rápidos: se calculan con la fecha de Honduras. */
function periodos() {
  const hoy = hoyHonduras()
  const anio = Number(hoy.slice(0, 4))
  return [
    { texto: 'Últimos 12 meses', desde: sumarMeses(`${hoy.slice(0, 8)}01`, -11), hasta: hoy },
    { texto: 'Este año', desde: `${anio}-01-01`, hasta: hoy },
    { texto: 'El año pasado', desde: `${anio - 1}-01-01`, hasta: `${anio - 1}-12-31` },
  ]
}

/**
 * HU-052: cuántos eventos se reportaron en un periodo, por tipo, gravedad y mes, para detectar patrones (art. 47.e).
 * Solo cifras: los reportes son anónimos (D11) y no se muestra ninguna descripción ni lugar. Los descartados no
 * cuentan (RN-28). Por defecto, los últimos 12 meses; el periodo se aplica al cambiarlo, sin botón.
 */
export function EstadisticasEventosPage() {
  const inicial = periodos()[0]
  const { control, trigger, setValue, formState } = useForm({
    resolver: zodResolver(periodoEsquema),
    mode: 'onChange',
    defaultValues: { desde: isoATexto(inicial.desde), hasta: isoATexto(inicial.hasta) },
  })
  const { field: desde } = useController({ control, name: 'desde' })
  const { field: hasta } = useController({ control, name: 'hasta' })
  const valores = useWatch({ control })
  const periodo = useMemo(() => {
    const r = periodoEsquema.safeParse({ desde: valores.desde ?? '', hasta: valores.hasta ?? '' })
    return r.success ? r.data : null
  }, [valores.desde, valores.hasta])

  // "Hasta" depende de "desde" (no antes, como máximo 24 meses): se revisa de nuevo al cambiar la inicial.
  useEffect(() => {
    if (valores.hasta) void trigger('hasta')
  }, [valores.desde, valores.hasta, trigger])

  const peticion = usePeticion(obtenerEstadisticas)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { ejecutar } = peticion
  const cargar = () => periodo && void ejecutar(periodo)
  useEffect(() => {
    if (periodo) void ejecutar(periodo)
  }, [ejecutar, periodo])

  const elegir = (p: { desde: string; hasta: string }) => {
    setValue('desde', isoATexto(p.desde), { shouldValidate: true })
    setValue('hasta', isoATexto(p.hasta), { shouldValidate: true })
  }
  const hoy = hoyHonduras()
  const datos = peticion.datos

  return (
    <div className="grid w-full max-w-layout grid-cols-1 gap-6">
      <Volver a="/eventos" texto="Volver a eventos" titulo="Estadísticas de eventos" ancho="layout" />
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Estadísticas de eventos</h1>
        <p className="flex items-start gap-2 text-text-secondary">
          <ShieldCheck aria-hidden size={16} strokeWidth={1.5} className="mt-1 shrink-0" />
          Solo cifras: no muestran quién reportó ni qué se escribió. Los reportes descartados no cuentan.
        </p>
      </header>

      <form role="search" aria-label="Periodo" className="grid gap-3" onSubmit={(e) => e.preventDefault()}>
        <div className="grid gap-4 sm:grid-cols-2 lg:max-w-layout-narrow">
          <CampoFecha
            etiqueta="Desde"
            required
            valor={desde.value}
            onChange={desde.onChange}
            onBlur={desde.onBlur}
            error={formState.errors.desde?.message}
            maximo={hoy}
          />
          <CampoFecha
            etiqueta="Hasta"
            required
            valor={hasta.value}
            onChange={hasta.onChange}
            onBlur={hasta.onBlur}
            error={formState.errors.hasta?.message}
            maximo={hoy}
          />
        </div>
        <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
          <span className="text-text-secondary">Ver:</span>
          {periodos().map((p) => (
            <button
              key={p.texto}
              type="button"
              className="inline-flex min-h-control-touch items-center font-medium text-text-link hover:text-text-link-hover sm:min-h-control"
              onClick={() => elegir(p)}
            >
              {p.texto}
            </button>
          ))}
        </div>
      </form>

      {peticion.error && !datos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar las estadísticas</h2>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !datos ? (
        <div aria-busy="true" aria-label="Cargando las estadísticas" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <Resultados datos={datos} cargando={peticion.cargando} />
      )}
    </div>
  )
}

function Resultados({ datos, cargando }: { datos: EstadisticasEventos; cargando: boolean }) {
  const { total, graves } = datos
  const masFrecuente = useMemo(() => {
    const max = Math.max(...datos.porTipo.map((t) => t.cantidad))
    return max === 0 ? null : { tipos: datos.porTipo.filter((t) => t.cantidad === max).map((t) => NOMBRE_TIPO[t.tipo]), cantidad: max }
  }, [datos.porTipo])
  const fila = (clave: string, etiqueta: string, cantidad: number, gravesFila?: number) => ({
    clave,
    etiqueta,
    cantidad,
    porcentaje: porcentaje(cantidad, total),
    detalle: gravesFila ? plural(gravesFila, 'grave', 'graves') : undefined,
  })

  return (
    <div className="grid gap-6" aria-busy={cargando}>
      <dl className="grid gap-3 sm:grid-cols-3" aria-live="polite">
        <Indicador etiqueta="Eventos" valor={total} detalle={`Del ${fechaCorta(datos.desde)} al ${fechaCorta(datos.hasta)}`} />
        <Indicador
          etiqueta="Graves"
          valor={graves}
          detalle={total > 0 ? `${formatoPorcentaje.format(porcentaje(graves, total))} % de los eventos` : undefined}
        />
        <Indicador
          etiqueta="Tipo más frecuente"
          valor={<span className="text-md">{masFrecuente ? masFrecuente.tipos.join(' y ') : 'Ninguno'}</span>}
          detalle={masFrecuente ? plural(masFrecuente.cantidad, 'evento', 'eventos') : undefined}
        />
      </dl>

      {total === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <ChartColumn aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">No hay eventos en este periodo</h2>
          <p className="max-w-layout-narrow text-text-secondary">Prueba con un periodo más largo.</p>
        </section>
      ) : (
        <section className="grid gap-8 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
          <Grupo titulo="Por tipo">
            <ListaMedidores
              unidad={['evento', 'eventos']}
              filas={datos.porTipo.map((t) => fila(t.tipo, NOMBRE_TIPO[t.tipo], t.cantidad, t.graves))}
            />
          </Grupo>
          <Grupo titulo="Por gravedad">
            <ListaMedidores
              unidad={['evento', 'eventos']}
              filas={[fila('graves', 'Graves', graves), fila('no_graves', 'No graves', total - graves)]}
            />
          </Grupo>
          <Grupo titulo="Por mes" descripcion="Según cuándo ocurrió cada evento.">
            <ListaMedidores
              unidad={['evento', 'eventos']}
              filas={datos.porMes.map((m) => fila(`${m.anio}-${m.mes}`, nombreMes(m.anio, m.mes), m.cantidad, m.graves))}
            />
          </Grupo>
        </section>
      )}
    </div>
  )
}

function Grupo({ titulo, descripcion, children }: { titulo: string; descripcion?: string; children: ReactNode }) {
  return (
    <div className="grid gap-3">
      <div className="grid gap-1">
        <h2 className="text-md font-semibold">{titulo}</h2>
        {descripcion && <p className="text-sm text-text-secondary">{descripcion}</p>}
      </div>
      {children}
    </div>
  )
}
