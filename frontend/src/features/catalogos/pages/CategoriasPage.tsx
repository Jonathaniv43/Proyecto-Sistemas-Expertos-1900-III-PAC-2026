import { Plus } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { useFormulario } from '@/shared/forms'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { crearCategoria, editarCategoria, estadoCategoria, listarCategorias, listarRubros } from '../api/catalogos'
import { AvisoMensaje } from '../components/AvisoMensaje'
import { BotonEstado, EstadoActivo } from '../components/Estado'
import { EstadoCarga } from '../components/EstadoCarga'
import { CuerpoFormulario, ModalFormulario, type EstadoFormulario } from '../components/ModalFormulario'
import { ERRORES_CATALOGO } from '../etiquetas'
import { usePuedeEditar } from '../permisos'
import { categoriaEsquema } from '../schemas/catalogos'
import type { CategoriaRiesgo, Rubro } from '../types/catalogos'
import { useCambioEstado } from '../useCambioEstado'

/**
 * HU-023: categorías de riesgo por rubro (D7). Una empresa ve solo las activas de su rubro al crear una encuesta
 * (HU-029); desactivar una no cambia las encuestas ya generadas (RN-12).
 */
export function CategoriasPage() {
  const categorias = usePeticion(listarCategorias)
  const rubros = usePeticion(listarRubros)
  const puedeEditar = usePuedeEditar()
  const [editando, setEditando] = useState<CategoriaRiesgo | 'nueva'>()
  const { register, control } = useForm({ defaultValues: { rubro: '' } })
  const rubro = useWatch({ control, name: 'rubro' })

  const { ejecutar: cargarCategorias } = categorias
  const { ejecutar: cargarRubros } = rubros
  const cargar = useCallback(() => {
    void cargarCategorias()
    void cargarRubros()
  }, [cargarCategorias, cargarRubros])
  useEffect(cargar, [cargar])
  const { mensaje, setMensaje, ocupado, cambiar } = useCambioEstado(() => void cargarCategorias())

  const nombreRubro = (id: number) => rubros.datos?.find((r) => r.id === id)?.nombre ?? '—'
  const visibles = (categorias.datos ?? [])
    .filter((c) => !rubro || c.rubroId === Number(rubro))
    .sort((a, b) => nombreRubro(a.rubroId).localeCompare(nombreRubro(b.rubroId)) || a.nombre.localeCompare(b.nombre))

  return (
    <section aria-labelledby="titulo-categorias" className="grid gap-4">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h2 id="titulo-categorias" className="text-md font-semibold">
            Categorías de riesgo
          </h2>
          <p className="text-text-secondary">Cada categoría agrupa preguntas de la encuesta y pertenece a un rubro.</p>
        </div>
        {puedeEditar && (
          <Boton variante="secundario" onClick={() => setEditando('nueva')}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Nueva categoría
          </Boton>
        )}
      </div>
      <form role="search" aria-label="Filtrar categorías" className="grid sm:max-w-xs" onSubmit={(e) => e.preventDefault()}>
        <CampoSelect
          etiqueta="Rubro"
          todos="Todos los rubros"
          registro={register('rubro')}
          opciones={(rubros.datos ?? []).map((r) => ({ valor: String(r.id), texto: r.nombre }))}
        />
      </form>
      <AvisoMensaje mensaje={mensaje} />
      <EstadoCarga
        cargando={categorias.cargando || rubros.cargando}
        error={categorias.error ?? rubros.error}
        hayDatos={!!categorias.datos && !!rubros.datos}
        que="las categorías"
        onReintentar={cargar}
      >
        {visibles.length === 0 ? (
          <p className="text-text-secondary">No hay categorías en este rubro.</p>
        ) : (
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Categoría</th>
                  <th scope="col" className="max-sm:hidden">
                    Rubro
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Estado
                  </th>
                  <th scope="col">
                    <span className="sr-only">Acciones</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {visibles.map((c) => (
                  <tr key={c.id}>
                    <td className="envolver">
                      <div className="font-medium">{c.nombre}</div>
                      {c.descripcion && <div className="text-xs text-text-secondary">{c.descripcion}</div>}
                      <div className="mt-1 flex flex-wrap items-center gap-2 text-sm sm:hidden">
                        {nombreRubro(c.rubroId)}
                        <EstadoActivo activo={c.activa} />
                      </div>
                    </td>
                    <td className="max-sm:hidden">{nombreRubro(c.rubroId)}</td>
                    <td className="max-sm:hidden">
                      <EstadoActivo activo={c.activa} />
                    </td>
                    <td className="text-right">
                      <span className="inline-flex flex-wrap items-center justify-end gap-1">
                        <Link to={`/catalogos/preguntas?categoria=${c.id}`} className="px-2 text-sm font-medium">
                          Preguntas
                        </Link>
                        {puedeEditar && (
                          <>
                            <Boton variante="secundario" aria-label={`Editar «${c.nombre}»`} onClick={() => setEditando(c)}>
                              Editar
                            </Boton>
                            <BotonEstado
                              activo={c.activa}
                              nombre={c.nombre}
                              ocupado={ocupado !== undefined}
                              onCambiar={(activo) => void cambiar(c.id, c.nombre, activo, () => estadoCategoria(c.id, activo))}
                            />
                          </>
                        )}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </EstadoCarga>
      <ModalFormulario
        abierto={editando !== undefined}
        titulo={editando === 'nueva' ? 'Nueva categoría' : 'Editar categoría'}
        onCerrar={() => setEditando(undefined)}
      >
        {(onEstado) => (
          <FormularioCategoria
            categoria={editando === 'nueva' ? undefined : editando}
            rubros={rubros.datos ?? []}
            rubroInicial={rubro}
            onEstado={onEstado}
            onCancelar={() => setEditando(undefined)}
            onGuardada={(c) => {
              setEditando(undefined)
              setMensaje({ tipo: 'exito', texto: `«${c.nombre}» quedó guardada.` })
              void cargarCategorias()
            }}
          />
        )}
      </ModalFormulario>
    </section>
  )
}

interface FormularioCategoriaProps {
  categoria: CategoriaRiesgo | undefined
  rubros: Rubro[]
  rubroInicial: string
  onEstado: (e: EstadoFormulario) => void
  onCancelar: () => void
  onGuardada: (c: CategoriaRiesgo) => void
}

function FormularioCategoria({ categoria, rubros, rubroInicial, onEstado, onCancelar, onGuardada }: FormularioCategoriaProps) {
  const {
    register,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(categoriaEsquema, {
    defaultValues: {
      rubroId: categoria ? String(categoria.rubroId) : rubroInicial,
      nombre: categoria?.nombre ?? '',
      descripcion: categoria?.descripcion ?? '',
    },
    erroresPorCodigo: {
      [ERRORES_CATALOGO.nombreDuplicado]: { campo: 'nombre', mensaje: 'Ese rubro ya tiene una categoría con ese nombre.' },
      'catalogos.rubro_invalido': { campo: 'rubroId', mensaje: 'Ese rubro está desactivado. Elige otro.' },
    },
  })
  // Los rubros desactivados no se ofrecen, salvo el que ya tenía la categoría.
  const opciones = rubros.filter((r) => r.activo || r.id === categoria?.rubroId).map((r) => ({ valor: String(r.id), texto: r.nombre }))
  return (
    <CuerpoFormulario
      onSubmit={enviar(async (d) =>
        onGuardada(
          categoria
            ? await editarCategoria(categoria.id, { rubroId: d.rubroId, nombre: d.nombre, descripcion: d.descripcion })
            : await crearCategoria({ ...d, descripcion: d.descripcion || undefined }),
        ),
      )}
      onCancelar={onCancelar}
      onEstado={onEstado}
      sucio={isDirty}
      ocupado={isSubmitting}
      errorGeneral={errorGeneral}
      textoEnviar={categoria ? 'Guardar cambios' : 'Crear categoría'}
    >
      <CampoSelect etiqueta="Rubro" required registro={register('rubroId')} error={errors.rubroId} opciones={opciones} />
      <CampoTexto etiqueta="Nombre" required autoComplete="off" registro={register('nombre')} error={errors.nombre} />
      <AreaTexto
        etiqueta="Descripción"
        ayuda="Opcional. El Responsable SST la ve al elegir las categorías de su encuesta."
        error={errors.descripcion?.message}
        {...register('descripcion')}
      />
    </CuerpoFormulario>
  )
}
