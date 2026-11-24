import { TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { EtiquetaRiesgo } from '@/shared/riesgo/EtiquetaRiesgo'
import { listarEvaluaciones } from '../api/riesgos'
import { PROBABILIDADES, SEVERIDADES } from '../niveles'
import type { Evaluacion } from '../types/riesgos'

const fechaHora = (iso: string) => new Date(iso).toLocaleString('es-HN', { dateStyle: 'medium', timeStyle: 'short' })
const fecha = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
const TIPOS = { inicial: 'Inicial', residual: 'Residual' } as const

interface HistorialEvaluacionesProps {
  riesgoId: string
  /** Cambia con cada evaluación nueva (las vigentes del riesgo): el historial se vuelve a pedir. */
  version: string
}

/**
 * HU-041: todas las evaluaciones del riesgo, iniciales y residuales, con fecha y autor, la más reciente primero.
 * Ninguna se borra al evaluar de nuevo (RN-20): la vigente de cada tipo lleva el badge "Vigente".
 * HU-051: las que se hicieron a raíz de un evento enlazan a él ("Tras un evento").
 */
export function HistorialEvaluaciones({ riesgoId, version }: HistorialEvaluacionesProps) {
  const peticion = usePeticion(listarEvaluaciones)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(riesgoId), [ejecutar, riesgoId])
  useEffect(cargar, [cargar, version])

  const evaluaciones = peticion.datos ?? []
  if (!peticion.error && evaluaciones.length === 0) return null

  // La primera de cada tipo es la vigente: la lista viene de la más reciente a la más antigua.
  const vigentes = new Set(['inicial', 'residual'].map((t) => evaluaciones.find((e) => e.tipo === t)?.id))

  return (
    <section aria-labelledby="titulo-historial" className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <header className="grid gap-1">
        <h2 id="titulo-historial" className="text-md font-semibold">
          Historial de evaluaciones
        </h2>
        {evaluaciones.length > 0 && (
          <p className="text-text-secondary">
            {evaluaciones.length === 1
              ? `1 evaluación, del ${fecha(evaluaciones[0].evaluadoEn)}.`
              : `${evaluaciones.length} evaluaciones desde el ${fecha(evaluaciones[evaluaciones.length - 1].evaluadoEn)}. Evaluar de nuevo no borra las anteriores.`}
          </p>
        )}
      </header>

      {peticion.error && evaluaciones.length === 0 ? (
        <div className="flex flex-col items-start gap-3">
          <p className="inline-flex items-center gap-2">
            <TriangleAlert aria-hidden size={16} strokeWidth={1.5} className="text-error-icon" />
            No se pudo cargar el historial. {peticion.error.mensaje}
          </p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </div>
      ) : (
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th scope="col">Evaluación</th>
                <th scope="col" className="max-sm:hidden">
                  Tipo
                </th>
                <th scope="col">Nivel</th>
                <th scope="col" className="max-sm:hidden">
                  Probabilidad × severidad
                </th>
              </tr>
            </thead>
            <tbody>
              {evaluaciones.map((e) => (
                <Fila key={e.id} evaluacion={e} vigente={vigentes.has(e.id)} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}

function Tipo({ evaluacion, vigente }: { evaluacion: Evaluacion; vigente: boolean }) {
  return (
    <span className="inline-flex items-center gap-2">
      {TIPOS[evaluacion.tipo]}
      {vigente && <Insignia tipo="exito">Vigente</Insignia>}
    </span>
  )
}

function Fila({ evaluacion: e, vigente }: { evaluacion: Evaluacion; vigente: boolean }) {
  const detalle = `${e.probabilidad} ${PROBABILIDADES[e.probabilidad - 1].nombre} × ${e.severidad} ${SEVERIDADES[e.severidad - 1].nombre}`
  return (
    <tr>
      <td className="envolver">
        <div className="tabular-nums">{fechaHora(e.evaluadoEn)}</div>
        <div className="text-xs text-text-secondary">por {e.evaluadoPorNombre}</div>
        {e.accidenteId && (
          <Link to={`/eventos/${e.accidenteId}`} className="text-xs">
            Tras un evento
          </Link>
        )}
        {/* En celular, el tipo y el detalle van aquí: así el nivel queda a la vista sin desplazar la tabla. */}
        <div className="mt-1 grid justify-items-start gap-1 text-sm sm:hidden">
          <Tipo evaluacion={e} vigente={vigente} />
          <span className="text-xs text-text-secondary">{detalle}</span>
        </div>
      </td>
      <td className="max-sm:hidden">
        <Tipo evaluacion={e} vigente={vigente} />
      </td>
      <td>
        <EtiquetaRiesgo nivel={e} />
      </td>
      <td className="envolver max-sm:hidden">{detalle}</td>
    </tr>
  )
}
