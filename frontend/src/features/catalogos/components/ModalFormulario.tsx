import { useEffect, useState, type FormEventHandler, type ReactNode } from 'react'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CuerpoModal, Modal, PieModal } from '@/shared/components/ui/Modal'

export interface EstadoFormulario {
  sucio: boolean
  ocupado: boolean
}

interface ModalFormularioProps {
  abierto: boolean
  titulo: string
  onCerrar: () => void
  /** El formulario se monta solo con el modal abierto: así arranca con los valores del elemento. */
  children: (onEstado: (e: EstadoFormulario) => void) => ReactNode
}

/** Modal de formulario de los catálogos: no se cierra con clic fuera si hay cambios, ni mientras guarda. */
export function ModalFormulario({ abierto, titulo, onCerrar, children }: ModalFormularioProps) {
  const [estado, setEstado] = useState<EstadoFormulario>({ sucio: false, ocupado: false })
  return (
    <Modal abierto={abierto} titulo={titulo} onCerrar={onCerrar} hayCambios={estado.sucio} ocupado={estado.ocupado}>
      {abierto && children(setEstado)}
    </Modal>
  )
}

interface CuerpoFormularioProps {
  onSubmit: FormEventHandler<HTMLFormElement>
  onCancelar: () => void
  onEstado: (e: EstadoFormulario) => void
  sucio: boolean
  ocupado: boolean
  errorGeneral?: string
  textoEnviar: string
  children: ReactNode
}

/** Cuerpo y pie del modal: Cancelar y la acción principal, que se habilita al cambiar algo. */
export function CuerpoFormulario({ onSubmit, onCancelar, onEstado, sucio, ocupado, errorGeneral, textoEnviar, children }: CuerpoFormularioProps) {
  useEffect(() => onEstado({ sucio, ocupado }), [onEstado, sucio, ocupado])
  return (
    <form noValidate onSubmit={onSubmit}>
      <CuerpoModal>
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        {children}
      </CuerpoModal>
      <PieModal>
        <Boton variante="secundario" disabled={ocupado} onClick={onCancelar}>
          Cancelar
        </Boton>
        <Boton type="submit" disabled={!sucio} cargando={ocupado} textoCargando="Guardando…">
          {textoEnviar}
        </Boton>
      </PieModal>
    </form>
  )
}
