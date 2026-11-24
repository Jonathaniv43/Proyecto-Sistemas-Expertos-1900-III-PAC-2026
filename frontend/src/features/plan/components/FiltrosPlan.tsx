import { useController, type UseFormReturn } from 'react-hook-form'
import { CampoFecha } from '@/shared/components/ui/CampoFecha'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { PRIORIDADES } from '../etiquetas'
import { errorFecha, fechaFiltro, type ValoresFiltros } from '../filtros'
import type { Responsable } from '../types/plan'

const ESTADOS = [
  { valor: 'vencidas', texto: 'Vencidas' },
  { valor: 'pendiente', texto: 'Pendiente' },
  { valor: 'en_progreso', texto: 'En progreso' },
  { valor: 'completada', texto: 'Completada' },
  { valor: 'cancelada', texto: 'Cancelada' },
]
const ORIGENES = [
  { valor: 'riesgo', texto: 'De un riesgo' },
  { valor: 'accidente', texto: 'De un accidente' },
  { valor: 'manual', texto: 'Manual' },
]

/**
 * HU-046: filtros del plan por estado (o vencidas), responsable (o sin responsable), origen, prioridad y rango de
 * fecha límite. Filtran al momento, como los de la matriz. El formulario es de la página, que también lo cambia
 * (enlace "N vencidas", "Quitar filtros").
 */
export function FiltrosPlan({ form, responsables }: { form: UseFormReturn<ValoresFiltros>; responsables: Responsable[] }) {
  const { register, control } = form
  const { field: desde } = useController({ control, name: 'desde' })
  const { field: hasta } = useController({ control, name: 'hasta' })
  const d = fechaFiltro(desde.value)
  const h = fechaFiltro(hasta.value)

  return (
    <form role="search" aria-label="Filtrar tareas" className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3" onSubmit={(e) => e.preventDefault()}>
      <CampoSelect etiqueta="Estado" todos="Todos los estados" registro={register('estado')} opciones={ESTADOS} />
      <CampoSelect
        etiqueta="Responsable"
        todos="Todas las personas"
        registro={register('responsable')}
        opciones={[
          { valor: 'sin_responsable', texto: 'Sin responsable' },
          ...responsables.map((r) => ({ valor: r.id, texto: r.nombreCompleto })),
        ]}
      />
      <CampoSelect etiqueta="Origen" todos="Todos los orígenes" registro={register('origen')} opciones={ORIGENES} />
      <CampoSelect
        etiqueta="Prioridad"
        todos="Todas las prioridades"
        registro={register('prioridad')}
        opciones={([1, 2, 3] as const).map((p) => ({ valor: String(p), texto: PRIORIDADES[p] }))}
      />
      <CampoFecha etiqueta="Vence desde" valor={desde.value} onChange={desde.onChange} onBlur={desde.onBlur} error={errorFecha(desde.value)} />
      <CampoFecha
        etiqueta="Vence hasta"
        valor={hasta.value}
        onChange={hasta.onChange}
        onBlur={hasta.onBlur}
        error={errorFecha(hasta.value) ?? (d && h && d > h ? 'No puede ser anterior a «Vence desde».' : undefined)}
      />
    </form>
  )
}
