import { ClipboardList, Plus, TriangleAlert } from 'lucide-react'
import { useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { clasesBoton } from '@/shared/components/ui/clases'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarEncuestas } from '../api/encuestas'
import { EstadoEncuesta } from '../components/EstadoEncuesta'

const fecha = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })

/** Encuestas de la empresa: entrada para crear una nueva o seguir con un borrador (HU-030). */
export function EncuestasPage() {
  const encuestas = usePeticion(listarEncuestas)
  const cargandoVisible = useCargaVisible(encuestas.cargando)
  const { ejecutar: cargar } = encuestas
  useEffect(() => {
    void cargar()
  }, [cargar])

  return (
    <div className="mx-auto grid w-full max-w-layout gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-xl font-semibold">Encuestas</h1>
        <Link to="/encuestas/nueva" className={clasesBoton('primario')}>
          <Plus aria-hidden size={16} strokeWidth={1.5} />
          Nueva encuesta
        </Link>
      </header>

      {encuestas.error ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar la lista de encuestas</h2>
          <p className="text-text-secondary">{encuestas.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargar()}>
            Reintentar
          </Boton>
        </section>
      ) : encuestas.cargando || !encuestas.datos ? (
        <div aria-busy="true" aria-label="Cargando encuestas" className="grid gap-2">
          {cargandoVisible && [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : encuestas.datos.length === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <ClipboardList aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">Todavía no hay encuestas</h2>
          <p className="text-text-secondary">Crea una para conocer los riesgos que perciben los trabajadores.</p>
        </section>
      ) : (
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th scope="col">Encuesta</th>
                <th scope="col">Estado</th>
                <th scope="col">Versión vigente</th>
                <th scope="col">Participaciones</th>
                <th scope="col">Creada</th>
              </tr>
            </thead>
            <tbody>
              {encuestas.datos.map((e) => (
                <tr key={e.id}>
                  <td className="envolver">
                    <Link to={`/encuestas/${e.id}`} className="font-medium">
                      {e.titulo}
                    </Link>
                  </td>
                  <td>
                    <EstadoEncuesta estado={e.estado} />
                  </td>
                  <td className="tabular-nums">{e.versionPublicadaNumero ? `Versión ${e.versionPublicadaNumero}` : '—'}</td>
                  <td className="tabular-nums">{e.participaciones}</td>
                  <td className="tabular-nums text-text-secondary">{fecha(e.creadaEn)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
