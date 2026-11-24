import { Megaphone, Search, TriangleAlert } from 'lucide-react'
import { useEffect, useMemo } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarEventos } from '../api/eventos'
import { ESTADOS_EVENTO, fechaHora, NOMBRE_TIPO, TIPOS_EVENTO } from '../etiquetas'
import type { EstadoEvento, Evento, FiltrosEventos, TipoEvento } from '../types/eventos'

const VACIOS = { tipo: '', estado: '', gravedad: '' }
const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

/**
 * HU-047: los eventos reportados en la empresa, el último recibido primero, con filtros por tipo, estado y gravedad.
 * Son anónimos: ninguna pantalla muestra quién los reportó (RN-27). Investigarlos y cerrarlos es HU-049.
 */
export function EventosPage() {
  const peticion = usePeticion(listarEventos)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { register, control, reset } = useForm({ defaultValues: VACIOS })
  const valores = useWatch({ control })
  const filtros = useMemo<FiltrosEventos>(() => {
    const f: FiltrosEventos = {}
    if (valores.tipo) f.tipo = valores.tipo as TipoEvento
    if (valores.estado) f.estado = valores.estado as EstadoEvento
    if (valores.gravedad) f.esGrave = valores.gravedad === 'graves'
    return f
  }, [valores.tipo, valores.estado, valores.gravedad])
  const hayFiltros = Object.keys(filtros).length > 0

  const { ejecutar } = peticion
  useEffect(() => {
    void ejecutar(filtros)
  }, [ejecutar, filtros])

  const eventos = peticion.datos

  return (
    <div className="mx-auto grid w-full max-w-layout grid-cols-1 gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h1 className="text-xl font-semibold">Eventos reportados</h1>
          <p className="text-text-secondary">Accidentes, incidentes, enfermedades y condiciones inseguras. Los reportes son anónimos.</p>
        </div>
        <Link to="/eventos/reportar" className={clasesBoton('secundario')}>
          <Megaphone aria-hidden size={16} strokeWidth={1.5} />
          Reportar un evento
        </Link>
      </header>

      <form role="search" aria-label="Filtrar eventos" className="grid gap-4 sm:grid-cols-3" onSubmit={(e) => e.preventDefault()}>
        <CampoSelect
          etiqueta="Tipo"
          todos="Todos los tipos"
          registro={register('tipo')}
          opciones={TIPOS_EVENTO.map((t) => ({ valor: t.valor, texto: t.texto }))}
        />
        <CampoSelect
          etiqueta="Estado"
          todos="Todos los estados"
          registro={register('estado')}
          opciones={Object.entries(ESTADOS_EVENTO).map(([valor, e]) => ({ valor, texto: e.texto }))}
        />
        <CampoSelect
          etiqueta="Gravedad"
          todos="Graves y no graves"
          registro={register('gravedad')}
          opciones={[
            { valor: 'graves', texto: 'Solo los graves' },
            { valor: 'no_graves', texto: 'Solo los no graves' },
          ]}
        />
      </form>

      {peticion.error && !eventos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar los eventos</h2>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void ejecutar(filtros)}>
            Reintentar
          </Boton>
        </section>
      ) : !eventos ? (
        <div aria-busy="true" aria-label="Cargando los eventos" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : eventos.length === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          {hayFiltros ? (
            <>
              <Search aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
              <h2 className="text-md font-semibold">No hay eventos con esos filtros</h2>
              <button
                type="button"
                className="inline-flex min-h-control-touch items-center text-sm font-medium text-text-link hover:text-text-link-hover sm:min-h-control"
                onClick={() => reset(VACIOS)}
              >
                Quitar filtros
              </button>
            </>
          ) : (
            <>
              <Megaphone aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
              <h2 className="text-md font-semibold">Todavía no hay eventos reportados</h2>
              <p className="max-w-layout-narrow text-text-secondary">
                Cualquier persona de la empresa puede reportar uno desde su inicio, de forma anónima.
              </p>
            </>
          )}
        </section>
      ) : (
        <div className="grid gap-2" aria-busy={peticion.cargando}>
          <p className="text-xs text-text-secondary" aria-live="polite">
            {plural(eventos.length, 'evento', 'eventos')}, el último recibido primero
          </p>
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Evento</th>
                  <th scope="col" className="max-sm:hidden">
                    Ocurrió
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Lugar
                  </th>
                  <th scope="col">Estado</th>
                </tr>
              </thead>
              <tbody>
                {eventos.map((e) => (
                  <FilaEvento key={e.id} evento={e} />
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}

function FilaEvento({ evento: e }: { evento: Evento }) {
  const estado = ESTADOS_EVENTO[e.estado]
  return (
    <tr>
      <td className="envolver">
        <div className="flex flex-wrap items-center gap-2">
          <Link to={`/eventos/${e.id}`} className="font-medium">
            {NOMBRE_TIPO[e.tipo]}
          </Link>
          {e.esGrave && <Insignia tipo="error">Grave</Insignia>}
        </div>
        <div className="line-clamp-2 text-sm text-text-secondary">{e.descripcion}</div>
        <div className="mt-1 grid gap-1 text-xs text-text-secondary sm:hidden">
          <span>{fechaHora(e.fechaHoraEvento)}</span>
          {e.lugar && <span>{e.lugar}</span>}
        </div>
      </td>
      <td className="tabular-nums max-sm:hidden">{fechaHora(e.fechaHoraEvento)}</td>
      <td className="envolver max-sm:hidden">{e.lugar ?? <span className="text-text-tertiary">—</span>}</td>
      <td>
        <Insignia tipo={estado.tipo}>{estado.texto}</Insignia>
      </td>
    </tr>
  )
}
