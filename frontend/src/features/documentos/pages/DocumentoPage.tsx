import { PencilLine, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { ContenidoDocumento } from '@/shared/documentos'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { ROLES, useTieneRol } from '@/shared/sesion'
import { obtenerDocumento } from '../api/documentos'
import { ESTADOS_VERSION, fecha, NOMBRE_TIPO } from '../etiquetas'

/** Lo que dejan en el estado de la navegación generar (HU-070) y guardar el borrador (HU-071). */
export interface EstadoDocumentoNavegacion {
  generado?: true
  guardado?: true
}

/**
 * HU-070: un documento con su versión de trabajo. Si es un borrador, avisa que la empresa debe revisarlo y qué datos
 * quedaron por completar (resaltados en el texto, RN-49). El Responsable SST edita el borrador (HU-071); aprobarlo es HU-072.
 */
export function DocumentoPage() {
  const { id = '' } = useParams()
  const estadoNavegacion = useLocation().state as EstadoDocumentoNavegacion | null
  const puedeEditar = useTieneRol([ROLES.responsableSst])
  const peticion = usePeticion(obtenerDocumento)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(id), [ejecutar, id])
  useEffect(cargar, [cargar])

  if (peticion.error?.status === 404) return <NotFoundPage />
  const d = peticion.datos?.id === id ? peticion.datos : undefined
  const v = d?.version

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <Volver a="/documentos" texto="Volver a documentos" titulo={d?.titulo} />
      {peticion.error && !d ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudo cargar el documento</h1>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !d || !v ? (
        <div aria-busy="true" aria-label="Cargando el documento" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <>
          <header className="grid gap-2">
            <div className="flex flex-wrap items-start justify-between gap-4">
              <h1 className="text-xl font-semibold wrap-break-word">{d.titulo}</h1>
              {puedeEditar && v.estado === 'borrador' && (
                <Link to={`/documentos/${d.id}/editar`} className={clasesBoton('primario')}>
                  <PencilLine aria-hidden size={16} strokeWidth={1.5} />
                  Editar borrador
                </Link>
              )}
            </div>
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

          <div role="status" className="contents">
            {estadoNavegacion?.generado && <Aviso tipo="exito">Se generó el borrador con los datos de la empresa.</Aviso>}
            {estadoNavegacion?.guardado && <Aviso tipo="exito">Se guardaron los cambios del borrador.</Aviso>}
          </div>
          {v.estado === 'borrador' && (
            <Aviso tipo="info" titulo="Revisa el contenido antes de aprobarlo">
              Es un borrador hecho desde una plantilla. La empresa debe revisarlo y adaptarlo a su realidad: lo que dice el
              documento es responsabilidad de la empresa.
            </Aviso>
          )}
          {v.estado === 'borrador' && d.porCompletar.length > 0 && (
            <Aviso tipo="advertencia" titulo="Faltan datos por completar">
              <p>Están resaltados en el documento y se completan al editar el borrador:</p>
              <ul className="mt-1 list-disc pl-6">
                {d.porCompletar.map((dato) => (
                  <li key={dato}>{dato}</li>
                ))}
              </ul>
            </Aviso>
          )}

          <article
            aria-label={d.titulo}
            className="rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
          >
            <ContenidoDocumento contenido={v.contenido} />
          </article>
        </>
      )}
    </div>
  )
}
