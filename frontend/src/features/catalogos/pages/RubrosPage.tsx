import { Plus } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { useFormulario } from '@/shared/forms'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { crearRubro, editarRubro, estadoRubro, listarRubros } from '../api/catalogos'
import { AvisoMensaje } from '../components/AvisoMensaje'
import { BotonEstado, EstadoActivo } from '../components/Estado'
import { EstadoCarga } from '../components/EstadoCarga'
import { CuerpoFormulario, ModalFormulario, type EstadoFormulario } from '../components/ModalFormulario'
import { ERRORES_CATALOGO } from '../etiquetas'
import { usePuedeEditar } from '../permisos'
import { rubroEsquema } from '../schemas/catalogos'
import type { Rubro } from '../types/catalogos'
import { useCambioEstado } from '../useCambioEstado'

/** HU-023: rubros de las empresas (RN-09). Un rubro inactivo no se ofrece al registrar empresas nuevas. */
export function RubrosPage() {
  const peticion = usePeticion(listarRubros)
  const puedeEditar = usePuedeEditar()
  const [editando, setEditando] = useState<Rubro | 'nuevo'>()
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(), [ejecutar])
  useEffect(cargar, [cargar])
  const { mensaje, setMensaje, ocupado, cambiar } = useCambioEstado(cargar)

  return (
    <section aria-labelledby="titulo-rubros" className="grid gap-4">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h2 id="titulo-rubros" className="text-md font-semibold">
            Rubros
          </h2>
          <p className="text-text-secondary">A qué se dedica cada empresa. Cada rubro tiene sus categorías de riesgo.</p>
        </div>
        {puedeEditar && (
          <Boton onClick={() => setEditando('nuevo')}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Nuevo rubro
          </Boton>
        )}
      </div>
      <AvisoMensaje mensaje={mensaje} />
      <EstadoCarga cargando={peticion.cargando} error={peticion.error} hayDatos={!!peticion.datos} que="los rubros" onReintentar={cargar}>
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th scope="col">Rubro</th>
                <th scope="col">Estado</th>
                {puedeEditar && (
                  <th scope="col">
                    <span className="sr-only">Acciones</span>
                  </th>
                )}
              </tr>
            </thead>
            <tbody>
              {peticion.datos?.map((r) => (
                <tr key={r.id}>
                  <td>
                    <div className="font-medium">{r.nombre}</div>
                    <div className="text-xs text-text-secondary">{r.codigo}</div>
                  </td>
                  <td>
                    <EstadoActivo activo={r.activo} />
                  </td>
                  {puedeEditar && (
                    <td className="text-right">
                      <Boton variante="secundario" aria-label={`Editar «${r.nombre}»`} onClick={() => setEditando(r)}>
                        Editar
                      </Boton>
                      <BotonEstado
                        activo={r.activo}
                        nombre={r.nombre}
                        ocupado={ocupado !== undefined}
                        onCambiar={(activo) => void cambiar(String(r.id), r.nombre, activo, () => estadoRubro(r.id, activo))}
                      />
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </EstadoCarga>
      <ModalFormulario
        abierto={editando !== undefined}
        titulo={editando === 'nuevo' ? 'Nuevo rubro' : 'Editar rubro'}
        onCerrar={() => setEditando(undefined)}
      >
        {(onEstado) => (
          <FormularioRubro
            rubro={editando === 'nuevo' ? undefined : editando}
            onEstado={onEstado}
            onCancelar={() => setEditando(undefined)}
            onGuardado={(r) => {
              setEditando(undefined)
              setMensaje({ tipo: 'exito', texto: `«${r.nombre}» quedó guardado.` })
              cargar()
            }}
          />
        )}
      </ModalFormulario>
    </section>
  )
}

interface FormularioRubroProps {
  rubro: Rubro | undefined
  onEstado: (e: EstadoFormulario) => void
  onCancelar: () => void
  onGuardado: (r: Rubro) => void
}

function FormularioRubro({ rubro, onEstado, onCancelar, onGuardado }: FormularioRubroProps) {
  const {
    register,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(rubroEsquema, {
    defaultValues: { codigo: rubro?.codigo ?? '', nombre: rubro?.nombre ?? '' },
    erroresPorCodigo: {
      [ERRORES_CATALOGO.codigoDuplicado]: { campo: 'codigo', mensaje: 'Ya hay un rubro con ese código.' },
      [ERRORES_CATALOGO.nombreDuplicado]: { campo: 'nombre', mensaje: 'Ya hay un rubro con ese nombre.' },
    },
  })
  return (
    <CuerpoFormulario
      onSubmit={enviar(async (d) => onGuardado(rubro ? await editarRubro(rubro.id, { nombre: d.nombre }) : await crearRubro(d)))}
      onCancelar={onCancelar}
      onEstado={onEstado}
      sucio={isDirty}
      ocupado={isSubmitting}
      errorGeneral={errorGeneral}
      textoEnviar={rubro ? 'Guardar cambios' : 'Crear rubro'}
    >
      <CampoTexto etiqueta="Nombre" required autoComplete="off" registro={register('nombre')} error={errors.nombre} />
      <CampoTexto
        etiqueta="Código"
        required
        autoComplete="off"
        readOnly={!!rubro}
        registro={register('codigo')}
        error={errors.codigo}
        ayuda={rubro ? 'El código no se cambia.' : 'Minúsculas, números y guion bajo. Por ejemplo: servicios. No se puede cambiar después.'}
      />
    </CuerpoFormulario>
  )
}
