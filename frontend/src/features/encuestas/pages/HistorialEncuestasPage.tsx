import { ArrowLeftRight, History, SearchX, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { useScrollRecordado } from '@/shared/hooks/useScrollRecordado'
import { listarHistorialEncuestas } from '../api/encuestas'
import { EstadoEncuesta } from '../components/EstadoEncuesta'
import type { EncuestaHistorial } from '../types/encuestas'

const fecha = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

function textoFecha(e: EncuestaHistorial) {
  if (e.cerradaEn) return { principal: fecha(e.cerradaEn), detalle: 'Cerrada' }
  if (e.ultimaPublicacionEn) return { principal: fecha(e.ultimaPublicacionEn), detalle: 'Última versión publicada' }
  return { principal: fecha(e.creadaEn), detalle: 'Creada' }
}

/**
 * HU-034: encuestas anteriores de la empresa (cerradas o con versiones reemplazadas),
 * mostrando fecha, categorías, versiones, respuestas y acceso directo a resultados, peligros sugeridos y comparación.
 */
export function HistorialEncuestasPage() {
  const historial = usePeticion(listarHistorialEncuestas)
  const cargandoVisible = useCargaVisible(historial.cargando && !historial.datos)
  useScrollRecordado(Boolean(historial.datos))

  const { register, control, reset } = useForm({ defaultValues: { buscar: '' } })
  const escrito = useWatch({ control, name: 'buscar' })
  const [buscar, setBuscar] = useState('')
  useEffect(() => {
    const t = setTimeout(() => setBuscar(escrito.trim()), 300)
    return () => clearTimeout(t)
  }, [escrito])

  const { ejecutar } = historial
  const cargar = useCallback(() => void ejecutar({ buscar }), [ejecutar, buscar])
  useEffect(cargar, [cargar])

  const datos = historial.datos
  const sinHistorial = datos?.length === 0 && !buscar

  return (
    <div className="grid w-full max-w-layout gap-6">
      <Volver a="/encuestas" texto="Volver a encuestas" titulo="Encuestas anteriores" ancho="layout" />

      <header className="flex flex-wrap items-start justify-between gap-4">
        <div className="grid gap-1">
          <h1 className="text-xl font-semibold">Encuestas anteriores</h1>
          <p className="text-text-secondary">
            Encuestas cerradas o con versiones reemplazadas. Puedes revisar sus resultados, peligros sugeridos y comparar periodos.
          </p>
        </div>
        <Link to="/encuestas/comparar" className={clasesBoton('primario')}>
          <ArrowLeftRight aria-hidden size={16} strokeWidth={1.5} />
          Comparar resultados
        </Link>
      </header>

      {!sinHistorial && (
        <form role="search" aria-label="Buscar encuestas anteriores" className="max-w-md" onSubmit={(e) => e.preventDefault()}>
          <CampoTexto
            etiqueta="Buscar"
            type="search"
            autoComplete="off"
            placeholder="Título de la encuesta o categoría"
            registro={register('buscar')}
          />
        </form>
      )}

      {historial.error && !datos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar el historial de encuestas</h2>
          <p className="text-text-secondary">{historial.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !datos ? (
        <div aria-busy="true" aria-label="Cargando encuestas anteriores" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : sinHistorial ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <History aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <div className="grid gap-1">
            <h2 className="text-md font-semibold">Todavía no hay encuestas anteriores</h2>
            <p className="text-text-secondary">
              Las encuestas aparecen aquí cuando se cierran o cuando publicas una nueva versión sobre otra ya publicada.
            </p>
          </div>
          <Link to="/encuestas" className={clasesBoton('secundario')}>
            Ir a encuestas
          </Link>
        </section>
      ) : datos.length === 0 ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <SearchX aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">{`No hay encuestas anteriores para «${buscar}»`}</h2>
          <Boton variante="secundario" onClick={() => reset()}>
            Quitar la búsqueda
          </Boton>
        </section>
      ) : (
        <div className="tabla-contenedor" aria-busy={historial.cargando}>
          <table className="tabla">
            <thead>
              <tr>
                <th scope="col">Encuesta</th>
                <th scope="col" className="max-sm:hidden">
                  Categorías
                </th>
                <th scope="col" className="max-sm:hidden">
                  Fecha
                </th>
                <th scope="col">Respuestas</th>
                <th scope="col" className="max-sm:hidden">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody>
              {datos.map((e) => {
                const f = textoFecha(e)
                const ultimaVersion = e.versiones[0]
                const anteriorVersion = e.versiones[1]
                const rutaComparar =
                  ultimaVersion && anteriorVersion
                    ? `/encuestas/comparar?base=${anteriorVersion.id}&comparar=${ultimaVersion.id}`
                    : ultimaVersion
                      ? `/encuestas/comparar?base=${ultimaVersion.id}`
                      : '/encuestas/comparar'

                return (
                  <tr key={e.id}>
                    <td className="envolver">
                      <div className="grid gap-2">
                        <div className="flex flex-wrap items-center gap-2">
                          <Link to={`/encuestas/${e.id}/resultados`} className="font-medium">
                            {e.titulo}
                          </Link>
                          <EstadoEncuesta estado={e.estado} />
                          {e.versiones.length > 1 && (
                            <Insignia tipo="info">{`${e.versiones.length} versiones`}</Insignia>
                          )}
                        </div>
                        <div className="grid gap-1 text-xs text-text-secondary sm:hidden">
                          <span>
                            {f.detalle}: {f.principal}
                          </span>
                          <span>
                            Categorías: {e.categorias.length > 0 ? e.categorias.join(', ') : 'Sin categorías'}
                          </span>
                          <div className="mt-1 flex flex-wrap gap-3 text-sm">
                            <Link to={`/encuestas/${e.id}/resultados`}>Resultados y peligros</Link>
                            {ultimaVersion && (
                              <Link to={rutaComparar}>
                                {anteriorVersion ? 'Comparar versiones' : 'Comparar'}
                              </Link>
                            )}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="envolver max-sm:hidden">
                      {e.categorias.length > 0 ? (
                        <div className="flex flex-wrap gap-1">
                          {e.categorias.slice(0, 3).map((cat) => (
                            <Insignia key={cat} tipo="neutro">
                              {cat}
                            </Insignia>
                          ))}
                          {e.categorias.length > 3 && (
                            <span className="text-xs text-text-secondary">+{e.categorias.length - 3} más</span>
                          )}
                        </div>
                      ) : (
                        <span className="text-text-secondary">Sin categorías</span>
                      )}
                    </td>
                    <td className="max-sm:hidden">
                      <div className="tabular-nums">{f.principal}</div>
                      <div className="text-xs text-text-secondary">{f.detalle}</div>
                    </td>
                    <td className="tabular-nums">
                      <div>{plural(e.resumen.respuestasAnonimas, 'respuesta', 'respuestas')}</div>
                      <div className="text-xs text-text-secondary">
                        {plural(e.resumen.participaciones, 'participante', 'participantes')}
                      </div>
                    </td>
                    <td className="max-sm:hidden">
                      <div className="flex flex-wrap gap-3 text-sm whitespace-nowrap">
                        <Link to={`/encuestas/${e.id}/resultados`}>Resultados y peligros</Link>
                        {ultimaVersion && (
                          <Link to={rutaComparar}>
                            {anteriorVersion ? 'Comparar versiones' : 'Comparar'}
                          </Link>
                        )}
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
