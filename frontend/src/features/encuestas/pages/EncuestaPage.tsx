import { ArrowLeft, ChartColumn, Eye, Pencil, Plus, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useFieldArray, useWatch } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { extraerError } from '@/shared/api'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { DialogoConfirmacion } from '@/shared/components/ui/DialogoConfirmacion'
import { Insignia } from '@/shared/components/ui/Insignia'
import { MensajeCampo } from '@/shared/components/ui/MensajeCampo'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { useSesion } from '@/shared/sesion'
import { cerrarEncuesta, guardarBorrador, listarTiposRespuesta, obtenerEncuesta, publicarVersion } from '../api/encuestas'
import { aSolicitud, desdeVersion } from '../borrador/convertir'
import { EditorPregunta } from '../components/EditorPregunta'
import { EstadoEncuesta } from '../components/EstadoEncuesta'
import { VistaEncuestado } from '../components/VistaEncuestado'
import { borradorEsquema, type BorradorFormulario } from '../schemas/borrador'
import type { EncuestaDetalle, TipoRespuestaCatalogo, VersionEncuesta } from '../types/encuestas'

const fechaHora = (iso: string) =>
  new Date(iso).toLocaleString('es-HN', { day: 'numeric', month: 'long', year: 'numeric', hour: 'numeric', minute: '2-digit' })

/** Mensajes de negocio del backend en el tono de la interfaz. */
const ERRORES = {
  'encuestas.solo_creador_puede_editar': { mensaje: 'Solo quien creó la encuesta puede modificarla, publicarla o cerrarla.' },
  'encuestas.cerrada': { mensaje: 'La encuesta está cerrada: ya no se puede modificar ni publicar.' },
  'encuestas.sin_preguntas': { mensaje: 'Agrega al menos una pregunta antes de publicar.' },
  'encuestas.version_no_publicable': { mensaje: 'Esta versión ya no es un borrador. Vuelve a cargar la página.' },
  'encuestas.peligro_invalido': { mensaje: 'Un peligro asociado a una pregunta ya no está disponible. Cambia esa pregunta a respuesta breve o quítala.' },
} as const

/**
 * HU-030: crear y editar la encuesta por versiones. Muestra el borrador (o la versión publicada si no
 * hay borrador) para editar título, instrucciones y preguntas; ofrece la vista del encuestado, publicar
 * y cerrar. Solo quien la creó puede modificarla; una encuesta cerrada es de solo lectura.
 */
