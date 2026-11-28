import { CircleCheck, ClipboardX, TriangleAlert } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useController, useWatch, type Control, type UseFormRegister } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { DialogoConfirmacion } from '@/shared/components/ui/DialogoConfirmacion'
import { Volver } from '@/shared/components/ui/Volver'
import { useConfirmarSalida, useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarPendientes, responderEncuesta } from '../api/encuestas'
import { PreguntaEncuesta } from '../components/PreguntaEncuesta'
import { aSolicitud, claveDe, esquemaRespuesta, RUTA_SIN_RESPUESTAS, valoresIniciales, type RespuestasFormulario } from '../schemas/respuesta'
import type { EncuestaPendiente } from '../types/encuestas'

/** HU-031: responder una encuesta publicada, de forma anónima y una sola vez. */
export function ResponderEncuestaPage() {
  const { id = '' } = useParams()
  const pendientes = usePeticion(listarPendientes)
  const cargandoVisible = useCargaVisible(pendientes.cargando)
  const { ejecutar: cargar } = pendientes
  useEffect(() => {
    void cargar()
  }, [cargar])

  if (pendientes.error) {
    return (
      <div className="grid w-full max-w-layout-narrow gap-6">
        <Volver a="/mis-encuestas" texto="Volver a mis encuestas" />
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudo cargar la encuesta</h1>
          <p className="text-text-secondary">{pendientes.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargar()}>
            Reintentar
          </Boton>
        </section>
      </div>
    )
  }
  if (pendientes.cargando || !pendientes.datos) {
    return (
      <div className="grid w-full max-w-layout-narrow gap-6">
        <Volver a="/mis-encuestas" texto="Volver a mis encuestas" />
        <div aria-busy="true" aria-label="Cargando encuesta" className="grid gap-3">
          {cargandoVisible && [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      </div>
    )
  }
  const encuesta = pendientes.datos.find((e) => e.encuestaId === id)
  if (!encuesta) return <YaNoPendiente />
  return <Formulario key={encuesta.versionId} encuesta={encuesta} />
}

/** La encuesta no está entre las pendientes: ya se respondió, se cerró o no existe para esta persona. */
function YaNoPendiente() {
  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <Volver a="/mis-encuestas" texto="Volver a mis encuestas" />
      <section className="flex flex-col items-center gap-3 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
        <ClipboardX aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
        <h1 className="text-md font-semibold">Esta encuesta ya no está pendiente</h1>
        <p className="text-text-secondary">Puede que ya la hayas respondido o que se haya cerrado.</p>
        <Link to="/mis-encuestas">Ver mis encuestas</Link>
      </section>
    </div>
  )
}

function Formulario({ encuesta }: { encuesta: EncuestaPendiente }) {
  const [enviada, setEnviada] = useState(false)
  const [cambio, setCambio] = useState<'version' | 'ya' | 'cerrada'>()
  const esquema = useMemo(() => esquemaRespuesta(encuesta.preguntas), [encuesta])

  const {
    register,
    control,
    formState: { errors, isSubmitting, isDirty },
    enviar,
    errorGeneral,
  } = useFormulario(esquema, {
    defaultValues: valoresIniciales(encuesta.preguntas),
    erroresPorCodigo: {
      'encuestas.ya_participo': { mensaje: 'Ya respondiste esta encuesta. Solo se permite una respuesta por persona.' },
      'encuestas.version_obsoleta': { mensaje: 'La encuesta cambió mientras la respondías.' },
      'encuestas.cerrada': { mensaje: 'La encuesta se cerró y ya no recibe respuestas.' },
      'encuestas.sin_version_publicada': { mensaje: 'La encuesta ya no está disponible para responder.' },
    },
  })

  // Avisa antes de salir con respuestas sin enviar: dentro de Norvia con un diálogo; al cerrar la pestaña, el navegador.
  const salida = useConfirmarSalida(isDirty && !enviada)

  if (enviada) {
    return (
      <div className="grid w-full max-w-layout-narrow gap-6">
        <Volver a="/mis-encuestas" texto="Volver a mis encuestas" />
        <section className="flex flex-col items-center gap-3 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <CircleCheck aria-hidden size={32} strokeWidth={1.5} className="text-success-icon" />
          <h1 className="text-md font-semibold">¡Gracias! Tu respuesta quedó registrada</h1>
          <p className="text-text-secondary">Es anónima: nadie en la empresa puede ver quién respondió qué.</p>
          <Link to="/mis-encuestas" className={clasesBoton('secundario')}>
            Ver mis encuestas
          </Link>
        </section>
      </div>
    )
  }

  const errorRaiz = (errors as Record<string, { message?: string } | undefined>)[RUTA_SIN_RESPUESTAS]?.message
  const bloqueada = cambio !== undefined

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <Volver a="/mis-encuestas" texto="Volver a mis encuestas" titulo={encuesta.titulo} />

      <form
        noValidate
        className="grid gap-8 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async (datos) => {
          try {
            await responderEncuesta(encuesta.encuestaId, aSolicitud(encuesta, datos))
            setEnviada(true)
          } catch (causa) {
            const codigo = (causa as { response?: { data?: { codigo?: string } } }).response?.data?.codigo
            if (codigo === 'encuestas.version_obsoleta') setCambio('version')
            if (codigo === 'encuestas.ya_participo') setCambio('ya')
            if (codigo === 'encuestas.cerrada' || codigo === 'encuestas.sin_version_publicada') setCambio('cerrada')
            throw causa
          }
        })}
      >
        <header className="grid gap-2">
          <h1 className="text-xl font-semibold">{encuesta.titulo}</h1>
          {encuesta.instrucciones && <p className="whitespace-pre-line text-text-secondary">{encuesta.instrucciones}</p>}
          <p className="text-text-secondary">
            Tus respuestas son anónimas.
            {encuesta.preguntas.some((p) => p.obligatoria) && ' Las preguntas con * son obligatorias.'}
          </p>
        </header>

        {encuesta.preguntas.map((p, i) => (
          <Pregunta key={p.id} numero={i + 1} pregunta={p} control={control} register={register} error={errors.respuestas?.[claveDe(p.id)]?.message as string | undefined} />
        ))}

        {errorRaiz && <Aviso tipo="error">{errorRaiz}</Aviso>}
        {errorGeneral && (
          <Aviso tipo="error">
            {errorGeneral}
            {cambio === 'version' && (
              <>
                {' '}
                <button type="button" className="font-medium underline" onClick={() => window.location.reload()}>
                  Abrir la versión actual
                </button>
              </>
            )}
          </Aviso>
        )}

        <div>
          {bloqueada ? (
            <Link to="/mis-encuestas" className={clasesBoton('secundario')}>
              Volver a mis encuestas
            </Link>
          ) : (
            <Boton type="submit" className="w-full sm:w-auto" cargando={isSubmitting} textoCargando="Enviando…">
              Enviar respuestas
            </Boton>
          )}
        </div>
      </form>
      <DialogoConfirmacion
        abierto={salida.bloqueado}
        titulo="¿Salir sin enviar tus respuestas?"
        textoConfirmar="Salir sin enviar"
        peligro
        onConfirmar={salida.salir}
        onCancelar={salida.quedarse}
      >
        Lo que respondiste en esta encuesta se pierde.
      </DialogoConfirmacion>
    </div>
  )
}

