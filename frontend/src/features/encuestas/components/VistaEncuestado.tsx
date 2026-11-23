import type { SeccionEncuesta, TipoRespuestaCatalogo } from '../types/encuestas'
import { PreguntaEncuesta } from './PreguntaEncuesta'

export interface PreguntaVista {
  clave: string
  seccionId: string | null
  texto: string
  tipoRespuestaCodigo: string
  obligatoria: boolean
  opciones: { texto: string }[]
}

interface VistaEncuestadoProps {
  titulo: string
  instrucciones?: string | null
  secciones: SeccionEncuesta[]
  preguntas: PreguntaVista[]
  tipos: TipoRespuestaCatalogo[]
}

/**
 * La encuesta tal como la verá el encuestado (HU-030): título, instrucciones y las preguntas por
 * sección, en una sola card. Los controles están deshabilitados; responder es HU-031.
 */
export function VistaEncuestado({ titulo, instrucciones, secciones, preguntas, tipos }: VistaEncuestadoProps) {
  const instruccionDe = (codigo: string) => tipos.find((t) => t.codigo === codigo)?.instruccionUi ?? ''
  const tituloSeccion = new Map(secciones.map((s) => [s.id, s.titulo]))

  return (
    <article className="grid gap-8 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6" aria-label="Vista del encuestado">
      <header className="grid gap-2">
        <h2 className="text-lg font-semibold">{titulo || 'Sin título'}</h2>
        {instrucciones && <p className="whitespace-pre-line text-text-secondary">{instrucciones}</p>}
        {preguntas.some((p) => p.obligatoria) && <p className="text-text-secondary">Las preguntas con * son obligatorias.</p>}
      </header>
      {preguntas.map((p, i) => {
        const nuevaSeccion = p.seccionId && (i === 0 || preguntas[i - 1].seccionId !== p.seccionId)
        return (
          <div key={p.clave} className="grid gap-2">
            {nuevaSeccion && <p className="text-xs font-medium text-text-secondary">{tituloSeccion.get(p.seccionId!)}</p>}
            <PreguntaEncuesta
              numero={i + 1}
              pregunta={{
                id: p.clave,
                texto: p.texto || 'Pregunta sin texto',
                tipoRespuestaCodigo: p.tipoRespuestaCodigo,
                instruccionUi: instruccionDe(p.tipoRespuestaCodigo),
                obligatoria: p.obligatoria,
                orden: i + 1,
                opciones: p.opciones.map((o, j) => ({ texto: o.texto || `Opción ${j + 1}`, orden: j + 1 })),
              }}
            />
          </div>
        )
      })}
    </article>
  )
}
