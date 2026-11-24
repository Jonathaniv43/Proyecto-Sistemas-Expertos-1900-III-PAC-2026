import { Check, ChevronDown, Search } from 'lucide-react'
import { useEffect, useId, useMemo, useRef, useState, type KeyboardEvent, type ReactNode } from 'react'
import { clasesControl } from './clases'
import { Etiqueta, MensajeCampo } from './MensajeCampo'

export interface OpcionBusqueda {
  valor: string
  texto: string
  /** Línea secundaria (p. ej. la clasificación del peligro); también se busca en ella. */
  descripcion?: string
}

interface SelectBusquedaProps {
  etiqueta: string
  required?: boolean
  opciones: readonly OpcionBusqueda[]
  valor: string
  onChange: (valor: string) => void
  onBlur?: () => void
  error?: string
  ayuda?: ReactNode
  /** Texto de ayuda dentro del campo de búsqueda, p. ej. "Buscar peligro". */
  etiquetaBusqueda: string
}

// Sin tildes ni mayúsculas: "caida" encuentra "Caída".
const normalizar = (texto: string) => texto.normalize('NFD').replace(/\p{Diacritic}/gu, '').toLowerCase()

/**
 * Select con búsqueda (design-system.md § Select): cerrado es igual que el input; abierto, una lista
 * con un campo de búsqueda arriba (desde ~10 opciones). Patrón de combobox: el foco va al buscador,
 * las flechas recorren las opciones, Enter elige, Esc cierra y el foco vuelve al campo.
 * En celular la lista se abre como hoja inferior.
 */
