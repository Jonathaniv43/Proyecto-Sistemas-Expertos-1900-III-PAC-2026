import type { ReactNode } from 'react'

/** Orden de las filas del inicio: primero lo que falló al cargar, después lo vencido, lo que necesita atención y lo demás. */
export const PESO = { error: 0, vencido: 1, atencion: 2, normal: 3, aviso: 4 } as const

export interface Fila {
  clave: string
  peso: number
  nodo: ReactNode
}

export const ordenar = (filas: Fila[]) => [...filas].sort((a, b) => a.peso - b.peso)

/** "1 tarea vencida" / "2 tareas vencidas". */
export const cantidad = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`
