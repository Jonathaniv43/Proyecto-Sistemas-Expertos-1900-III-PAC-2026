/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** URL base de la API en producción (p. ej. https://norvia-api.onrender.com/api). En desarrollo se usa el proxy /api. */
  readonly VITE_API_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
