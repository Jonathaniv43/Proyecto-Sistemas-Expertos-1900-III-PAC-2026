import { Calendar, ChevronLeft, ChevronRight } from 'lucide-react'
import { useEffect, useId, useLayoutEffect, useRef, useState, type CSSProperties, type KeyboardEvent, type ReactNode } from 'react'
import { conBarras, hoyHonduras, isoATexto, textoAIso } from '@/shared/validacion'
import { clasesControl } from './clases'
import { Etiqueta, MensajeCampo } from './MensajeCampo'

interface CampoFechaProps {
  etiqueta: string
  required?: boolean
  /** Lo escrito, en dd/mm/aaaa (puede estar incompleto mientras se escribe). */
  valor: string
  onChange: (valor: string) => void
  onBlur?: () => void
  error?: string
  ayuda?: ReactNode
  /** Primer día que se puede elegir en el calendario (aaaa-mm-dd); los anteriores se ven deshabilitados. */
  minimo?: string
  /** Último día que se puede elegir (aaaa-mm-dd), p. ej. hoy para un accidente; los posteriores se ven deshabilitados. */
  maximo?: string
}

const SEMANA = [
  { corto: 'D', largo: 'domingo' },
  { corto: 'L', largo: 'lunes' },
  { corto: 'M', largo: 'martes' },
  { corto: 'M', largo: 'miércoles' },
  { corto: 'J', largo: 'jueves' },
  { corto: 'V', largo: 'viernes' },
  { corto: 'S', largo: 'sábado' },
]

const dos = (n: number) => String(n).padStart(2, '0')
const iso = (d: Date) => `${d.getUTCFullYear()}-${dos(d.getUTCMonth() + 1)}-${dos(d.getUTCDate())}`
const desdeIso = (texto: string) => new Date(`${texto}T00:00:00Z`)
const sumarDias = (texto: string, dias: number) => {
  const d = desdeIso(texto)
  d.setUTCDate(d.getUTCDate() + dias)
  return iso(d)
}
const sumarMeses = (texto: string, meses: number) => {
  const d = desdeIso(texto)
  const dia = d.getUTCDate()
  d.setUTCDate(1)
  d.setUTCMonth(d.getUTCMonth() + meses)
  const ultimo = new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth() + 1, 0)).getUTCDate()
  d.setUTCDate(Math.min(dia, ultimo))
  return iso(d)
}
const nombreDia = (texto: string) =>
  desdeIso(texto).toLocaleDateString('es-HN', { timeZone: 'UTC', weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })
const nombreMes = (texto: string) => desdeIso(texto).toLocaleDateString('es-HN', { timeZone: 'UTC', month: 'long', year: 'numeric' })

/** Los días del mes de `texto`, en semanas de domingo a sábado; null en los huecos. */
function semanasDelMes(texto: string) {
  const d = desdeIso(texto)
  const primero = new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), 1))
  const dias = new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth() + 1, 0)).getUTCDate()
  const celdas: (string | null)[] = Array(primero.getUTCDay()).fill(null)
  for (let i = 0; i < dias; i++) celdas.push(sumarDias(iso(primero), i))
  while (celdas.length % 7) celdas.push(null)
  return Array.from({ length: celdas.length / 7 }, (_, i) => celdas.slice(i * 7, i * 7 + 7))
}

/**
 * Campo de fecha (design-system.md § Campo de fecha): se escribe en dd/mm/aaaa (las barras se ponen solas) o se
 * elige en el calendario, que empieza el domingo, marca hoy con un borde y deshabilita los días fuera de `minimo`–`maximo`.
 * Teclado en el calendario: flechas (día y semana), Inicio/Fin (semana), RePág/AvPág (mes), Enter elige y Esc
 * cierra devolviendo el foco al botón. En celular el calendario se abre como hoja inferior.
 */
