/** Destino de "Volver" que una pantalla le pasa a otra al enlazarla (design-system.md § Volver). */
export interface DestinoVolver {
  a: string
  texto: string
}

/**
 * Estado para un `<Link state>` que cruza de un módulo a otro: la pantalla de destino muestra
 * "Volver al riesgo" (u otro origen) en lugar de su pantalla padre fija.
 */
export const estadoVolver = (a: string, texto: string) => ({ volver: { a, texto } satisfies DestinoVolver })

/** Lee el destino que dejó la pantalla de origen, si lo hay. */
export const leerEstadoVolver = (estado: unknown): DestinoVolver | undefined => {
  const volver = (estado as { volver?: Partial<DestinoVolver> } | null)?.volver
  return typeof volver?.a === 'string' && typeof volver.texto === 'string' ? { a: volver.a, texto: volver.texto } : undefined
}
