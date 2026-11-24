import { ChevronDown, Plus } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link, useLocation, useSearchParams } from 'react-router-dom'
import { clasesBoton, clasesControl } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { estadoPregunta, listarCategorias, listarPeligros, listarPreguntas, listarRubros } from '../api/catalogos'
import { AvisoMensaje } from '../components/AvisoMensaje'
import { BotonEstado, EstadoActivo } from '../components/Estado'
import { EstadoCarga } from '../components/EstadoCarga'
import { NOMBRE_TIPO_RESPUESTA } from '../etiquetas'
import { opcionesCategoria } from '../opciones'
import { usePuedeEditar } from '../permisos'
import { useCambioEstado } from '../useCambioEstado'

/** Lo que el editor de preguntas deja en la navegación al guardar. */
export interface EstadoPreguntaGuardada {
  guardada?: string
}

/**
 * HU-023: preguntas exploratorias de una categoría, en el orden en que salen en la encuesta. Cada una tiene su tipo
 * (D26) y, si detecta un peligro, qué opciones indican exposición (HU-033). Cambiarlas no altera las encuestas ya
 * generadas, que guardan su copia (RN-12).
 */
export function PreguntasPage() {
  const [params, setParams] = useSearchParams()
  const categoriaId = params.get('categoria') ?? ''
  const { state } = useLocation()
  const guardada = (state as EstadoPreguntaGuardada | null)?.guardada
  const puedeEditar = usePuedeEditar()
  const rubros = usePeticion(listarRubros)
  const categorias = usePeticion(listarCategorias)
  const peligros = usePeticion(listarPeligros)
  const preguntas = usePeticion(listarPreguntas)

  const { ejecutar: cargarRubros } = rubros
  const { ejecutar: cargarCategorias } = categorias
  const { ejecutar: cargarPeligros } = peligros
  const { ejecutar: cargarPreguntas } = preguntas
  const recargarPreguntas = useCallback(() => {
    if (categoriaId) void cargarPreguntas(categoriaId)
  }, [cargarPreguntas, categoriaId])
  const cargar = useCallback(() => {
    void cargarRubros()
    void cargarCategorias()
    void cargarPeligros()
  }, [cargarRubros, cargarCategorias, cargarPeligros])
  useEffect(cargar, [cargar])
  useEffect(recargarPreguntas, [recargarPreguntas])
  const { mensaje, ocupado, cambiar } = useCambioEstado(recargarPreguntas)

  const categoria = categorias.datos?.find((c) => c.id === categoriaId)
  const nombrePeligro = (id: string | null) => peligros.datos?.find((p) => p.id === id)?.nombre
  const lista = categoriaId && preguntas.datos?.every((p) => p.categoriaRiesgoId === categoriaId) ? preguntas.datos : undefined

  return (
    <section aria-labelledby="titulo-preguntas" className="grid gap-4">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h2 id="titulo-preguntas" className="text-md font-semibold">
            Preguntas exploratorias
          </h2>
          <p className="text-text-secondary">Lo que se pregunta en la encuesta de cada categoría, en el orden en que aparece.</p>
        </div>
        {puedeEditar && (
          <Link to={`/catalogos/preguntas/nueva${categoriaId ? `?categoria=${categoriaId}` : ''}`} className={clasesBoton('secundario')}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Nueva pregunta
          </Link>
        )}
      </div>
      <EstadoCarga
        cargando={rubros.cargando || categorias.cargando}
        error={rubros.error ?? categorias.error ?? peligros.error}
        hayDatos={!!rubros.datos && !!categorias.datos}
        que="las categorías"
        onReintentar={cargar}
      >
        <div className="grid gap-2 sm:max-w-md">
          <label htmlFor="filtro-categoria" className="text-sm font-medium">
            Categoría
          </label>
          {/* Controlado por la URL (?categoria=), así el enlace "Preguntas" de una categoría llega filtrado. */}
          <div className="relative">
            <select
              id="filtro-categoria"
              className={`${clasesControl(false)} cursor-pointer appearance-none pr-8`}
              value={categoriaId}
              onChange={(e) => setParams(e.target.value ? { categoria: e.target.value } : {}, { replace: true })}
            >
              <option value="">Elige una categoría…</option>
              {opcionesCategoria(categorias.datos ?? [], rubros.datos ?? []).map((o) => (
                <option key={o.valor} value={o.valor}>
                  {o.texto}
                </option>
              ))}
            </select>
            <ChevronDown
              aria-hidden
              size={16}
              strokeWidth={1.5}
              className="pointer-events-none absolute top-1/2 right-3 -translate-y-1/2 text-text-secondary"
            />
          </div>
        </div>
      </EstadoCarga>
      <div role="status" className="contents">
        {guardada && !mensaje && <AvisoMensaje mensaje={{ tipo: 'exito', texto: `La pregunta «${guardada}» quedó guardada.` }} />}
      </div>
      <AvisoMensaje mensaje={mensaje} />

      {!categoriaId ? (
        <p className="text-text-secondary">Elige una categoría para ver sus preguntas.</p>
      ) : (
        <EstadoCarga
          cargando={preguntas.cargando}
          error={preguntas.error}
          hayDatos={!!lista}
          que="las preguntas"
          onReintentar={recargarPreguntas}
        >
          {lista?.length === 0 ? (
            <p className="text-text-secondary">Esta categoría todavía no tiene preguntas.</p>
          ) : (
            <ol aria-label={`Preguntas de ${categoria?.nombre ?? 'la categoría'}`} className="grid gap-3">
              {lista?.map((p) => (
                <li key={p.id} className="grid gap-2 rounded-md border border-border-default bg-bg-surface p-4">
                  <div className="flex flex-wrap items-start justify-between gap-2">
                    <p className="font-medium">
                      <span className="text-text-secondary tabular-nums">{p.orden}. </span>
                      {p.texto}
                    </p>
                    <EstadoActivo activo={p.activa} />
                  </div>
                  <p className="flex flex-wrap items-center gap-2 text-xs text-text-secondary">
                    {NOMBRE_TIPO_RESPUESTA[p.tipoRespuestaCodigo]}
                    {p.obligatoria ? ' · Obligatoria' : ' · Opcional'}
                    {p.peligroId && <Insignia tipo="neutro">{`Detecta: ${nombrePeligro(p.peligroId) ?? 'peligro'}`}</Insignia>}
                  </p>
                  {p.opciones.length > 0 && (
                    <ul aria-label="Opciones" className="flex flex-wrap gap-2 text-sm">
                      {p.opciones.map((o) => (
                        <li key={o.id} className="rounded-sm border border-border-default px-2 py-1">
                          {o.texto}
                          {o.indicaExposicion && <span className="text-xs text-text-secondary"> · indica exposición</span>}
                        </li>
                      ))}
                    </ul>
                  )}
                  {puedeEditar && (
                    <div className="flex flex-wrap items-center gap-1">
                      <Link to={`/catalogos/preguntas/${p.id}`} className={clasesBoton('secundario')} aria-label={`Editar la pregunta ${p.orden}`}>
                        Editar
                      </Link>
                      <BotonEstado
                        activo={p.activa}
                        nombre={`Pregunta ${p.orden}`}
                        ocupado={ocupado !== undefined}
                        onCambiar={(activo) => void cambiar(p.id, `Pregunta ${p.orden}`, activo, () => estadoPregunta(p.id, activo))}
                      />
                    </div>
                  )}
                </li>
              ))}
            </ol>
          )}
        </EstadoCarga>
      )}
    </section>
  )
}
