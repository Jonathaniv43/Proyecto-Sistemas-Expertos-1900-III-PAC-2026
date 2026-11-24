import { TriangleAlert } from 'lucide-react'
import type { ReactNode } from 'react'
import { Boton } from '@/shared/components/ui/Boton'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import type { ErrorApi } from '@/shared/api/errores'

interface EstadoCargaProps {
  cargando: boolean
  error: ErrorApi | undefined
  hayDatos: boolean
  que: string
  onReintentar: () => void
  children: ReactNode
}

/** Error con reintento, esqueleto mientras carga (si tarda) o el contenido. */
export function EstadoCarga({ cargando, error, hayDatos, que, onReintentar, children }: EstadoCargaProps) {
  const visible = useCargaVisible(cargando && !hayDatos)
  if (error && !hayDatos)
    return (
      <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
        <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
        <h2 className="text-md font-semibold">No se pudo cargar {que}</h2>
        <p className="text-text-secondary">{error.mensaje}</p>
        <Boton variante="secundario" onClick={onReintentar}>
          Reintentar
        </Boton>
      </section>
    )
  if (!hayDatos)
    return (
      <div aria-busy="true" aria-label={`Cargando ${que}`} className="grid gap-2">
        {visible &&
          [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
      </div>
    )
  return <>{children}</>
}
