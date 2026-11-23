import { contrasenaNueva } from './reglas'

const MAYUSCULAS = 'ABCDEFGHJKLMNPQRSTUVWXYZ'
const MINUSCULAS = 'abcdefghijkmnopqrstuvwxyz'
const DIGITOS = '23456789'
const ESPECIALES = '!@#$%&*?'
const TODOS = MAYUSCULAS + MINUSCULAS + DIGITOS + ESPECIALES

const azar = (maximo: number) => {
  // Rechazo de valores sesgados: cada carácter tiene la misma probabilidad.
  const limite = Math.floor(0x1_0000_0000 / maximo) * maximo
  const valor = new Uint32Array(1)
  do crypto.getRandomValues(valor)
  while (valor[0] >= limite)
  return valor[0] % maximo
}

const elegir = (de: string) => de[azar(de.length)]

/**
 * Contraseña temporal aleatoria que cumple la política de HU-114 (se cambia en el primer ingreso).
 * Sin caracteres que se confunden al dictarla o copiarla (0/O, 1/l/I).
 */
export function generarContrasenaTemporal(largo = 14): string {
  for (;;) {
    const caracteres = [elegir(MAYUSCULAS), elegir(MINUSCULAS), elegir(DIGITOS), elegir(ESPECIALES)]
    while (caracteres.length < largo) caracteres.push(elegir(TODOS))
    for (let i = caracteres.length - 1; i > 0; i--) {
      const j = azar(i + 1)
      ;[caracteres[i], caracteres[j]] = [caracteres[j], caracteres[i]]
    }
    const contrasena = caracteres.join('')
    if (contrasenaNueva().safeParse(contrasena).success) return contrasena
  }
}
