import { useEffect, useMemo, useState } from 'react'
import { useController } from 'react-hook-form'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoFecha } from '@/shared/components/ui/CampoFecha'
import { CuerpoModal, Modal, PieModal } from '@/shared/components/ui/Modal'
import { SelectBusqueda } from '@/shared/components/ui/SelectBusqueda'
import { useFormulario } from '@/shared/forms'
import { hoyHonduras, isoATexto } from '@/shared/validacion'
import { asignarTarea } from '../api/plan'
import { opcionesResponsable } from '../etiquetas'
import { asignacionEsquema } from '../schemas/asignacion'
import type { Responsable, Tarea } from '../types/plan'

interface AsignarTareaModalProps {
  tarea: Tarea | undefined
  responsables: Responsable[]
  onCerrar: () => void
  onGuardada: (tarea: Tarea) => void
}

/** HU-043: responsable y fecha límite de una tarea. Se monta por tarea, así arranca con sus valores. */
export function AsignarTareaModal({ tarea, responsables, onCerrar, onGuardada }: AsignarTareaModalProps) {
  const [estado, setEstado] = useState({ sucio: false, ocupado: false })
  return (
    <Modal
      abierto={tarea !== undefined}
      titulo={tarea?.responsableId ? 'Reasignar la tarea' : 'Asignar la tarea'}
      onCerrar={onCerrar}
      hayCambios={estado.sucio}
      ocupado={estado.ocupado}
    >
      {tarea && (
        <Formulario key={tarea.id} tarea={tarea} responsables={responsables} onCerrar={onCerrar} onGuardada={onGuardada} onEstado={setEstado} />
      )}
    </Modal>
  )
}

function Formulario({
  tarea,
  responsables,
  onCerrar,
  onGuardada,
  onEstado,
}: Omit<AsignarTareaModalProps, 'tarea'> & { tarea: Tarea; onEstado: (e: { sucio: boolean; ocupado: boolean }) => void }) {
  const esquema = useMemo(() => asignacionEsquema(tarea.fechaLimite), [tarea.fechaLimite])
  const {
    control,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(esquema, {
    defaultValues: { responsableId: tarea.responsableId ?? '', fechaLimite: tarea.fechaLimite ? isoATexto(tarea.fechaLimite) : '' },
    erroresPorCodigo: {
      'plan.responsable_invalido': {
        campo: 'responsableId',
        mensaje: 'Esa persona ya no está activa en la empresa. Elige a otra.',
      },
      'plan.fecha_pasada': { campo: 'fechaLimite', mensaje: 'La fecha límite no puede ser anterior a hoy.' },
      'plan.tarea_cerrada': { mensaje: 'La tarea ya se completó o se canceló, así que no se reasigna.' },
    },
  })
  const { field: responsable } = useController({ control, name: 'responsableId' })
  const { field: fecha } = useController({ control, name: 'fechaLimite' })
  useEffect(() => onEstado({ sucio: isDirty, ocupado: isSubmitting }), [onEstado, isDirty, isSubmitting])

  return (
    <form noValidate onSubmit={enviar(async (datos) => onGuardada(await asignarTarea(tarea.id, datos)))}>
      <CuerpoModal>
        <p className="font-medium">{tarea.titulo}</p>
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <SelectBusqueda
          etiqueta="Responsable"
          required
          etiquetaBusqueda="Buscar persona"
          opciones={opcionesResponsable(responsables)}
          valor={responsable.value}
          onChange={responsable.onChange}
          onBlur={responsable.onBlur}
          error={errors.responsableId?.message}
          ayuda="Personas activas de la empresa."
        />
        <CampoFecha
          etiqueta="Fecha límite"
          required
          valor={fecha.value}
          onChange={fecha.onChange}
          onBlur={fecha.onBlur}
          error={errors.fechaLimite?.message}
          minimo={hoyHonduras()}
        />
      </CuerpoModal>
      <PieModal>
        <Boton variante="secundario" disabled={isSubmitting} onClick={onCerrar}>
          Cancelar
        </Boton>
        <Boton type="submit" disabled={!isDirty} cargando={isSubmitting} textoCargando="Guardando…">
          {tarea.responsableId ? 'Guardar cambios' : 'Asignar'}
        </Boton>
      </PieModal>
    </form>
  )
}