export function EncuestaPage() {
  const { id = '' } = useParams()
  const encuesta = usePeticion(obtenerEncuesta)
  const tipos = usePeticion(listarTiposRespuesta)
  const cargandoVisible = useCargaVisible(encuesta.cargando && !encuesta.datos)

  const { ejecutar: cargarEncuesta } = encuesta
  const { ejecutar: cargarTipos } = tipos
  const recargar = useCallback(() => cargarEncuesta(id), [cargarEncuesta, id])
  useEffect(() => {
    void recargar()
    void cargarTipos()
  }, [recargar, cargarTipos])

  if (encuesta.error?.status === 404) return <NotFoundPage />
  if (encuesta.error || tipos.error) {
    return (
      <section className="mx-auto flex max-w-layout-narrow flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
        <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
        <h1 className="text-md font-semibold">No se pudo cargar la encuesta</h1>
        <p className="text-text-secondary">{(encuesta.error ?? tipos.error)!.mensaje}</p>
        <Boton
          variante="secundario"
          onClick={() => {
            void recargar()
            void cargarTipos()
          }}
        >
          Reintentar
        </Boton>
      </section>
    )
  }
  if (!encuesta.datos || !tipos.datos) {
    return (
      <div aria-busy="true" aria-label="Cargando encuesta" className="mx-auto grid w-full max-w-layout-narrow gap-3">
        {cargandoVisible &&
          [0, 1, 2, 3].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
      </div>
    )
  }
  return <Encuesta encuesta={encuesta.datos} tipos={tipos.datos} onCambio={recargar} />
}

function Encuesta({ encuesta, tipos, onCambio }: { encuesta: EncuestaDetalle; tipos: TipoRespuestaCatalogo[]; onCambio: () => Promise<unknown> }) {
  const sesion = useSesion()
  const esCreador = sesion.estado === 'activa' && sesion.usuario.vinculoId === encuesta.creadaPorId
  const cerrada = encuesta.estado === 'cerrada'
  const editable = esCreador && !cerrada

  // Se edita el borrador; si no hay, la versión publicada (guardar crea la versión siguiente).
  const version = (encuesta.versionBorrador ?? encuesta.versionPublicada) as VersionEncuesta
  const secciones = useMemo(() => [...version.secciones].sort((a, b) => a.orden - b.orden), [version])
  const sinBorrador = !encuesta.versionBorrador

  const [modo, setModo] = useState<'editar' | 'vista'>(editable ? 'editar' : 'vista')
  const [aviso, setAviso] = useState<string>()
  const [dialogo, setDialogo] = useState<'publicar' | 'cerrar'>()
  const [ocupado, setOcupado] = useState(false)

  const form = useFormulario(borradorEsquema, {
    defaultValues: desdeVersion(encuesta.titulo, version),
    erroresPorCodigo: ERRORES,
  })
  const {
    register,
    control,
    reset,
    setValue,
    handleSubmit,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
    setErrorGeneral,
    aplicarErrorApi,
  } = form
  const preguntas = useFieldArray({ control, name: 'preguntas', keyName: 'clave' })
  const valores = useWatch({ control }) as BorradorFormulario

  // Al recargar la encuesta (guardar, publicar, cerrar) el formulario toma la versión vigente.
  useEffect(() => reset(desdeVersion(encuesta.titulo, version)), [encuesta, version, reset])

  // Avisa antes de salir de la página con cambios sin guardar.
  useEffect(() => {
    if (!isDirty) return
    const avisar = (e: BeforeUnloadEvent) => e.preventDefault()
    window.addEventListener('beforeunload', avisar)
    return () => window.removeEventListener('beforeunload', avisar)
  }, [isDirty])

  const guardar = async (datos: Parameters<Parameters<typeof handleSubmit>[0]>[0]) =>
    guardarBorrador(encuesta.id, aSolicitud(datos, secciones, { seccionesNuevas: sinBorrador }))

  const onGuardar = enviar(async (datos) => {
    setAviso(undefined)
    const guardada = await guardar(datos)
    await onCambio()
    setAviso(
      sinBorrador
        ? `Se creó la versión ${guardada.numeroVersion} como borrador. Los encuestados siguen viendo la versión ${encuesta.versionPublicada?.numeroVersion}.`
        : 'Cambios guardados en el borrador.',
    )
  })

  const publicar = handleSubmit(
    async (datos) => {
      setOcupado(true)
      setErrorGeneral(undefined)
      setAviso(undefined)
      try {
        // Publicar guarda primero lo que esté sin guardar.
        const aPublicar = isDirty || sinBorrador ? await guardar(datos) : encuesta.versionBorrador!
        await publicarVersion(encuesta.id, aPublicar.id)
        setDialogo(undefined)
        await onCambio()
        setAviso(`Versión ${aPublicar.numeroVersion} publicada: los encuestados ya pueden responderla.`)
      } catch (causa) {
        setDialogo(undefined)
        aplicarErrorApi(extraerError(causa))
      } finally {
        setOcupado(false)
      }
    },
    () => setDialogo(undefined),
  )

  const cerrar = async () => {
    setOcupado(true)
    setErrorGeneral(undefined)
    try {
      await cerrarEncuesta(encuesta.id)
      setDialogo(undefined)
      await onCambio()
      setAviso('La encuesta quedó cerrada: ya no recibe respuestas.')
      setModo('vista')
    } catch (causa) {
      setDialogo(undefined)
      aplicarErrorApi(extraerError(causa))
    } finally {
      setOcupado(false)
    }
  }

  // Agregar al final de una sección y mover solo dentro de su sección. La sección de una pregunta no
  // cambia, así que se lee de `fields` (siempre al día) y no de los valores observados (van un render atrás).
  const seccionDe = (i: number) => preguntas.fields[i]?.seccionId ?? null
  const agregarEn = (seccionId: string | null) => {
    const ultima = preguntas.fields.findLastIndex((p) => p.seccionId === seccionId)
    preguntas.insert(ultima === -1 ? preguntas.fields.length : ultima + 1, {
      id: null,
      seccionId,
      categoriaRiesgoId: null,
      texto: '',
      tipoRespuestaCodigo: 'respuesta_breve',
      obligatoria: true,
      peligroId: null,
      opciones: [],
    })
  }
  const mismaSeccion = (a: number, b: number) => b >= 0 && b < preguntas.fields.length && seccionDe(a) === seccionDe(b)

  const hayPublicada = Boolean(encuesta.versionPublicada)
  const puedePublicar = editable && (!sinBorrador || isDirty)
  const errorPreguntas = errors.preguntas?.root?.message ?? errors.preguntas?.message

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <div>
        <Link to="/encuestas" className="inline-flex items-center gap-1 text-sm">
          <ArrowLeft aria-hidden size={16} strokeWidth={1.5} />
          Encuestas
        </Link>
      </div>

      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">{encuesta.titulo}</h1>
        <div className="flex flex-wrap items-center gap-2">
          <EstadoEncuesta estado={encuesta.estado} />
          {encuesta.versionPublicada && <Insignia tipo="neutro">Vigente: versión {encuesta.versionPublicada.numeroVersion}</Insignia>}
          {encuesta.versionBorrador && hayPublicada && (
            <Insignia tipo="neutro">Borrador: versión {encuesta.versionBorrador.numeroVersion}</Insignia>
          )}
        </div>
      </header>

      <EstadoExplicado encuesta={encuesta} esCreador={esCreador} />
      {aviso && <Aviso tipo="exito">{aviso}</Aviso>}
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}

      <div className="flex flex-wrap gap-3">
        {hayPublicada && (
          <Link to={`/encuestas/${encuesta.id}/resultados`} className={clasesBoton('secundario')}>
            <ChartColumn aria-hidden size={16} strokeWidth={1.5} />
            Ver resultados
          </Link>
        )}
        {modo === 'editar' ? (
          <Boton variante="secundario" onClick={() => setModo('vista')}>
            <Eye aria-hidden size={16} strokeWidth={1.5} />
            Ver como encuestado
          </Boton>
        ) : (
          editable && (
            <Boton variante="secundario" onClick={() => setModo('editar')}>
              <Pencil aria-hidden size={16} strokeWidth={1.5} />
              Volver a editar
            </Boton>
          )
        )}
      </div>

      {modo === 'vista' ? (
        <VistaEncuestado
          titulo={valores.titulo}
          instrucciones={valores.instrucciones}
          secciones={secciones}
          preguntas={preguntas.fields.map((f, i) => ({ ...f, ...valores.preguntas[i], clave: f.clave }))}
          tipos={tipos}
        />
      ) : (
        <form noValidate className="grid gap-6" onSubmit={onGuardar}>
          <div className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
            <CampoTexto etiqueta="Título" required autoComplete="off" registro={register('titulo')} error={errors.titulo} />
            <AreaTexto
              etiqueta="Instrucciones"
              {...register('instrucciones')}
              error={errors.instrucciones?.message}
              ayuda="Opcional. Se muestran al inicio de la encuesta: para qué es y que las respuestas son anónimas."
            />
          </div>

          <section className="grid gap-4" aria-labelledby="titulo-preguntas">
            <h2 id="titulo-preguntas" className="text-md font-semibold">
              Preguntas ({preguntas.fields.length})
            </h2>
            <MensajeCampo id="preguntas-mensaje" error={errorPreguntas} />
            {[...secciones.map((s) => ({ id: s.id as string | null, titulo: s.titulo })), { id: null, titulo: 'Otras preguntas' }].map((seccion) => {
              const indices = preguntas.fields.map((_, i) => i).filter((i) => seccionDe(i) === seccion.id)
              if (seccion.id === null && indices.length === 0 && secciones.length > 0) {
                return (
                  <div key="otras">
                    <Boton variante="secundario" onClick={() => agregarEn(null)}>
                      <Plus aria-hidden size={16} strokeWidth={1.5} />
                      Agregar una pregunta sin categoría
                    </Boton>
                  </div>
                )
              }
              return (
                <div key={seccion.id ?? 'otras'} className="grid gap-3 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
                  <h3 className="text-sm font-semibold text-text-secondary">{seccion.titulo}</h3>
                  {indices.map((i) => (
                    <EditorPregunta
                      key={preguntas.fields[i].clave}
                      indice={i}
                      numero={i + 1}
                      control={control}
                      register={register}
                      setValue={setValue}
                      errores={errors.preguntas?.[i]}
                      tipos={tipos}
                      puedeSubir={mismaSeccion(i, i - 1)}
                      puedeBajar={mismaSeccion(i, i + 1)}
                      onSubir={() => preguntas.swap(i, i - 1)}
                      onBajar={() => preguntas.swap(i, i + 1)}
                      onEliminar={() => preguntas.remove(i)}
                    />
                  ))}
                  <div>
                    <Boton variante="secundario" onClick={() => agregarEn(seccion.id)}>
                      <Plus aria-hidden size={16} strokeWidth={1.5} />
                      Agregar pregunta
                    </Boton>
                  </div>
                </div>
              )
            })}
          </section>

          <div className="flex flex-col gap-3 rounded-lg border border-border-default bg-bg-surface p-4 sm:flex-row sm:items-center sm:justify-between sm:p-6">
            <Boton variante="secundario" onClick={() => setDialogo('cerrar')}>
              Cerrar encuesta…
            </Boton>
            <div className="flex flex-col gap-3 sm:flex-row">
              <Boton type="submit" variante="secundario" disabled={!isDirty} cargando={isSubmitting} textoCargando="Guardando…">
                Guardar borrador
              </Boton>
              <Boton disabled={!puedePublicar} onClick={() => setDialogo('publicar')}>
                Publicar
              </Boton>
            </div>
          </div>
        </form>
      )}

      {modo === 'vista' && editable && (
        <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
          <Boton disabled={!puedePublicar} onClick={() => setDialogo('publicar')}>
            Publicar
          </Boton>
        </div>
      )}

      <DialogoConfirmacion
        abierto={dialogo === 'publicar'}
        titulo={hayPublicada ? '¿Publicar la versión nueva?' : '¿Publicar la encuesta?'}
        textoConfirmar="Publicar"
        textoCargando="Publicando…"
        cargando={ocupado}
        onConfirmar={() => void publicar()}
        onCancelar={() => setDialogo(undefined)}
      >
        {hayPublicada
          ? `Los encuestados verán esta versión en lugar de la versión ${encuesta.versionPublicada!.numeroVersion}. Las respuestas ya recibidas se conservan.`
          : 'Los encuestados de la empresa podrán verla y responderla.'}
        {isDirty && ' Antes se guardarán los cambios pendientes.'}
      </DialogoConfirmacion>

      <DialogoConfirmacion
        abierto={dialogo === 'cerrar'}
        peligro
        titulo={`¿Cerrar la encuesta "${encuesta.titulo}"?`}
        textoConfirmar="Cerrar encuesta"
        textoCargando="Cerrando…"
        cargando={ocupado}
        onConfirmar={() => void cerrar()}
        onCancelar={() => setDialogo(undefined)}
      >
        Ya no recibirá respuestas ni se podrá editar. Esta acción no se deshace.
      </DialogoConfirmacion>
    </div>
  )
}

