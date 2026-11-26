import type { ButtonHTMLAttributes, ReactNode } from 'react'

interface BotonIconoProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'aria-label'> {
  /** Nombre accesible y tooltip: un ícono nunca comunica solo (design-system.md § Íconos). */
  etiqueta: string
  children: ReactNode
}

/** Botón de solo ícono: 36px (44px en celular), con etiqueta accesible y tooltip. */
export function BotonIcono({ etiqueta, children, className = '', type = 'button', ...props }: BotonIconoProps) {
  return (
    <button type={type} aria-label={etiqueta} title={etiqueta} className={`boton-icono ${className}`} {...props}>
      {children}
    </button>
  )
}
