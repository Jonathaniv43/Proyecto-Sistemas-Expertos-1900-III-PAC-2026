import { Insignia } from '@/shared/components/ui/Insignia'
import type { EstadoEncuesta as Estado } from '../types/encuestas'

/** Badge de estado de una encuesta (design-system.md § Badges: borrador y cerrada neutros, publicada info). */
export function EstadoEncuesta({ estado }: { estado: Estado }) {
  if (estado === 'publicada') return <Insignia tipo="info">Publicada</Insignia>
  return <Insignia tipo="neutro">{estado === 'cerrada' ? 'Cerrada' : 'Borrador'}</Insignia>
}
