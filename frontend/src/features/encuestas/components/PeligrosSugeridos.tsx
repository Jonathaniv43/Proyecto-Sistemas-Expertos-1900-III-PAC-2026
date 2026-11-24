import { SearchCheck, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { extraerError, type ErrorApi } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { DialogoConfirmacion } from '@/shared/components/ui/DialogoConfirmacion'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { obtenerPeligrosSugeridos, revisarPeligroSugerido } from '../api/encuestas'
import type { EstadoRiesgo, PeligroRevisado, PeligrosSugeridos as Datos } from '../types/encuestas'
import { ConfirmarPeligroModal, type PeligroPorConfirmar } from './ConfirmarPeligroModal'

const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`
const formatoPorcentaje = new Intl.NumberFormat('es-HN', { maximumFractionDigits: 1 })

/** Badge del riesgo que registró la sugerencia (design-system.md § Badges: peligro sugerido). */
function EstadoSugerencia({ estado }: { estado: EstadoRiesgo }) {
  if (estado === 'confirmado') return <Insignia tipo="exito">En la matriz</Insignia>
  return <Insignia tipo="neutro">{estado === 'descartado' ? 'Descartado' : 'Por revisar'}</Insignia>
}

/**
 * HU-033: peligros que sugieren las respuestas (RN-13). Un peligro aparece cuando al menos una
 * respuesta eligió una opción que indica exposición en una pregunta asociada a ese peligro. Cada
 * sugerencia muestra cuántas respuestas la respaldan y de qué preguntas salió.
 *
 * HU-035: cada sugerencia "Por revisar" se confirma (pasa a la matriz, con la actividad opcional) o
 * se descarta (diálogo de confirmación). Se revisa una sola vez (RN-18).
 *
 * Tiene su propia carga: si falla, el resto de los resultados se sigue viendo.
 */
export function PeligrosSugeridos({ encuestaId }: { encuestaId: string }) {
  const peticion = usePeticion(obtenerPeligrosSugeridos)
  const cargandoVisible = useCargaVisible(peticion.cargando)
  // Revisiones hechas en esta pantalla: se muestran sin volver a pedir la lista.
  const [revisados, setRevisados] = useState<Record<string, PeligroRevisado>>({})
  const [aviso, setAviso] = useState<{ tipo: 'exito' | 'error'; texto: string }>()
  const [porConfirmar, setPorConfirmar] = useState<PeligroPorConfirmar>()
  const [porDescartar, setPorDescartar] = useState<PeligroPorConfirmar>()
  const [descartando, setDescartando] = useState(false)

  const { ejecutar } = peticion
  // Las revisiones anotadas no se borran al recargar: son definitivas y el servidor las devuelve igual.
  const cargar = useCallback(() => {
    void ejecutar(encuestaId)
  }, [ejecutar, encuestaId])
  useEffect(cargar, [cargar])

  const anotar = (revisado: PeligroRevisado, texto: string) => {
    setRevisados((previos) => ({ ...previos, [revisado.riesgoId]: revisado }))
    setAviso({ tipo: 'exito', texto })
  }
  // Otra persona ya la revisó (409): se recarga para ver su decisión.
  const yaRevisado = (mensaje: string) => {
    setPorConfirmar(undefined)
    setPorDescartar(undefined)
    setAviso({ tipo: 'error', texto: `${mensaje} Actualizamos la lista.` })
    cargar()
  }

  const datos = peticion.datos && {
    ...peticion.datos,
    sugerencias: peticion.datos.sugerencias.map((s) => {
      const r = revisados[s.riesgoSugeridoId]
      return r ? { ...s, estado: r.estado, actividad: r.actividad } : s
    }),
  }

  return (
    <section
      aria-labelledby="titulo-peligros-sugeridos"
      className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
    >
      <header className="grid gap-1">
        <h2 id="titulo-peligros-sugeridos" className="text-md font-semibold">
          Peligros sugeridos
        </h2>
        <p className="text-text-secondary">
          Salen de las respuestas que indican exposición a un peligro, en todas las versiones de la encuesta. Los que
          confirmes pasan a la <Link to="/riesgos">matriz de riesgos</Link>.
        </p>
      </header>
      <div role="status" className="contents">
        {aviso && <Aviso tipo={aviso.tipo}>{aviso.texto}</Aviso>}
      </div>
      <Contenido
        peticion={{ ...peticion, datos }}
        cargandoVisible={cargandoVisible}
        onReintentar={cargar}
        onConfirmar={(s) => {
          setAviso(undefined)
          setPorConfirmar({ encuestaId, riesgoId: s.riesgoSugeridoId, nombre: s.peligroNombre })
        }}
        onDescartar={(s) => {
          setAviso(undefined)
          setPorDescartar({ encuestaId, riesgoId: s.riesgoSugeridoId, nombre: s.peligroNombre })
        }}
      />

      <ConfirmarPeligroModal
        peligro={porConfirmar}
        onCerrar={() => setPorConfirmar(undefined)}
        onConfirmado={(revisado) => {
          anotar(revisado, `${porConfirmar?.nombre} pasó a la matriz de riesgos.`)
          setPorConfirmar(undefined)
        }}
        onYaRevisado={yaRevisado}
      />
      <DialogoConfirmacion
        abierto={porDescartar !== undefined}
        titulo={`¿Descartar el peligro ${porDescartar?.nombre ?? ''}?`}
        textoConfirmar="Descartar"
        textoCargando="Descartando…"
        cargando={descartando}
        onCancelar={() => setPorDescartar(undefined)}
        onConfirmar={async () => {
          if (!porDescartar) return
          setDescartando(true)
          try {
            const revisado = await revisarPeligroSugerido(porDescartar.encuestaId, porDescartar.riesgoId, { estado: 'descartado' })
            anotar(revisado, `Se descartó ${porDescartar.nombre}: no pasará a la matriz.`)
            setPorDescartar(undefined)
          } catch (causa) {
            const error = extraerError(causa)
            if (error.codigo === 'encuestas.sugerencia_ya_revisada') yaRevisado(error.mensaje)
            else {
              setPorDescartar(undefined)
              setAviso({ tipo: 'error', texto: `No se pudo descartar. ${error.mensaje}` })
            }
          } finally {
            setDescartando(false)
          }
        }}
      >
        No pasará a la matriz de riesgos. Esta decisión no se cambia después.
      </DialogoConfirmacion>
    </section>
  )
}

type Sugerencia = Datos['sugerencias'][number]

interface ContenidoProps {
  peticion: { cargando: boolean; datos: Datos | undefined; error: ErrorApi | undefined }
  cargandoVisible: boolean
  onReintentar: () => void
  onConfirmar: (s: Sugerencia) => void
  onDescartar: (s: Sugerencia) => void
}

function Contenido({ peticion, cargandoVisible, onReintentar, onConfirmar, onDescartar }: ContenidoProps) {
  if (peticion.error) {
    return (
      <div className="flex flex-col items-center gap-3 px-4 py-8 text-center">
        <TriangleAlert aria-hidden size={24} strokeWidth={1.5} className="text-error-icon" />
        <p className="text-text-secondary">No se pudieron cargar los peligros sugeridos. {peticion.error.mensaje}</p>
        <Boton variante="secundario" onClick={onReintentar}>
          Reintentar
        </Boton>
      </div>
    )
  }

  const datos = peticion.datos
  if (peticion.cargando || !datos) {
    return (
      <div aria-busy="true" aria-label="Cargando peligros sugeridos" className="grid gap-3">
        {cargandoVisible &&
          [0, 1].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
      </div>
    )
  }

  if (datos.detalleOculto) {
    return (
      <p className="text-text-secondary">
        Las sugerencias aparecen con {datos.minimoRespuestas} respuestas, para proteger el anonimato. Van{' '}
        {plural(datos.totalRespuestas, 'respuesta', 'respuestas')}.
      </p>
    )
  }

  if (datos.sugerencias.length === 0) {
    return (
      <div className="flex flex-col items-center gap-2 px-4 py-8 text-center">
        <SearchCheck aria-hidden size={24} strokeWidth={1.5} className="text-text-tertiary" />
        <p className="font-medium">Ninguna respuesta indica exposición a un peligro</p>
        <p className="text-text-secondary">Si llegan respuestas nuevas que lo indiquen, el peligro aparecerá aquí.</p>
      </div>
    )
  }

  const pendientes = datos.sugerencias.filter((s) => s.estado === 'sugerido').length

  return (
    <>
      <p className="text-text-secondary">
        {pendientes > 0
          ? `${plural(pendientes, 'queda por revisar', 'quedan por revisar')}: confirma los que aplican a la empresa y descarta los que no.`
          : 'Ya revisaste todas las sugerencias.'}
      </p>
      <ul className="grid gap-6" aria-label="Peligros sugeridos">
        {datos.sugerencias.map((s, i) => {
          const porcentaje = (s.respuestasConExposicion * 100) / datos.totalRespuestas
          return (
            <li key={s.riesgoSugeridoId} className={`grid gap-3 ${i > 0 ? 'border-t border-border-default pt-6' : ''}`}>
              <div className="flex flex-wrap items-center justify-between gap-2">
                <h3 className="font-semibold">{s.peligroNombre}</h3>
                <EstadoSugerencia estado={s.estado} />
              </div>
              {s.estado === 'confirmado' && s.actividad && (
                <p>
                  <span className="text-text-secondary">Actividad: </span>
                  {s.actividad}
                </p>
              )}
              <div className="grid gap-2">
                <p className="text-text-secondary">
                  Lo indican {s.respuestasConExposicion} de {plural(datos.totalRespuestas, 'respuesta', 'respuestas')} ·{' '}
                  <span className="tabular-nums">{formatoPorcentaje.format(porcentaje)} %</span>
                </p>
                <span aria-hidden className="flex h-2 w-full overflow-hidden rounded-full bg-dato-pista">
                  <span className="h-full rounded-full bg-dato-serie" style={{ width: `${Math.max(Math.min(porcentaje, 100), 1)}%` }} />
                </span>
              </div>
              <div className="grid gap-1">
                <p className="text-xs text-text-secondary">
                  {s.preguntas.length === 1 ? 'Pregunta que lo originó' : 'Preguntas que lo originaron'}
                </p>
                <ul className="grid gap-2 sm:gap-1">
                  {s.preguntas.map((p) => (
                    <li key={p.preguntaId} className="flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between sm:gap-4">
                      <span>{p.texto}</span>
                      <span className="shrink-0 text-sm whitespace-nowrap text-text-secondary tabular-nums">
                        {plural(p.respuestasConExposicion, 'respuesta', 'respuestas')}
                      </span>
                    </li>
                  ))}
                </ul>
              </div>
              {s.estado === 'sugerido' && (
                <div className="flex flex-col gap-2 sm:flex-row">
                  <Boton variante="secundario" aria-label={`Confirmar ${s.peligroNombre}`} onClick={() => onConfirmar(s)}>
                    Confirmar…
                  </Boton>
                  <Boton variante="secundario" aria-label={`Descartar ${s.peligroNombre}`} onClick={() => onDescartar(s)}>
                    Descartar…
                  </Boton>
                </div>
              )}
            </li>
          )
        })}
      </ul>
    </>
  )
}
