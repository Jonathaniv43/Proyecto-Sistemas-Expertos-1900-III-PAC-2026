import { useEffect, useState } from 'react'
import { extraerError } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { CuerpoModal, Modal, PieModal } from '@/shared/components/ui/Modal'
import { useFormulario } from '@/shared/forms'
import { revisarPeligroSugerido } from '../api/encuestas'
import { confirmarPeligroEsquema } from '../schemas/revisarPeligro'
import type { PeligroRevisado } from '../types/encuestas'

export interface PeligroPorConfirmar {
  encuestaId: string
  riesgoId: string
  nombre: string
}

interface ConfirmarPeligroModalProps {
  peligro: PeligroPorConfirmar | undefined
  onCerrar: () => void
  onConfirmado: (revisado: PeligroRevisado) => void
  /** El backend dice que ya se había revisado (409): hay que recargar la lista. */
  onYaRevisado: (mensaje: string) => void
}

/** HU-035: confirmar un peligro sugerido lo pasa a la matriz; se puede indicar dónde se presenta. */
export function ConfirmarPeligroModal({ peligro, onCerrar, onConfirmado, onYaRevisado }: ConfirmarPeligroModalProps) {
  const [ocupado, setOcupado] = useState({ sucio: false, enviando: false })
  return (
    <Modal
      abierto={peligro !== undefined}
      titulo={peligro ? `Confirmar el peligro ${peligro.nombre}` : ''}
      onCerrar={onCerrar}
      hayCambios={ocupado.sucio}
      ocupado={ocupado.enviando}
    >
      {peligro && (
        <Formulario key={peligro.riesgoId} peligro={peligro} onCerrar={onCerrar} onConfirmado={onConfirmado} onYaRevisado={onYaRevisado} onEstado={setOcupado} />
      )}
    </Modal>
  )
}

function Formulario({
  peligro,
  onCerrar,
  onConfirmado,
  onYaRevisado,
  onEstado,
}: Omit<ConfirmarPeligroModalProps, 'peligro'> & {
  peligro: PeligroPorConfirmar
  onEstado: (estado: { sucio: boolean; enviando: boolean }) => void
}) {
  const {
    register,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(confirmarPeligroEsquema, {
    defaultValues: { actividad: '' },
    erroresPorCodigo: {
      'encuestas.sugerencia_no_encontrada': { mensaje: 'Esta sugerencia ya no existe. Vuelve a cargar los resultados.' },
    },
  })
  // El modal necesita saber si hay cambios (el clic fuera no cierra) y si se está guardando (no se cierra).
  useEffect(() => onEstado({ sucio: isDirty, enviando: isSubmitting }), [onEstado, isDirty, isSubmitting])

  return (
    <form
      noValidate
      onSubmit={enviar(async ({ actividad }) => {
        try {
          onConfirmado(await revisarPeligroSugerido(peligro.encuestaId, peligro.riesgoId, { estado: 'confirmado', actividad }))
        } catch (causa) {
          const error = extraerError(causa)
          if (error.codigo === 'encuestas.sugerencia_ya_revisada') {
            onYaRevisado(error.mensaje)
            return
          }
          throw causa
        }
      })}
    >
      <CuerpoModal>
        <p className="text-text-secondary">
          Pasará a la matriz de riesgos de la empresa, donde se evalúa y se le definen controles. Esta decisión no se cambia
          después.
        </p>
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <CampoTexto
          etiqueta="Actividad relacionada"
          autoComplete="off"
          registro={register('actividad')}
          error={errors.actividad}
          ayuda="Opcional. Dónde o en qué tarea se presenta; por ejemplo: Carga de camiones en bodega."
        />
      </CuerpoModal>
      <PieModal>
        <Boton variante="secundario" disabled={isSubmitting} onClick={onCerrar}>
          Cancelar
        </Boton>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Confirmando…">
          Confirmar peligro
        </Boton>
      </PieModal>
    </form>
  )
}
