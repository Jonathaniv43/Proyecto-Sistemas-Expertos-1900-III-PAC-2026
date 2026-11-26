import { ListaMedidores } from '@/shared/components/datos/ListaMedidores'
import type { ReporteEncuesta } from '../types/encuestas'

type Distribucion = NonNullable<ReporteEncuesta['preguntas'][number]['distribucion']>

/**
 * Distribución de respuestas de una pregunta de selección como "lista de medidores", en el orden de la encuesta.
 * El componente de medidores es compartido: también lo usan las estadísticas de eventos (HU-052).
 */
export function DistribucionPregunta({ distribucion }: { distribucion: Distribucion }) {
  return (
    <ListaMedidores
      unidad={['respuesta', 'respuestas']}
      filas={distribucion.map((o) => ({ clave: o.opcionId, etiqueta: o.etiqueta, cantidad: o.cantidad, porcentaje: o.porcentaje }))}
    />
  )
}