/** Explica en qué estado está la encuesta y qué pasa al editarla. */
function EstadoExplicado({ encuesta, esCreador }: { encuesta: EncuestaDetalle; esCreador: boolean }) {
  if (encuesta.estado === 'cerrada') {
    return (
      <Aviso tipo="info" titulo="Encuesta cerrada">
        {encuesta.cerradaEn ? `Se cerró el ${fechaHora(encuesta.cerradaEn)}. ` : ''}Ya no recibe respuestas ni se puede editar.
      </Aviso>
    )
  }
  if (!esCreador) {
    return <Aviso tipo="info">Solo quien creó la encuesta puede modificarla, publicarla o cerrarla. Aquí puedes ver cómo la verán los encuestados.</Aviso>
  }
  const publicada = encuesta.versionPublicada
  if (!publicada) {
    return <Aviso tipo="info">Es un borrador: los encuestados todavía no la ven. Cuando esté lista, publícala.</Aviso>
  }
  if (!encuesta.versionBorrador) {
    return (
      <Aviso tipo="info">
        Los encuestados están respondiendo la versión {publicada.numeroVersion}. Si guardas cambios, se crea la versión{' '}
        {publicada.numeroVersion + 1} como borrador y la versión {publicada.numeroVersion} sigue vigente hasta que publiques la nueva.
      </Aviso>
    )
  }
  return (
    <Aviso tipo="advertencia">
      Hay cambios sin publicar en la versión {encuesta.versionBorrador.numeroVersion}. Los encuestados siguen viendo la versión{' '}
      {publicada.numeroVersion}.
    </Aviso>
  )
}