interface PreguntaProps {
  numero: number
  pregunta: EncuestaPendiente['preguntas'][number]
  control: Control<RespuestasFormulario, unknown, RespuestasFormulario>
  register: UseFormRegister<RespuestasFormulario>
  error?: string
}

/** Conecta una pregunta con el formulario según su tipo. */
function Pregunta({ numero, pregunta, control, register, error }: PreguntaProps) {
  const nombre = `respuestas.${claveDe(pregunta.id)}` as const
  const multiple = pregunta.tipoRespuesta === 'seleccion_multiple'
  const { field } = useController({ control, name: nombre })
  const texto = useWatch({ control, name: nombre })

  return (
    <PreguntaEncuesta
      numero={numero}
      pregunta={{
        id: pregunta.id,
        texto: pregunta.texto,
        tipoRespuestaCodigo: pregunta.tipoRespuesta,
        instruccionUi: pregunta.instruccionUi,
        obligatoria: pregunta.obligatoria,
        orden: numero,
        opciones: pregunta.opciones.map((o, j) => ({ id: o.id, texto: o.texto, orden: j + 1 })),
      }}
      respuesta={{
        registro: multiple ? undefined : register(nombre),
        marcadas: multiple ? (field.value as string[]) : undefined,
        onMarcadas: multiple ? field.onChange : undefined,
        texto: typeof texto === 'string' ? texto : undefined,
        maxCaracteres: pregunta.maxCaracteres,
        error,
      }}
    />
  )
}
