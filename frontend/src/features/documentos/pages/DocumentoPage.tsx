import { CircleCheck, PencilLine, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { ContenidoDocumento } from '@/shared/documentos'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { DialogoConfirmacion } from '@/shared/components/ui/DialogoConfirmacion'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { extraerError } from '@/shared/api/errores'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { ROLES, useTieneRol } from '@/shared/sesion'
import { aprobarDocumento, obtenerDocumento } from '../api/documentos'
import { ESTADOS_VERSION, fecha, NOMBRE_TIPO } from '../etiquetas'

/** Los 409 de aprobar ocurren si el documento cambió desde que se abrió (otra pestaña u otra persona). */
const ERRORES_APROBAR: Record<string, string> = {
  'documentos.sin_borrador': 'Ya no tiene un borrador: alguien lo aprobó antes. Abajo se ve cómo quedó.',
  'documentos.por_completar': 'Todavía tiene datos por completar. Complétalos en el borrador y vuelve a aprobarlo.',
}

/** Lo que dejan en el estado de la navegación generar (HU-070) y guardar el borrador (HU-071). */
export interface EstadoDocumentoNavegacion {
  generado?: true
  guardado?: true
}

/**
 * HU-070: un documento con su versión de trabajo. Si es un borrador, avisa que la empresa debe revisarlo y qué datos
 * quedaron por completar (resaltados en el texto, RN-49). El Responsable SST edita el borrador (HU-071) y lo aprueba
 * (HU-072) cuando ya no le faltan datos: queda vigente y, si es una política, Norvia avisa a las personas de la empresa.
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

  const [confirmando, setConfirmando] = useState(false)
  const [aprobando, setAprobando] = useState(false)
  const [resultado, setResultado] = useState<{ id: string; exito?: string; error?: string }>()

  const aprobar = async (titulo: string, esPolitica: boolean) => {
    setAprobando(true)
    try {
      await aprobarDocumento(id)
      setResultado({
        id,
        exito: esPolitica
          ? `«${titulo}» quedó vigente. Norvia avisará en Norvia y por correo a las personas con acceso vigente a la empresa.`
          : `«${titulo}» quedó vigente.`,
      })
    } catch (causa) {
      const error = extraerError(causa)
      setResultado({ id, error: ERRORES_APROBAR[error.codigo ?? ''] ?? error.mensaje })
    } finally {
      setConfirmando(false)
      setAprobando(false)
      // Aprobado o no (alguien más pudo cambiarlo), se muestra el documento como quedó.
      cargar()
    }
  }

  if (peticion.error?.status === 404) return <NotFoundPage />
  const d = peticion.datos?.id === id ? peticion.datos : undefined
  const v = d?.version
  const aviso = resultado?.id === id ? resultado : undefined
  const esBorrador = v?.estado === 'borrador'
  const puedeAprobar = puedeEditar && esBorrador && d?.porCompletar.length === 0

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
              {puedeEditar && esBorrador && (
                <div className="flex flex-wrap gap-3">
                  <Link to={`/documentos/${d.id}/editar`} className={clasesBoton(puedeAprobar ? 'secundario' : 'primario')}>
                    <PencilLine aria-hidden size={16} strokeWidth={1.5} />
                    Editar borrador
                  </Link>
                  {puedeAprobar && (
                    <Boton onClick={() => setConfirmando(true)}>
                      <CircleCheck aria-hidden size={16} strokeWidth={1.5} />
                      Aprobar
                    </Boton>
                  )}
                </div>
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
            {!aviso && estadoNavegacion?.generado && <Aviso tipo="exito">Se generó el borrador con los datos de la empresa.</Aviso>}
            {!aviso && estadoNavegacion?.guardado && <Aviso tipo="exito">Se guardaron los cambios del borrador.</Aviso>}
            {aviso?.exito && <Aviso tipo="exito">{aviso.exito}</Aviso>}
          </div>
          {aviso?.error && (
            <div role="alert" className="contents">
              <Aviso tipo="error" titulo="No se pudo aprobar el documento">
                {aviso.error}
              </Aviso>
            </div>
          )}
          {esBorrador && (
            <Aviso tipo="info" titulo="Revisa el contenido antes de aprobarlo">
              Es un borrador hecho desde una plantilla. La empresa debe revisarlo y adaptarlo a su realidad: lo que dice el
              documento es responsabilidad de la empresa.
            </Aviso>
          )}
          {esBorrador && d.porCompletar.length > 0 && (
            <Aviso tipo="advertencia" titulo="Faltan datos por completar">
              <p>
                Están resaltados en el documento y se completan al editar el borrador.
                {puedeEditar && ' Cuando no falte ninguno, podrás aprobarlo.'}
              </p>
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

          <DialogoConfirmacion
            abierto={confirmando}
            titulo={`¿Aprobar «${d.titulo}»?`}
            textoConfirmar="Aprobar"
            textoCargando="Aprobando…"
            cargando={aprobando}
            onConfirmar={() => void aprobar(d.titulo, d.tipoDocumento === 'politica')}
            onCancelar={() => setConfirmando(false)}
          >
            {v.numeroVersion > 1 ? 'Reemplazará a la versión vigente y ya' : 'Quedará vigente y ya'} no se podrá editar.
            {d.tipoDocumento === 'politica' &&
              ' Norvia avisará en Norvia y por correo a las personas con acceso vigente a la empresa.'}
          </DialogoConfirmacion>
        </>
      )}
    </div>
  )
}
