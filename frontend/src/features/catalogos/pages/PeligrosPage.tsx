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
import { crearPeligro, editarPeligro, estadoPeligro, listarPeligros, listarPlantillasTarea } from '../api/catalogos'
import { AvisoMensaje } from '../components/AvisoMensaje'
import { BotonEstado, EstadoActivo } from '../components/Estado'
import { EstadoCarga } from '../components/EstadoCarga'
import { CuerpoFormulario, ModalFormulario, type EstadoFormulario } from '../components/ModalFormulario'
import { CLASIFICACIONES, ERRORES_CATALOGO } from '../etiquetas'
import { usePuedeEditar } from '../permisos'
import { peligroEsquema } from '../schemas/catalogos'
import type { ClasificacionPeligro, Peligro } from '../types/catalogos'
import { useCambioEstado } from '../useCambioEstado'

// Sin tildes ni mayúsculas: "electrico" encuentra "Eléctrico".
const normalizar = (t: string) => t.normalize('NFD').replace(/\p{Diacritic}/gu, '').toLowerCase()
const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

/**
 * HU-023: peligros del catálogo. Las preguntas los detectan (HU-033), las empresas los eligen al agregar riesgos
 * (HU-036) y sus plantillas sugieren controles y tareas (HU-039, HU-042). Un peligro inactivo deja de ofrecerse; los
 * riesgos que ya lo usan lo conservan.
 */