export function CampoFecha({ etiqueta, required, valor, onChange, onBlur, error, ayuda, minimo, maximo }: CampoFechaProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  const idTitulo = `${id}-mes`
  const [abierto, setAbierto] = useState(false)
  const [enfocado, setEnfocado] = useState(hoyHonduras)
  const contenedor = useRef<HTMLDivElement>(null)
  const boton = useRef<HTMLButtonElement>(null)
  const rejilla = useRef<HTMLTableElement>(null)
  const caja = useRef<HTMLDivElement>(null)
  const panel = useRef<HTMLDivElement>(null)
  const [posicion, setPosicion] = useState<CSSProperties>()

  const elegido = textoAIso(valor)
  const hoy = hoyHonduras()
  const deshabilitado = (dia: string) => (minimo !== undefined && dia < minimo) || (maximo !== undefined && dia > maximo)

  const abrir = () => {
    setEnfocado(elegido ?? (minimo && minimo > hoy ? minimo : maximo && maximo < hoy ? maximo : hoy))
    setAbierto(true)
  }
  const cerrar = (devolverFoco = true) => {
    setAbierto(false)
    onBlur?.()
    if (devolverFoco) boton.current?.focus()
  }
  const elegir = (dia: string) => {
    if (deshabilitado(dia)) return
    onChange(isoATexto(dia))
    cerrar()
  }

  // Posición fija junto al campo: así no lo recorta un contenedor con desplazamiento (p. ej. el cuerpo de un modal).
  // Abre debajo y, si no cabe, arriba. En celular el CSS lo vuelve hoja inferior y estas coordenadas no se usan.
  useLayoutEffect(() => {
    if (!abierto) return
    const colocar = () => {
      const campo = caja.current?.getBoundingClientRect()
      const alto = panel.current?.offsetHeight ?? 0
      if (!campo) return
      const abajo = campo.bottom + 4
      const top = abajo + alto > window.innerHeight && campo.top - alto - 4 > 0 ? campo.top - alto - 4 : abajo
      setPosicion({ '--calendario-top': `${top}px`, '--calendario-left': `${campo.left}px` } as CSSProperties)
    }
    colocar()
    window.addEventListener('resize', colocar)
    window.addEventListener('scroll', colocar, true)
    return () => {
      window.removeEventListener('resize', colocar)
      window.removeEventListener('scroll', colocar, true)
    }
  }, [abierto])

  // El día enfocado recibe el foco al abrir y al moverse con el teclado.
  useEffect(() => {
    if (abierto) rejilla.current?.querySelector<HTMLButtonElement>(`[data-dia="${enfocado}"]`)?.focus()
  }, [abierto, enfocado])

  // Clic fuera: cierra sin elegir.
  useEffect(() => {
    if (!abierto) return
    const fuera = (e: PointerEvent) => {
      if (!contenedor.current?.contains(e.target as Node)) cerrar(false)
    }
    document.addEventListener('pointerdown', fuera)
    return () => document.removeEventListener('pointerdown', fuera)
  })

  const teclas = (e: KeyboardEvent<HTMLDivElement>) => {
    const dia = desdeIso(enfocado).getUTCDay()
    const mover: Record<string, () => string> = {
      ArrowLeft: () => sumarDias(enfocado, -1),
      ArrowRight: () => sumarDias(enfocado, 1),
      ArrowUp: () => sumarDias(enfocado, -7),
      ArrowDown: () => sumarDias(enfocado, 7),
      Home: () => sumarDias(enfocado, -dia),
      End: () => sumarDias(enfocado, 6 - dia),
      PageUp: () => sumarMeses(enfocado, -1),
      PageDown: () => sumarMeses(enfocado, 1),
    }
    if (mover[e.key]) {
      e.preventDefault()
      setEnfocado(mover[e.key]())
    } else if (e.key === 'Escape') {
      e.preventDefault()
      e.stopPropagation() // no cierra el modal que lo contiene
      cerrar()
    }
  }

  return (
    <div ref={contenedor} className="relative grid content-start gap-2">
      <Etiqueta htmlFor={id} required={required}>
        {etiqueta}
      </Etiqueta>
      <div ref={caja} className="relative">
        <input
          id={id}
          type="text"
          inputMode="numeric"
          autoComplete="off"
          placeholder="dd/mm/aaaa"
          maxLength={10}
          value={valor}
          aria-invalid={error ? true : undefined}
          aria-describedby={error || ayuda ? idMensaje : undefined}
          aria-required={required || undefined}
          className={`${clasesControl(Boolean(error))} pr-10 tabular-nums`}
          onChange={(e) => onChange(conBarras(e.target.value))}
          onBlur={() => !abierto && onBlur?.()}
        />
        <button
          ref={boton}
          type="button"
          aria-label={`Elegir ${etiqueta.toLowerCase()} en el calendario`}
          aria-haspopup="dialog"
          aria-expanded={abierto}
          className="absolute top-1/2 right-1 inline-flex -translate-y-1/2 rounded-sm p-2 text-text-secondary hover:bg-bg-subtle"
          onClick={() => (abierto ? cerrar() : abrir())}
        >
          <Calendar aria-hidden size={16} strokeWidth={1.5} />
        </button>
      </div>
      <MensajeCampo id={idMensaje} error={error} ayuda={ayuda} />

      {abierto && (
        <>
          <div aria-hidden className="calendario-fondo" onPointerDown={() => cerrar(false)} />
          <div
            ref={panel}
            role="dialog"
            aria-modal="false"
            aria-labelledby={idTitulo}
            className="calendario"
            style={posicion}
            onKeyDown={teclas}
          >
            <div className="flex items-center justify-between gap-2">
              <button
                type="button"
                aria-label="Mes anterior"
                className="calendario-nav"
                onClick={() => setEnfocado(sumarMeses(enfocado, -1))}
              >
                <ChevronLeft aria-hidden size={16} strokeWidth={1.5} />
              </button>
              <p id={idTitulo} aria-live="polite" className="text-sm font-medium first-letter:uppercase">
                {nombreMes(enfocado)}
              </p>
              <button
                type="button"
                aria-label="Mes siguiente"
                className="calendario-nav"
                onClick={() => setEnfocado(sumarMeses(enfocado, 1))}
              >
                <ChevronRight aria-hidden size={16} strokeWidth={1.5} />
              </button>
            </div>
            <table ref={rejilla} role="grid" aria-labelledby={idTitulo} className="calendario-rejilla">
              <thead>
                <tr>
                  {SEMANA.map((d) => (
                    <th key={d.largo} scope="col" abbr={d.largo} className="text-xs font-normal text-text-secondary">
                      {d.corto}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {semanasDelMes(enfocado).map((semana, i) => (
                  <tr key={i}>
                    {semana.map((dia, j) => (
                      <td key={dia ?? `v${j}`} role={dia ? 'gridcell' : undefined}>
                        {dia && (
                          <button
                            type="button"
                            data-dia={dia}
                            tabIndex={dia === enfocado ? 0 : -1}
                            disabled={deshabilitado(dia)}
                            aria-label={nombreDia(dia)}
                            aria-pressed={dia === elegido}
                            aria-current={dia === hoy ? 'date' : undefined}
                            className="calendario-dia"
                            onClick={() => elegir(dia)}
                          >
                            {Number(dia.slice(8))}
                          </button>
                        )}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}
