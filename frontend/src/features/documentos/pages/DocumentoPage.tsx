import { CircleCheck, FilePlus2, PencilLine, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
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
import { aprobarDocumento, crearVersionNueva, obtenerDocumento } from '../api/documentos'
import { HistorialVersiones } from '../components/HistorialVersiones'
import { ESTADOS_VERSION, fecha, NOMBRE_TIPO } from '../etiquetas'

/** Los 409 de aprobar ocurren si el documento cambió desde que se abrió (otra pestaña u otra persona). */
const ERRORES_APROBAR: Record<string, string> = {
  'documentos.sin_borrador': 'Ya no tiene un borrador: alguien lo aprobó antes. Abajo se ve cómo quedó.',
  'documentos.por_completar': 'Todavía tiene datos por completar. Complétalos en el borrador y vuelve a aprobarlo.',
}

/** Los 409 de versión nueva ocurren si alguien ya abrió un borrador antes. */
const ERRORES_VERSION: Record<string, string> = {
  'documentos.ya_tiene_borrador': 'Ya existe un borrador abierto para este documento. Abajo se muestra cómo quedó.',
  'documentos.sin_vigente': 'El documento todavía no tiene una versión vigente desde la cual crear una versión nueva.',
}

/** Lo que dejan en el estado de la navegación generar (HU-070) y guardar el borrador (HU-071). */
export interface EstadoDocumentoNavegacion {
  generado?: true
  guardado?: true
  nuevaVersion?: true
}

/**
 * HU-070 a HU-073: un documento con su versión de trabajo y su historial de versiones.
 * Si es un borrador, avisa qué datos faltan completar (RN-49); el Responsable SST edita (HU-071) y aprueba (HU-072).
 * Si está vigente, el Responsable SST puede crear una versión nueva en borrador partiendo de la vigente (HU-073, RN-50).
 */
export function DocumentoPage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const estadoNavegacion = useLocation().state as EstadoDocumentoNavegacion | null
  const puedeEditar = useTieneRol([ROLES.responsableSst])
  const peticion = usePeticion(obtenerDocumento)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(id), [ejecutar, id])
  useEffect(cargar, [cargar])

  const [confirmandoAprobar, setConfirmandoAprobar] = useState(false)
  const [aprobando, setAprobando] = useState(false)
  const [confirmandoVersion, setConfirmandoVersion] = useState(false)
  const [creandoVersion, setCreandoVersion] = useState(false)
  const [resultado, setResultado] = useState<{ id: string; exito?: string; tituloError?: string; error?: string }>()

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
      setResultado({
        id,
        tituloError: 'No se pudo aprobar el documento',
        error: ERRORES_APROBAR[error.codigo ?? ''] ?? error.mensaje,
      })
    } finally {
      setConfirmandoAprobar(false)
      setAprobando(false)
      // Aprobado o no (alguien más pudo cambiarlo), se muestra el documento como quedó.
      cargar()
    }
  }

  const crearVersion = async () => {
    setCreandoVersion(true)
    try {
      await crearVersionNueva(id)
      setConfirmandoVersion(false)
      navigate(`/documentos/${id}/editar`, { state: { nuevaVersion: true } satisfies EstadoDocumentoNavegacion })
    } catch (causa) {
      const error = extraerError(causa)
      setConfirmandoVersion(false)
      setResultado({
        id,
        tituloError: 'No se pudo crear la versión nueva',
        error: ERRORES_VERSION[error.codigo ?? ''] ?? error.mensaje,
      })
      cargar()
    } finally {
      setCreandoVersion(false)
    }
  }

  if (peticion.error?.status === 404) return <NotFoundPage />
  const d = peticion.datos?.id === id ? peticion.datos : undefined
  const v = d?.version
  const aviso = resultado?.id === id ? resultado : undefined
  const esBorrador = v?.estado === 'borrador'
  const puedeAprobar = puedeEditar && esBorrador && d?.porCompletar.length === 0
  const puedeCrearVersion = puedeEditar && v?.estado === 'vigente'

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
                    <Boton onClick={() => setConfirmandoAprobar(true)}>
                      <CircleCheck aria-hidden size={16} strokeWidth={1.5} />
                      Aprobar
                    </Boton>
                  )}
                </div>
              )}
              {puedeCrearVersion && (
                <Boton onClick={() => setConfirmandoVersion(true)}>
                  <FilePlus2 aria-hidden size={16} strokeWidth={1.5} />
                  Crear nueva versión
                </Boton>
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
              <Aviso tipo="error" titulo={aviso.tituloError ?? 'No se pudo completar la acción'}>
                {aviso.error}
              </Aviso>
            </div>
          )}
          {esBorrador && (
            <Aviso tipo="info" titulo="Revisa el contenido antes de aprobarlo">
              {v.numeroVersion === 1
                ? 'Es un borrador hecho desde una plantilla. La empresa debe revisarlo y adaptarlo a su realidad: lo que dice el documento es responsabilidad de la empresa.'
                : `Es un borrador de la versión ${v.numeroVersion}, creado a partir de la versión vigente. Mientras no lo apruebes, las personas de la empresa siguen viendo la versión vigente anterior.`}
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

          <HistorialVersiones documentoId={d.id} versionActualId={v.id} claveRecarga={`${v.id}-${v.estado}`} />

          <DialogoConfirmacion
            abierto={confirmandoAprobar}
            titulo={`¿Aprobar «${d.titulo}»?`}
            textoConfirmar="Aprobar"
            textoCargando="Aprobando…"
            cargando={aprobando}
            onConfirmar={() => void aprobar(d.titulo, d.tipoDocumento === 'politica')}
            onCancelar={() => setConfirmandoAprobar(false)}
          >
            {v.numeroVersion > 1 ? 'Reemplazará a la versión vigente y ya' : 'Quedará vigente y ya'} no se podrá editar.
            {d.tipoDocumento === 'politica' &&
              ' Norvia avisará en Norvia y por correo a las personas con acceso vigente a la empresa.'}
          </DialogoConfirmacion>

          <DialogoConfirmacion
            abierto={confirmandoVersion}
            titulo={`¿Crear una versión nueva de «${d.titulo}»?`}
            textoConfirmar="Crear versión"
            textoCargando="Creando…"
            cargando={creandoVersion}
            onConfirmar={() => void crearVersion()}
            onCancelar={() => setConfirmandoVersion(false)}
          >
            {`Se creará la versión ${v.numeroVersion + 1} como borrador a partir de la versión vigente. La versión vigente seguirá publicada para las personas de la empresa hasta que apruebes la nueva.`}
          </DialogoConfirmacion>
        </>
      )}
    </div>
  )
}
