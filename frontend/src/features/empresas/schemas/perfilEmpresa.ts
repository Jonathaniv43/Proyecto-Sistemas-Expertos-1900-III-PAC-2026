import { textoRequerido, z } from '@/shared/validacion'

/**
 * Espejo de ActualizarPerfilEmpresaValidator (backend). El front además pide rubro y número de
 * trabajadores: son los que completan el perfil, y una vez guardados el backend no permite borrarlos.
 * Los textos se dejan como string (no undefined): vacío en dirección o actividad significa "borrar".
 */
export const perfilEmpresaEsquema = z.object({
  razonSocial: textoRequerido(200),
  rubroCodigo: z.string('Elige el rubro de la empresa.').min(1, 'Elige el rubro de la empresa.').max(30),
  trabajadoresPermanentes: z
    .string()
    .trim()
    .min(1, 'Este campo es obligatorio.')
    .regex(/^\d+$/, 'Escribe un número entero, sin puntos ni comas.')
    // Hasta 9 cifras: cabe en el entero del backend.
    .max(9, 'Escribe un número de hasta 9 cifras.')
    .transform(Number),
  direccion: z.string().trim().max(300, 'Debe tener como máximo 300 caracteres.'),
  actividadEconomica: z.string().trim().max(150, 'Debe tener como máximo 150 caracteres.'),
})
