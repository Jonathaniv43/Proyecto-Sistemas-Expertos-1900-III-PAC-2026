import { TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarVersiones } from '../api/documentos'
import { ESTADOS_VERSION, fecha } from '../etiquetas'
import type { VersionResumen } from '../types/documentos'

interface HistorialVersionesProps {
  documentoId: string
  /** Id de la versión que ya se muestra arriba en la pantalla del documento. */
  versionActualId: string
  /** Cambia al aprobar o crear una versión nueva para volver a pedir el historial. */
  claveRecarga: string
}

/**
 * HU-073: historial de todas las versiones del documento (de la más reciente a la primera) para consultar
 * versiones anteriores sin perder el historial (RN-50, D13). Solo aparece cuando existe más de una versión.
 */
export function HistorialVersiones({ documentoId, versionActualId, claveRecarga }: HistorialVersionesProps) {
  const peticion = usePeticion(listarVersiones)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(documentoId), [ejecutar, documentoId])
  useEffect(cargar, [cargar, claveRecarga])

  const versiones = peticion.datos ?? []
  if (!peticion.error && versiones.length <= 1) return null

  return (
    <section aria-labelledby="titulo-historial-versiones" className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <header className="grid gap-1">
        <h2 id="titulo-historial-versiones" className="text-md font-semibold">
          Historial de versiones
        </h2>
        {versiones.length > 1 && (
          <p className="text-text-secondary">
            {`${versiones.length} versiones registradas. Las versiones anteriores siguen disponibles para consulta.`}
          </p>
        )}
      </header>

      {peticion.error && versiones.length === 0 ? (
        <div className="flex flex-col items-start gap-3">
          <p className="inline-flex items-center gap-2">
            <TriangleAlert aria-hidden size={16} strokeWidth={1.5} className="text-error-icon" />
            No se pudo cargar el historial de versiones. {peticion.error.mensaje}
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
                <th scope="col">Versión</th>
                <th scope="col">Estado</th>
                <th scope="col" className="max-sm:hidden">
                  Registro
                </th>
                <th scope="col" className="text-right">
                  Consulta
                </th>
              </tr>
            </thead>
            <tbody>
              {versiones.map((v) => (
                <FilaVersion key={v.id} documentoId={documentoId} version={v} esActual={v.id === versionActualId} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}

function descripcionRegistro(v: VersionResumen): string {
  if (v.estado === 'borrador') {
    return `${v.numeroVersion === 1 ? 'Generada' : 'Creada'} el ${fecha(v.creadaEn)} por ${v.creadaPorNombre}`
  }
  return `Aprobada el ${fecha(v.aprobadaEn ?? v.creadaEn)}${v.aprobadaPorNombre ? ` por ${v.aprobadaPorNombre}` : ''}`
}

function FilaVersion({
  documentoId,
  version: v,
  esActual,
}: {
  documentoId: string
  version: VersionResumen
  esActual: boolean
}) {
  const estado = ESTADOS_VERSION[v.estado]
  const ruta = `/documentos/${documentoId}/versiones/${v.id}`
  const registro = descripcionRegistro(v)

  return (
    <tr>
      <td className="envolver">
        <div className="font-medium tabular-nums">
          {esActual ? (
            `Versión ${v.numeroVersion}`
          ) : (
            <Link to={ruta}>{`Versión ${v.numeroVersion}`}</Link>
          )}
        </div>
        <div className="text-xs text-text-secondary sm:hidden">{registro}</div>
      </td>
      <td>
        <Insignia tipo={estado.tipo}>{estado.texto}</Insignia>
      </td>
      <td className="envolver text-sm text-text-secondary max-sm:hidden">{registro}</td>
      <td className="text-right">
        {esActual ? (
          <span className="text-xs text-text-secondary">En pantalla</span>
        ) : (
          <Link to={ruta} className="text-sm font-medium">
            Consultar
          </Link>
        )}
      </td>
    </tr>
  )
}
