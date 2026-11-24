import { Plus, Search, TriangleAlert, Users } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { ETIQUETAS_ROL, etiquetaRol, ROLES_EMPRESA, useSesion } from '@/shared/sesion'
import { listarVinculos } from '../api/vinculos'
import { EditarAccesoModal } from '../components/EditarAccesoModal'
import { EstadoVinculo } from '../components/EstadoVinculo'
import { ETIQUETAS_ESTADO_VINCULO } from '../etiquetas'
import type { Vinculo } from '../types/vinculos'

const fecha = (iso: string) => new Date(iso).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })
const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`
// Sin tildes ni mayúsculas: "jose" encuentra a "José".
const normalizar = (texto: string) => texto.normalize('NFD').replace(/\p{Diacritic}/gu, '').toLowerCase()

const FILTROS_VACIOS = { busqueda: '', rol: '', estado: '' }

/**
 * HU-021: personas con acceso a la empresa, con su rol y estado. Desde aquí se agrega gente y se
 * edita el acceso de cada una (rol, suspender, dar de baja o reactivar). La propia fila no se
 * edita: así nadie se quita el acceso a sí mismo por error; lo cambia otra persona administradora.
 */
export function PersonasPage() {
  const sesion = useSesion()
  const miVinculo = sesion.estado === 'activa' ? sesion.usuario.vinculoId : null
  const personas = usePeticion(listarVinculos)
  const cargandoVisible = useCargaVisible(personas.cargando)
  const [lista, setLista] = useState<Vinculo[]>()
  const [editando, setEditando] = useState<Vinculo>()
  const [mensaje, setMensaje] = useState<string>()

  const { ejecutar: cargar } = personas
  useEffect(() => {
    void cargar().then((datos) => datos && setLista(datos))
  }, [cargar])

  const { register, control, reset } = useForm({ defaultValues: FILTROS_VACIOS })
  const filtros = useWatch({ control })
  const hayFiltros = Boolean(filtros.busqueda?.trim() || filtros.rol || filtros.estado)

  const visibles = useMemo(() => {
    const texto = normalizar(filtros.busqueda?.trim() ?? '')
    return (lista ?? []).filter(
      (v) =>
        (!texto || normalizar(`${v.nombreCompleto} ${v.email}`).includes(texto)) &&
        (!filtros.rol || v.rolCodigo === filtros.rol) &&
        (!filtros.estado || v.estado === filtros.estado),
    )
  }, [lista, filtros])

  return (
    <div className="mx-auto grid w-full max-w-layout gap-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h1 className="text-xl font-semibold">Personas</h1>
          <p className="text-text-secondary">Quiénes tienen acceso a la empresa en Norvia y qué rol tiene cada una.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link to="/personas/encuestados" className={clasesBoton('secundario')}>
            Agregar encuestados
          </Link>
          <Link to="/personas/nueva" className={clasesBoton('primario')}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Agregar persona
          </Link>
        </div>
      </header>

      <div role="status">{mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}</div>

      {personas.error && !lista ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar la lista de personas</h2>
          <p className="text-text-secondary">{personas.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargar().then((datos) => datos && setLista(datos))}>
            Reintentar
          </Boton>
        </section>
      ) : !lista ? (
        <div aria-busy="true" aria-label="Cargando personas" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2, 3].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <>
          <form role="search" aria-label="Filtrar personas" className="grid gap-4 sm:grid-cols-3" onSubmit={(e) => e.preventDefault()}>
            <CampoTexto etiqueta="Buscar" type="search" autoComplete="off" placeholder="Nombre o correo" registro={register('busqueda')} />
            <CampoSelect
              etiqueta="Rol"
              todos="Todos los roles"
              registro={register('rol')}
              opciones={ROLES_EMPRESA.map((r) => ({ valor: r, texto: ETIQUETAS_ROL[r] }))}
            />
            <CampoSelect
              etiqueta="Estado"
              todos="Todos los estados"
              registro={register('estado')}
              opciones={Object.entries(ETIQUETAS_ESTADO_VINCULO).map(([valor, texto]) => ({ valor, texto }))}
            />
          </form>

          {visibles.length === 0 ? (
            <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
              {hayFiltros ? (
                <Search aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
              ) : (
                <Users aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
              )}
              <h2 className="text-md font-semibold">
                {filtros.busqueda?.trim() ? `No hay resultados para «${filtros.busqueda.trim()}»` : 'No hay personas con esos filtros'}
              </h2>
              <p className="text-text-secondary">Prueba con otra palabra o quita los filtros.</p>
              <button
                type="button"
                className="inline-flex min-h-control-touch items-center text-sm font-medium text-text-link hover:text-text-link-hover sm:min-h-control"
                onClick={() => reset(FILTROS_VACIOS)}
              >
                Quitar filtros
              </button>
            </section>
          ) : (
            <div className="grid gap-2">
              <p className="text-xs text-text-secondary" aria-live="polite">
                {hayFiltros ? `${visibles.length} de ${plural(lista.length, 'persona', 'personas')}` : plural(lista.length, 'persona', 'personas')}
              </p>
              <div className="tabla-contenedor">
                <table className="tabla">
                  <thead>
                    <tr>
                      <th scope="col">Persona</th>
                      <th scope="col" className="max-sm:hidden">
                        Rol
                      </th>
                      <th scope="col" className="max-sm:hidden">
                        Estado
                      </th>
                      <th scope="col" className="max-sm:hidden">
                        Con acceso desde
                      </th>
                      <th scope="col">
                        <span className="sr-only">Acciones</span>
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {visibles.map((v) => {
                      const soyYo = v.id === miVinculo
                      return (
                        <tr key={v.id}>
                          <td className="envolver">
                            <div className="flex flex-wrap items-center gap-2">
                              <span className="font-medium">{v.nombreCompleto}</span>
                              {soyYo && <Insignia tipo="neutro">Tú</Insignia>}
                            </div>
                            <div className="text-xs text-text-secondary">{v.email}</div>
                            {/* En celular, rol y estado van aquí: así la acción queda a la vista sin desplazar la tabla. */}
                            <div className="mt-1 flex flex-wrap items-center gap-2 text-sm sm:hidden">
                              {etiquetaRol(v.rolCodigo)}
                              <EstadoVinculo estado={v.estado} />
                            </div>
                          </td>
                          <td className="max-sm:hidden">{etiquetaRol(v.rolCodigo)}</td>
                          <td className="max-sm:hidden">
                            <EstadoVinculo estado={v.estado} />
                          </td>
                          <td className="tabular-nums text-text-secondary max-sm:hidden">{fecha(v.creadoEn)}</td>
                          <td className="text-right">
                            {soyYo ? (
                              <span className="text-xs text-text-secondary max-sm:hidden">Lo cambia otra persona administradora</span>
                            ) : (
                              <Boton
                                variante="secundario"
                                aria-label={`Editar acceso de ${v.nombreCompleto}`}
                                onClick={() => {
                                  setMensaje(undefined)
                                  setEditando(v)
                                }}
                              >
                                Editar acceso
                              </Boton>
                            )}
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}

      <EditarAccesoModal
        vinculo={editando}
        onCerrar={() => setEditando(undefined)}
        onGuardado={(actualizado, texto) => {
          setLista((previa) => previa?.map((v) => (v.id === actualizado.id ? actualizado : v)))
          setEditando(undefined)
          setMensaje(texto)
        }}
      />
    </div>
  )
}
