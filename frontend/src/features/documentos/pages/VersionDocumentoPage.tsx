import { TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link, useParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { Volver } from '@/shared/components/ui/Volver'
import { ContenidoDocumento } from '@/shared/documentos'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { obtenerVersion } from '../api/documentos'
import { ESTADOS_VERSION, fecha, NOMBRE_TIPO } from '../etiquetas'

/**
 * HU-073: consulta de solo lectura de una versión concreta del documento (reemplazada, vigente o borrador)
 * accesible desde el historial de versiones para Responsable SST y Admin de empresa.
 */
export function VersionDocumentoPage() {
  const { id = '', versionId = '' } = useParams()
  const peticion = usePeticion(obtenerVersion)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(id, versionId), [ejecutar, id, versionId])
  useEffect(cargar, [cargar])

  if (peticion.error?.status === 404) return <NotFoundPage />
  const d = peticion.datos?.id === id && peticion.datos.version.id === versionId ? peticion.datos : undefined
  const v = d?.version

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <Volver
        a={`/documentos/${id}`}
        texto="Volver al documento"
        titulo={d && v ? `Versión ${v.numeroVersion} · ${d.titulo}` : undefined}
      />

      {peticion.error && !d ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudo cargar la versión</h1>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !d || !v ? (
        <div aria-busy="true" aria-label="Cargando la versión del documento" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <>
          <header className="grid gap-2">
            <h1 className="text-xl font-semibold wrap-break-word">{d.titulo}</h1>
            <div className="flex flex-wrap items-center gap-2">
              <Insignia tipo="neutro">{NOMBRE_TIPO[d.tipoDocumento]}</Insignia>
              <Insignia tipo={ESTADOS_VERSION[v.estado].tipo}>{ESTADOS_VERSION[v.estado].texto}</Insignia>
            </div>
            <p className="text-sm text-text-secondary">
              {`Versión ${v.numeroVersion} · `}
              {v.estado === 'borrador'
                ? `${v.numeroVersion === 1 ? 'Generada' : 'Creada'} el ${fecha(v.creadaEn)} por ${v.creadaPorNombre}`
                : `Aprobada el ${fecha(v.aprobadaEn ?? v.creadaEn)}${v.aprobadaPorNombre ? ` por ${v.aprobadaPorNombre}` : ''}`}
              {d.plantillaTitulo && ` · Plantilla: ${d.plantillaTitulo}`}
            </p>
          </header>

          {v.estado === 'reemplazada' && (
            <Aviso tipo="info" titulo="Versión anterior de solo lectura">
              Esta versión fue reemplazada por una más reciente y no se puede editar.
            </Aviso>
          )}
          {v.estado === 'vigente' && (
            <Aviso tipo="info" titulo="Versión vigente de solo lectura">
              Es la versión publicada actualmente para las personas de la empresa. No se puede editar.
            </Aviso>
          )}
          {v.estado === 'borrador' && (
            <Aviso tipo="info" titulo="Esta versión está en borrador">
              Para editarla o aprobarla, abre la vista de trabajo del documento.{' '}
              <Link to={`/documentos/${d.id}`}>Ir al documento</Link>
            </Aviso>
          )}

          <article
            aria-label={`${d.titulo} — versión ${v.numeroVersion}`}
            className="rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
          >
            <ContenidoDocumento contenido={v.contenido} />
          </article>
        </>
      )}
    </div>
  )
}
