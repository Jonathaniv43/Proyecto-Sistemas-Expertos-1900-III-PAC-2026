import { Insignia } from '@/shared/components/ui/Insignia'
import { ETIQUETAS_ESTADO_VINCULO } from '../etiquetas'
import type { EstadoVinculo as Estado } from '../types/vinculos'

/** Badge del acceso (design-system.md § Badges: activo éxito, suspendido advertencia, de baja neutro). */
export function EstadoVinculo({ estado }: { estado: Estado }) {
  const tipo = estado === 'activo' ? 'exito' : estado === 'suspendido' ? 'advertencia' : 'neutro'
  return <Insignia tipo={tipo}>{ETIQUETAS_ESTADO_VINCULO[estado]}</Insignia>
}
