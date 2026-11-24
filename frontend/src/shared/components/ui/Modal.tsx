import { X } from 'lucide-react'
import { useEffect, useId, useRef, type ReactNode } from 'react'
import { BotonIcono } from './BotonIcono'

interface ModalProps {
  abierto: boolean
  titulo: string
  /** Contenido: normalmente un <form> con <CuerpoModal> y <PieModal>. */
  children: ReactNode
  onCerrar: () => void
  /** Con cambios sin guardar, el clic fuera no cierra (Esc y la X sí). */
  hayCambios?: boolean
  /** Mientras se guarda no se puede cerrar. */
  ocupado?: boolean
}

/**
 * Modal de formulario (design-system.md § Modal) sobre <dialog> nativo: mediano (560px), encabezado
 * y pie fijos con el medio desplazable, Esc cierra, el foco queda dentro y al cerrar vuelve al botón
 * que lo abrió. El clic fuera cierra solo si no hay cambios sin guardar. En celular ocupa la pantalla.
 */
export function Modal({ abierto, titulo, children, onCerrar, hayCambios = false, ocupado = false }: ModalProps) {
  const dialogo = useRef<HTMLDialogElement>(null)
  const id = useId()

  useEffect(() => {
    const d = dialogo.current
    if (!d) return
    if (abierto && !d.open) d.showModal()
    if (!abierto && d.open) d.close()
  }, [abierto])

  return (
    <dialog
      ref={dialogo}
      className="modal"
      aria-labelledby={`${id}-titulo`}
      onCancel={(e) => {
        e.preventDefault()
        if (!ocupado) onCerrar()
      }}
      // El fondo es el propio <dialog>: el contenido lo cubre entero, así que solo el clic fuera llega aquí.
      onClick={(e) => {
        if (e.target === e.currentTarget && !hayCambios && !ocupado) onCerrar()
      }}
    >
      {abierto && (
        <>
          <div className="modal-cabecera">
            <h2 id={`${id}-titulo`} className="text-md font-semibold">
              {titulo}
            </h2>
            <BotonIcono etiqueta="Cerrar" onClick={onCerrar} disabled={ocupado}>
              <X aria-hidden size={20} strokeWidth={1.5} />
            </BotonIcono>
          </div>
          {children}
        </>
      )}
    </dialog>
  )
}

/** Parte desplazable del modal. */
export function CuerpoModal({ children }: { children: ReactNode }) {
  return <div className="modal-cuerpo grid gap-6">{children}</div>
}

/** Acciones a la derecha (secundaria y luego principal); en celular, apiladas con la principal arriba. */
export function PieModal({ children }: { children: ReactNode }) {
  return <div className="modal-pie flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">{children}</div>
}
