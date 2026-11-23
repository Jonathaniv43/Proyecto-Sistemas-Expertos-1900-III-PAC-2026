import { useEffect, useId, useRef, type ReactNode } from 'react'
import { Boton } from './Boton'

interface DialogoConfirmacionProps {
  abierto: boolean
  /** Pregunta con la acción y el objeto: "¿Cerrar la encuesta Condiciones 2026?" */
  titulo: string
  /** La consecuencia, en una línea. */
  children: ReactNode
  /** Verbo concreto, nunca "Aceptar": "Publicar", "Cerrar encuesta". */
  textoConfirmar: string
  textoCargando?: string
  /** Acción que no se deshace: botón de peligro y el foco empieza en "Cancelar". */
  peligro?: boolean
  cargando?: boolean
  onConfirmar: () => void
  onCancelar: () => void
}

/**
 * Diálogo de confirmación (design-system.md § Diálogo de confirmación) sobre <dialog> nativo: el foco
 * queda dentro, Esc cancela, no se cierra con clic fuera y al cerrar el foco vuelve a quien lo abrió.
 */
export function DialogoConfirmacion({
  abierto,
  titulo,
  children,
  textoConfirmar,
  textoCargando,
  peligro = false,
  cargando = false,
  onConfirmar,
  onCancelar,
}: DialogoConfirmacionProps) {
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
      className="dialogo"
      aria-labelledby={`${id}-titulo`}
      aria-describedby={`${id}-texto`}
      onCancel={(e) => {
        e.preventDefault()
        if (!cargando) onCancelar()
      }}
    >
      <h2 id={`${id}-titulo`} className="text-md font-semibold">
        {titulo}
      </h2>
      <div id={`${id}-texto`} className="mt-2 text-text-secondary">
        {children}
      </div>
      <div className="mt-6 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
        <Boton variante="secundario" autoFocus={peligro} disabled={cargando} onClick={onCancelar}>
          Cancelar
        </Boton>
        <Boton
          variante={peligro ? 'peligro' : 'primario'}
          autoFocus={!peligro}
          cargando={cargando}
          textoCargando={textoCargando}
          onClick={onConfirmar}
        >
          {textoConfirmar}
        </Boton>
      </div>
    </dialog>
  )
}