export function SelectBusqueda({ etiqueta, required, opciones, valor, onChange, onBlur, error, ayuda, etiquetaBusqueda }: SelectBusquedaProps) {
  const id = useId()
  const idLista = `${id}-lista`
  const idMensaje = `${id}-mensaje`
  const [abierto, setAbierto] = useState(false)
  const [busqueda, setBusqueda] = useState('')
  const [activa, setActiva] = useState(0)
  const contenedor = useRef<HTMLDivElement>(null)
  const boton = useRef<HTMLButtonElement>(null)
  const buscador = useRef<HTMLInputElement>(null)
  const lista = useRef<HTMLUListElement>(null)

  const elegida = opciones.find((o) => o.valor === valor)
  const visibles = useMemo(() => {
    const texto = normalizar(busqueda.trim())
    return texto ? opciones.filter((o) => normalizar(`${o.texto} ${o.descripcion ?? ''}`).includes(texto)) : opciones
  }, [opciones, busqueda])

  const abrir = () => {
    setBusqueda('')
    setActiva(Math.max(opciones.findIndex((o) => o.valor === valor), 0))
    setAbierto(true)
  }
  const cerrar = (devolverFoco = true) => {
    setAbierto(false)
    onBlur?.()
    if (devolverFoco) boton.current?.focus()
  }
  const elegir = (opcion: OpcionBusqueda) => {
    onChange(opcion.valor)
    cerrar()
  }

  useEffect(() => {
    if (abierto) buscador.current?.focus()
  }, [abierto])

  // La opción activa queda a la vista al moverse con el teclado.
  useEffect(() => {
    if (abierto) lista.current?.querySelector(`[data-indice="${activa}"]`)?.scrollIntoView({ block: 'nearest' })
  }, [abierto, activa])

  // Clic fuera: cierra sin elegir.
  useEffect(() => {
    if (!abierto) return
    const fuera = (e: PointerEvent) => {
      if (!contenedor.current?.contains(e.target as Node)) cerrar(false)
    }
    document.addEventListener('pointerdown', fuera)
    return () => document.removeEventListener('pointerdown', fuera)
  })

  const teclasBuscador = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setActiva((i) => Math.min(i + 1, visibles.length - 1))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setActiva((i) => Math.max(i - 1, 0))
    } else if (e.key === 'Home') {
      e.preventDefault()
      setActiva(0)
    } else if (e.key === 'End') {
      e.preventDefault()
      setActiva(visibles.length - 1)
    } else if (e.key === 'Enter') {
      e.preventDefault()
      if (visibles[activa]) elegir(visibles[activa])
    } else if (e.key === 'Escape') {
      e.preventDefault()
      e.stopPropagation()
      cerrar()
    } else if (e.key === 'Tab') {
      cerrar(false)
    }
  }

  return (
    <div ref={contenedor} className="relative grid content-start gap-2">
      <Etiqueta htmlFor={id} required={required}>
        {etiqueta}
      </Etiqueta>
      <button
        ref={boton}
        id={id}
        type="button"
        aria-haspopup="listbox"
        aria-expanded={abierto}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || ayuda ? idMensaje : undefined}
        aria-required={required || undefined}
        className={`${clasesControl(Boolean(error))} flex cursor-pointer items-center justify-between gap-2 text-left`}
        onClick={() => (abierto ? cerrar() : abrir())}
        onKeyDown={(e) => {
          if (!abierto && (e.key === 'ArrowDown' || e.key === 'ArrowUp')) {
            e.preventDefault()
            abrir()
          }
        }}
      >
        <span className={`truncate ${elegida ? '' : 'text-text-tertiary'}`}>{elegida?.texto ?? 'Selecciona…'}</span>
        <ChevronDown aria-hidden size={16} strokeWidth={1.5} className="shrink-0 text-text-secondary" />
      </button>
      <MensajeCampo id={idMensaje} error={error} ayuda={ayuda} />

      {abierto && (
        <>
          {/* En celular la lista es una hoja inferior con fondo oscurecido detrás. */}
          <div aria-hidden className="lista-busqueda-fondo" onPointerDown={() => cerrar(false)} />
          <div className="lista-busqueda">
            <div className="relative p-2">
              <Search aria-hidden size={16} strokeWidth={1.5} className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-text-tertiary" />
              <input
                ref={buscador}
                type="text"
                role="combobox"
                aria-label={etiquetaBusqueda}
                aria-expanded
                aria-controls={idLista}
                aria-autocomplete="list"
                aria-activedescendant={visibles[activa] ? `${id}-op-${activa}` : undefined}
                placeholder={etiquetaBusqueda}
                autoComplete="off"
                value={busqueda}
                onChange={(e) => {
                  setBusqueda(e.target.value)
                  setActiva(0)
                }}
                onKeyDown={teclasBuscador}
                className={`${clasesControl(false)} pl-8`}
              />
            </div>
            <ul ref={lista} id={idLista} role="listbox" aria-label={etiqueta} className="lista-busqueda-opciones">
              {visibles.length === 0 ? (
                <li className="px-3 py-3 text-text-secondary">No hay resultados para «{busqueda.trim()}».</li>
              ) : (
                visibles.map((o, i) => {
                  const seleccionada = o.valor === valor
                  return (
                    <li
                      key={o.valor}
                      id={`${id}-op-${i}`}
                      data-indice={i}
                      role="option"
                      aria-selected={seleccionada}
                      className={`flex min-h-control-touch cursor-pointer items-center justify-between gap-2 px-3 py-2 sm:min-h-control ${
                        i === activa ? 'bg-bg-subtle' : ''
                      }`}
                      // pointerdown: elige antes de que el buscador pierda el foco.
                      onPointerDown={(e) => {
                        e.preventDefault()
                        elegir(o)
                      }}
                      onPointerMove={() => setActiva(i)}
                    >
                      <span className="grid">
                        <span className={seleccionada ? 'text-text-selected' : 'text-text-primary'}>{o.texto}</span>
                        {o.descripcion && <span className="text-xs text-text-secondary">{o.descripcion}</span>}
                      </span>
                      {seleccionada && <Check aria-hidden size={16} strokeWidth={2} className="shrink-0 text-text-selected" />}
                    </li>
                  )
                })
              )}
            </ul>
          </div>
        </>
      )}
    </div>
  )
}
