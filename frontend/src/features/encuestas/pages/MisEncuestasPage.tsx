import { ChevronRight, ClipboardCheck, TriangleAlert } from 'lucide-react'
import { useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { useScrollRecordado } from '@/shared/hooks/useScrollRecordado'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarPendientes } from '../api/encuestas'

const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

/** HU-031: encuestas publicadas de la empresa que la persona todavía no respondió. */
export function MisEncuestasPage() {
  const pendientes = usePeticion(listarPendientes)
  const cargandoVisible = useCargaVisible(pendientes.cargando)
  useScrollRecordado(Boolean(pendientes.datos))
  const { ejecutar: cargar } = pendientes
  useEffect(() => {
    void cargar()
  }, [cargar])

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Mis encuestas</h1>
        <p className="text-text-secondary">Tus respuestas son anónimas: nadie en la empresa puede ver quién respondió qué.</p>
      </header>

      {pendientes.error ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar tus encuestas</h2>
          <p className="text-text-secondary">{pendientes.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargar()}>
            Reintentar
          </Boton>
        </section>
      ) : pendientes.cargando || !pendientes.datos ? (
        <div aria-busy="true" aria-label="Cargando encuestas" className="grid gap-3">
          {cargandoVisible && [0, 1].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : pendientes.datos.length === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <ClipboardCheck aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">No tienes encuestas pendientes</h2>
          <p className="text-text-secondary">Cuando tu empresa publique una encuesta, aparecerá aquí.</p>
        </section>
      ) : (
        <ul className="grid gap-3">
          {pendientes.datos.map((e) => (
            <li key={e.encuestaId}>
              <Link
                to={`/mis-encuestas/${e.encuestaId}`}
                className="flex items-center justify-between gap-4 rounded-lg border border-border-default bg-bg-surface p-4 text-text-primary no-underline transition-[box-shadow,border-color] hover:border-border-strong hover:text-text-primary hover:shadow-1 sm:p-6"
              >
                <span className="grid gap-1">
                  <span className="font-semibold">{e.titulo}</span>
                  <span className="text-xs text-text-secondary">
                    {plural(e.preguntas.length, 'pregunta', 'preguntas')}
                    {e.preguntas.some((p) => p.obligatoria) && ` · ${plural(e.preguntas.filter((p) => p.obligatoria).length, 'obligatoria', 'obligatorias')}`}
                  </span>
                </span>
                <span className="flex shrink-0 items-center gap-1 text-sm font-medium text-text-link">
                  Responder
                  <ChevronRight aria-hidden size={16} strokeWidth={1.5} />
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
