import { FilePlus, FileText, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { ROLES, useTieneRol } from '@/shared/sesion'
import { listarDocumentos } from '../api/documentos'
import { ESTADOS_VERSION, fecha, NOMBRE_TIPO } from '../etiquetas'
import type { DocumentoResumen } from '../types/documentos'

/**
 * HU-070: las políticas y los procedimientos de la empresa, el último actualizado primero, con el estado de su versión
 * de trabajo. Solo el Responsable SST genera documentos; el admin los consulta.
 */
export function DocumentosPage() {
  const peticion = usePeticion(listarDocumentos)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const puedeGenerar = useTieneRol([ROLES.responsableSst])
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(), [ejecutar])
  useEffect(cargar, [cargar])

  const documentos = peticion.datos
  const generar = (
    <Link to="/documentos/generar" className={clasesBoton('primario')}>
      <FilePlus aria-hidden size={16} strokeWidth={1.5} />
      Generar documento
    </Link>
  )

  return (
    <div className="grid w-full max-w-layout grid-cols-1 gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h1 className="text-xl font-semibold">Documentos</h1>
          <p className="text-text-secondary">Las políticas y los procedimientos de seguridad y salud de la empresa.</p>
        </div>
        {puedeGenerar && documentos && documentos.length > 0 && generar}
      </header>

      {peticion.error && !documentos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar los documentos</h2>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !documentos ? (
        <div aria-busy="true" aria-label="Cargando los documentos" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : documentos.length === 0 ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <FileText aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <div className="grid gap-2">
            <h2 className="text-md font-semibold">Todavía no hay documentos</h2>
            <p className="max-w-layout-narrow text-text-secondary">
              {puedeGenerar
                ? 'Genera la política de seguridad y salud y los procedimientos desde una plantilla, con los datos de la empresa.'
                : 'El Responsable SST genera la política de seguridad y salud y los procedimientos de la empresa.'}
            </p>
          </div>
          {puedeGenerar && generar}
        </section>
      ) : (
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th scope="col">Documento</th>
                <th scope="col" className="max-sm:hidden">
                  Tipo
                </th>
                <th scope="col" className="max-sm:hidden">
                  Versión
                </th>
                <th scope="col">Estado</th>
              </tr>
            </thead>
            <tbody>
              {documentos.map((d) => (
                <FilaDocumento key={d.id} documento={d} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

function FilaDocumento({ documento: d }: { documento: DocumentoResumen }) {
  const estado = ESTADOS_VERSION[d.estado]
  return (
    <tr>
      <td className="envolver">
        <Link to={`/documentos/${d.id}`} className="font-medium">
          {d.titulo}
        </Link>
        <div className="text-xs text-text-secondary">
          <span className="sm:hidden">{`${NOMBRE_TIPO[d.tipoDocumento]} · versión ${d.numeroVersion} · `}</span>
          {`Actualizado el ${fecha(d.actualizadoEn)}`}
        </div>
      </td>
      <td className="max-sm:hidden">{NOMBRE_TIPO[d.tipoDocumento]}</td>
      <td className="tabular-nums max-sm:hidden">{d.numeroVersion}</td>
      <td>
        <Insignia tipo={estado.tipo}>{estado.texto}</Insignia>
      </td>
    </tr>
  )
}
