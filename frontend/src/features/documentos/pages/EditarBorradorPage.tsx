import { TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { useWatch } from 'react-hook-form'
import { Link, useBlocker, useLocation, useNavigate, useParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { DialogoConfirmacion } from '@/shared/components/ui/DialogoConfirmacion'
import { Volver } from '@/shared/components/ui/Volver'
import { ContenidoDocumento, porCompletar } from '@/shared/documentos'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { editarBorrador, obtenerDocumento } from '../api/documentos'
import { borradorEsquema, LARGO_MAXIMO_DOCUMENTO } from '../schemas/borrador'
import type { DocumentoDetalle } from '../types/documentos'
import type { EstadoDocumentoNavegacion } from './DocumentoPage'

/**
 * HU-071: editar el borrador de un documento, en Markdown y con la vista previa al lado (design-system.md § Editor de
 * documento). Solo el borrador se edita (RN-50) y la plantilla no cambia. Solo el Responsable SST.
 */
export function EditarBorradorPage() {
  const { id = '' } = useParams()
  const peticion = usePeticion(obtenerDocumento)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(id), [ejecutar, id])
  useEffect(cargar, [cargar])

  if (peticion.error?.status === 404) return <NotFoundPage />
  const d = peticion.datos?.id === id ? peticion.datos : undefined

  return (
    <div className="mx-auto grid w-full max-w-layout gap-6">
      <Volver a={`/documentos/${id}`} texto="Volver al documento" titulo="Editar borrador" ancho="layout" />
      {peticion.error && !d ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudo cargar el documento</h1>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !d ? (
        <div aria-busy="true" aria-label="Cargando el documento" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : d.version.estado !== 'borrador' ? (
        <>
          <h1 className="text-xl font-semibold wrap-break-word">{d.titulo}</h1>
          <Aviso tipo="info" titulo="Este documento no tiene un borrador">
            Una versión aprobada no se edita. <Link to={`/documentos/${d.id}`}>Volver al documento</Link>
          </Aviso>
        </>
      ) : (
        <Editor documento={d} />
      )}
    </div>
  )
}

function Editor({ documento: d }: { documento: DocumentoDetalle }) {
  const navigate = useNavigate()
  const estadoNavegacion = useLocation().state as EstadoDocumentoNavegacion | null
  const {
    register,
    control,
    formState: { errors, isSubmitting, isDirty },
    enviar,
    errorGeneral,
  } = useFormulario(borradorEsquema, {
    defaultValues: { contenido: d.version.contenido },
    erroresPorCodigo: {
      'documentos.sin_borrador': { mensaje: 'Este documento ya no tiene un borrador: alguien lo aprobó. Vuelve al documento para verlo.' },
    },
  })
  const contenido = useWatch({ control, name: 'contenido' }) ?? ''
  const faltan = porCompletar(contenido)

  // Avisa antes de salir con cambios sin guardar: al recargar o cerrar la pestaña, el navegador; dentro de Norvia, un diálogo.
  useEffect(() => {
    if (!isDirty) return
    const avisar = (e: BeforeUnloadEvent) => e.preventDefault()
    window.addEventListener('beforeunload', avisar)
    return () => window.removeEventListener('beforeunload', avisar)
  }, [isDirty])
  // Al guardar se navega con `guardado`: esa salida no se bloquea.
  const bloqueo = useBlocker(
    ({ currentLocation, nextLocation }) =>
      isDirty &&
      currentLocation.pathname !== nextLocation.pathname &&
      !(nextLocation.state as EstadoDocumentoNavegacion | null)?.guardado,
  )

  return (
    <>
      <header className="grid gap-1">
        <h1 className="text-xl font-semibold">Editar borrador</h1>
        <p className="text-text-secondary wrap-break-word">{`${d.titulo} · versión ${d.version.numeroVersion}`}</p>
      </header>

      {estadoNavegacion?.nuevaVersion && (
        <div role="status" className="contents">
          <Aviso tipo="exito">
            {`Se creó la versión ${d.version.numeroVersion} como borrador a partir de la versión vigente. La versión vigente sigue publicada hasta que apruebes esta.`}
          </Aviso>
        </div>
      )}

      <form
        noValidate
        className="grid gap-6"
        onSubmit={enviar(async (datos) => {
          await editarBorrador(d.id, datos.contenido)
          navigate(`/documentos/${d.id}`, { replace: true, state: { guardado: true } satisfies EstadoDocumentoNavegacion })
        })}
      >
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <div className="grid items-start gap-6 lg:grid-cols-2">
          <div className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
            <AreaTexto
              etiqueta="Contenido"
              required
              spellCheck
              className="editor-documento"
              maximo={LARGO_MAXIMO_DOCUMENTO}
              error={errors.contenido?.message}
              ayuda={
                <>
                  <code># Título</code>, <code>## Sección</code>, <code>- lista</code>, <code>1. paso</code> y <code>**negrita**</code>. Deja una
                  línea vacía entre párrafos. Reemplaza cada <code>[Completar: …]</code> por el dato que falta.
                </>
              }
              {...register('contenido')}
            />
          </div>
          <section aria-labelledby="vista-previa" className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
            <div className="flex flex-wrap items-baseline justify-between gap-2">
              <h2 id="vista-previa" className="text-md font-semibold">
                Vista previa
              </h2>
              <p className="text-xs text-text-secondary" aria-live="polite">
                {faltan.length === 0 ? 'Nada por completar' : faltan.length === 1 ? '1 dato por completar' : `${faltan.length} datos por completar`}
              </p>
            </div>
            <ContenidoDocumento contenido={contenido} />
          </section>
        </div>
        <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
          <Link to={`/documentos/${d.id}`}>Cancelar</Link>
          <Boton type="submit" cargando={isSubmitting} textoCargando="Guardando…">
            Guardar borrador
          </Boton>
        </div>
      </form>

      <DialogoConfirmacion
        abierto={bloqueo.state === 'blocked'}
        titulo="¿Salir sin guardar los cambios?"
        textoConfirmar="Salir sin guardar"
        peligro
        onConfirmar={() => bloqueo.proceed?.()}
        onCancelar={() => bloqueo.reset?.()}
      >
        Lo que cambiaste en el borrador se pierde.
      </DialogoConfirmacion>
    </>
  )
}