export function PeligrosPage() {
  const peligros = usePeticion(listarPeligros)
  const plantillas = usePeticion(listarPlantillasTarea)
  const puedeEditar = usePuedeEditar()
  const [editando, setEditando] = useState<Peligro | 'nuevo'>()
  const { register, control } = useForm({ defaultValues: { buscar: '', clasificacion: '' } })
  const { buscar, clasificacion } = useWatch({ control })

  const { ejecutar: cargarPeligros } = peligros
  const { ejecutar: cargarPlantillas } = plantillas
  const cargar = useCallback(() => {
    void cargarPeligros()
    void cargarPlantillas()
  }, [cargarPeligros, cargarPlantillas])
  useEffect(cargar, [cargar])
  const { mensaje, setMensaje, ocupado, cambiar } = useCambioEstado(() => void cargarPeligros())

  const texto = normalizar(buscar?.trim() ?? '')
  const visibles = (peligros.datos ?? []).filter(
    (p) => (!clasificacion || p.clasificacion === clasificacion) && (!texto || normalizar(`${p.nombre} ${p.codigo}`).includes(texto)),
  )
  const conteo = (id: string) => {
    const de = (plantillas.datos ?? []).filter((t) => t.peligroId === id && t.activa)
    return { controles: de.filter((t) => t.tipoControl).length, tareas: de.filter((t) => !t.tipoControl).length }
  }

  return (
    <section aria-labelledby="titulo-peligros" className="grid gap-4">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h2 id="titulo-peligros" className="text-md font-semibold">
            Peligros y plantillas
          </h2>
          <p className="text-text-secondary">Cada peligro tiene plantillas: los controles que Norvia sugiere y las tareas modelo del plan.</p>
        </div>
        {puedeEditar && (
          <Boton variante="secundario" onClick={() => setEditando('nuevo')}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Nuevo peligro
          </Boton>
        )}
      </div>
      <form role="search" aria-label="Filtrar peligros" className="grid gap-4 sm:grid-cols-2" onSubmit={(e) => e.preventDefault()}>
        <CampoTexto etiqueta="Buscar" type="search" autoComplete="off" placeholder="Nombre o código" registro={register('buscar')} />
        <CampoSelect
          etiqueta="Clasificación"
          todos="Todas"
          registro={register('clasificacion')}
          opciones={Object.entries(CLASIFICACIONES).map(([valor, texto]) => ({ valor, texto }))}
        />
      </form>
      <AvisoMensaje mensaje={mensaje} />
      <EstadoCarga
        cargando={peligros.cargando}
        error={peligros.error}
        hayDatos={!!peligros.datos}
        que="los peligros"
        onReintentar={cargar}
      >
        {visibles.length === 0 ? (
          <p className="text-text-secondary">No hay peligros con esos filtros.</p>
        ) : (
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Peligro</th>
                  <th scope="col" className="max-sm:hidden">
                    Clasificación
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Plantillas activas
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
                {visibles.map((p) => {
                  const n = conteo(p.id)
                  const resumen = plantillas.datos
                    ? `${plural(n.controles, 'control', 'controles')} · ${plural(n.tareas, 'tarea', 'tareas')}`
                    : '…'
                  return (
                    <tr key={p.id}>
                      <td className="envolver">
                        <Link to={`/catalogos/peligros/${p.id}`} className="font-medium">
                          {p.nombre}
                        </Link>
                        <div className="text-xs text-text-secondary">{p.codigo}</div>
                        <div className="mt-1 grid justify-items-start gap-1 text-sm sm:hidden">
                          <span>{CLASIFICACIONES[p.clasificacion]}</span>
                          <span className="text-text-secondary">{resumen}</span>
                          <EstadoActivo activo={p.activo} />
                        </div>
                      </td>
                      <td className="max-sm:hidden">{CLASIFICACIONES[p.clasificacion]}</td>
                      <td className="tabular-nums text-text-secondary max-sm:hidden">{resumen}</td>
                      <td className="max-sm:hidden">
                        <EstadoActivo activo={p.activo} />
                      </td>
                      <td className="text-right">
                        {puedeEditar && (
                          <span className="inline-flex flex-wrap items-center justify-end gap-1">
                            <Boton variante="secundario" aria-label={`Editar «${p.nombre}»`} onClick={() => setEditando(p)}>
                              Editar
                            </Boton>
                            <BotonEstado
                              activo={p.activo}
                              nombre={p.nombre}
                              ocupado={ocupado !== undefined}
                              onCambiar={(activo) => void cambiar(p.id, p.nombre, activo, () => estadoPeligro(p.id, activo))}
                            />
                          </span>
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </EstadoCarga>
      <ModalFormulario
        abierto={editando !== undefined}
        titulo={editando === 'nuevo' ? 'Nuevo peligro' : 'Editar peligro'}
        onCerrar={() => setEditando(undefined)}
      >
        {(onEstado) => (
          <FormularioPeligro
            peligro={editando === 'nuevo' ? undefined : editando}
            onEstado={onEstado}
            onCancelar={() => setEditando(undefined)}
            onGuardado={(p) => {
              setEditando(undefined)
              setMensaje({ tipo: 'exito', texto: `«${p.nombre}» quedó guardado.` })
              void cargarPeligros()
            }}
          />
        )}
      </ModalFormulario>
    </section>
  )
}

interface FormularioPeligroProps {
  peligro: Peligro | undefined
  onEstado: (e: EstadoFormulario) => void
  onCancelar: () => void
  onGuardado: (p: Peligro) => void
}

function FormularioPeligro({ peligro, onEstado, onCancelar, onGuardado }: FormularioPeligroProps) {
  const {
    register,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(peligroEsquema, {
    defaultValues: {
      codigo: peligro?.codigo ?? '',
      nombre: peligro?.nombre ?? '',
      descripcion: peligro?.descripcion ?? '',
      clasificacion: (peligro?.clasificacion ?? '') as ClasificacionPeligro,
    },
    erroresPorCodigo: { [ERRORES_CATALOGO.codigoDuplicado]: { campo: 'codigo', mensaje: 'Ya hay un peligro con ese código.' } },
  })
  return (
    <CuerpoFormulario
      onSubmit={enviar(async (d) =>
        onGuardado(
          peligro
            ? await editarPeligro(peligro.id, { nombre: d.nombre, descripcion: d.descripcion, clasificacion: d.clasificacion })
            : await crearPeligro({ ...d, descripcion: d.descripcion || undefined }),
        ),
      )}
      onCancelar={onCancelar}
      onEstado={onEstado}
      sucio={isDirty}
      ocupado={isSubmitting}
      errorGeneral={errorGeneral}
      textoEnviar={peligro ? 'Guardar cambios' : 'Crear peligro'}
    >
      <CampoTexto etiqueta="Nombre" required autoComplete="off" registro={register('nombre')} error={errors.nombre} />
      <CampoSelect
        etiqueta="Clasificación"
        required
        registro={register('clasificacion')}
        error={errors.clasificacion}
        opciones={Object.entries(CLASIFICACIONES).map(([valor, texto]) => ({ valor, texto }))}
      />
      <AreaTexto
        etiqueta="Descripción"
        ayuda="Opcional. Qué es y dónde se presenta; la ven las empresas al elegirlo."
        error={errors.descripcion?.message}
        {...register('descripcion')}
      />
      <CampoTexto
        etiqueta="Código"
        required
        autoComplete="off"
        readOnly={!!peligro}
        registro={register('codigo')}
        error={errors.codigo}
        ayuda={peligro ? 'El código no se cambia.' : 'Minúsculas, números y guion bajo. Por ejemplo: fis_ruido. No se puede cambiar después.'}
      />
    </CuerpoFormulario>
  )
}
