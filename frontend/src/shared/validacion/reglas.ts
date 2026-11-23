import { z } from './zod'

/**
 * Reglas reutilizables, espejo de las de FluentValidation del backend.
 * Si cambia una regla en el backend, se cambia aquí (y al revés): 16-arquitectura §9.
 */

/** NotEmpty() de FluentValidation: tampoco acepta solo espacios. Se envía sin espacios al inicio ni al final. */
export const textoRequerido = (maximo?: number) => {
  const base = z.string().trim().min(1, 'Este campo es obligatorio.')
  return maximo ? base.max(maximo, `Debe tener como máximo ${maximo} caracteres.`) : base
}

/** Texto opcional: vacío se envía como undefined. */
export const textoOpcional = (maximo: number) =>
  z
    .string()
    .trim()
    .max(maximo, `Debe tener como máximo ${maximo} caracteres.`)
    .transform((valor) => (valor === '' ? undefined : valor))

/** Correo: NotEmpty().EmailAddress().MaximumLength(254). */
export const correo = () =>
  z
    .string()
    .trim()
    .min(1, 'Ingresa tu correo.')
    .max(254, 'El correo debe tener como máximo 254 caracteres.')
    .pipe(z.email('Ingresa un correo válido, por ejemplo nombre@empresa.com.'))

/** Contraseña para iniciar sesión: solo se compara con la guardada (LoginEmpresaValidator). */
export const contrasenaIngresada = () =>
  z.string().min(1, 'Ingresa tu contraseña.').min(8, 'La contraseña tiene al menos 8 caracteres.')

// --- Política de contraseñas (HU-114) ---
// Espejo de backend/Norvia.Application/Common/Validacion/PoliticaContrasena.cs.

export const POLITICA_CONTRASENA = { minimo: 8, maximo: 64 } as const

/** Contraseñas comunes que cumplen la composición; misma lista que el backend. */
const CONTRASENAS_COMUNES = new Set(
  [
    'Password1!', 'Password1@', 'Password123!', 'Password@123', 'P@ssw0rd', 'P@ssw0rd1', 'P@ssword1', 'P@ssword123',
    'Passw0rd!', 'Contraseña1!', 'Contrasena1!', 'Contraseña123!', 'Contrasena123!', 'Clave123!', 'Clave@123',
    'Admin123!', 'Admin@123', 'Admin1234!', 'Administrador1!', 'Qwerty123!', 'Qwerty1!', 'Qwerty@123',
    'Welcome1!', 'Welcome123!', 'Bienvenido1!', 'Bienvenido123!', 'Abc123!', 'Abc12345!', 'Abcd1234!', 'Abc@1234',
    'Aa123456!', 'Aa12345678!', 'Test123!', 'Prueba123!', 'Usuario123!', 'User123!', 'Root123!', 'Hola1234!',
    'Honduras1!', 'Honduras123!', 'Tegucigalpa1!', 'Norvia123!', 'Norvia2026!', 'Empresa123!', 'Seguridad1!',
    'Changeme1!', 'Letmein1!', 'Iloveyou1!', 'Teamo123!', 'Summer2026!', 'Verano2026!', 'Enero2026!',
    'Football1!', 'Futbol123!', 'Monkey123!', 'Dragon123!', 'Master123!', 'Sunshine1!', 'Princess1!',
    'Zaq12wsx!', '1Qaz2wsx!', '1q2w3e4r!', '1Q2w3e4r!', 'Q1w2e3r4!', '123456Aa!', '12345678Aa!', 'Temporal123!',
  ].map((c) => c.toLowerCase()),
)

// Equivalentes de char.IsUpper, IsLetterOrDigit e IsWhiteSpace de .NET (Unicode).
const tieneMayuscula = (p: string) => /\p{Lu}/u.test(p)
const tieneNumero = (p: string) => /[0-9]/.test(p)
const tieneEspecial = (p: string) => /[^\p{L}\p{Nd}\s]/u.test(p)
const esComun = (p: string) => CONTRASENAS_COMUNES.has(p.trim().toLowerCase())

/** Requisitos para mostrar como lista de chequeo junto al campo. */
export const REQUISITOS_CONTRASENA = [
  { texto: `Entre ${POLITICA_CONTRASENA.minimo} y ${POLITICA_CONTRASENA.maximo} caracteres`,
    cumple: (p: string) => p.length >= POLITICA_CONTRASENA.minimo && p.length <= POLITICA_CONTRASENA.maximo },
  { texto: 'Una letra mayúscula', cumple: tieneMayuscula },
  { texto: 'Un número', cumple: tieneNumero },
  { texto: 'Un carácter especial (por ejemplo ! @ # $ %)', cumple: tieneEspecial },
] as const

/** Contraseña nueva: CumplePoliticaContrasena() del backend, un mensaje por requisito. */
export const contrasenaNueva = () =>
  z
    .string()
    .min(1, 'Ingresa una contraseña.')
    .min(POLITICA_CONTRASENA.minimo, `La contraseña debe tener entre ${POLITICA_CONTRASENA.minimo} y ${POLITICA_CONTRASENA.maximo} caracteres.`)
    .max(POLITICA_CONTRASENA.maximo, `La contraseña debe tener entre ${POLITICA_CONTRASENA.minimo} y ${POLITICA_CONTRASENA.maximo} caracteres.`)
    .refine(tieneMayuscula, 'La contraseña debe incluir al menos una letra mayúscula.')
    .refine(tieneNumero, 'La contraseña debe incluir al menos un número.')
    .refine(tieneEspecial, 'La contraseña debe incluir al menos un carácter especial (por ejemplo ! @ # $ %).')
    .refine((p) => !esComun(p), 'Esa contraseña es muy común o conocida; elige otra.')
