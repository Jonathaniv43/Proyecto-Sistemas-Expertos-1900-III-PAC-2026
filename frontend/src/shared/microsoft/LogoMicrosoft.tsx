/**
 * Logo oficial de Microsoft, sin modificar (design-system.md § Elegir cómo entrar). Conserva sus cuatro colores:
 * es la única excepción a «solo variables semánticas» y no cambia con el modo oscuro ni por empresa.
 */
export function LogoMicrosoft() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true" focusable="false">
      <rect x="1" y="1" width="6.5" height="6.5" fill="#F25022" />
      <rect x="8.5" y="1" width="6.5" height="6.5" fill="#7FBA00" />
      <rect x="1" y="8.5" width="6.5" height="6.5" fill="#00A4EF" />
      <rect x="8.5" y="8.5" width="6.5" height="6.5" fill="#FFB900" />
    </svg>
  )
}
