import { useCallback, useLayoutEffect, useRef, useState, type Ref } from 'react'

type Control = HTMLInputElement | HTMLTextAreaElement

const asignar = <T,>(ref: Ref<T> | undefined, valor: T | null) => {
  if (typeof ref === 'function') ref(valor)
  else if (ref) ref.current = valor
}

/**
 * Largo del valor de un input o área de texto, para el contador (design-system.md § Contador).
 * Sirve con campos no controlados de React Hook Form: escucha `input` y relee el valor después de
 * cada render, así también refleja `reset()` y `setValue()`.
 *
 * Devuelve el largo y un `ref` que se fusiona con el que ya tenga el campo (p. ej. el de `register`).
 */
export function useLargoControl<T extends Control>(contar: (valor: string) => number, refExterno?: Ref<T>) {
  const control = useRef<T | null>(null)
  const [largo, setLargo] = useState(0)
  // Sin dependencias a propósito: el valor puede cambiar por fuera de React (reset de RHF).
  useLayoutEffect(() => {
    const el = control.current
    if (!el) return
    const medir = () => setLargo(contar(el.value))
    medir()
    el.addEventListener('input', medir)
    return () => el.removeEventListener('input', medir)
  })

  const ref = useCallback(
    (el: T | null) => {
      control.current = el
      asignar(refExterno, el)
    },
    [refExterno],
  )

  return { largo, ref }
}
