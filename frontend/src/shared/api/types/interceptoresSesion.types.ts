/** Callbacks que el feature de autenticación inyecta. shared no conoce el store. */
export interface InterceptoresSesion {
  obtenerToken: () => string | null
  renovar: () => Promise<string | null>
}
