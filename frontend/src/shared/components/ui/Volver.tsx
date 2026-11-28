import { ArrowLeft } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { pantallaDe } from '@/shared/navegacion'
import { leerEstadoVolver } from './estadoVolver'
import { pantallaAnterior } from './historialVolver'

interface VolverProps {
  /** Pantalla padre: solo se usa si no hay pantalla anterior a la que volver (se entró por URL,
   *  recarga sin historial). Si la pantalla de origen dejó otro destino (`estadoVolver`), se usa ese. */
  a: string
  /** Texto para la pantalla padre: "Volver a {destino}". Si la pantalla padre está en la lista única de pantallas
   *  (shared/navegacion/pantallas.ts), se usa el texto de la lista. Con pantalla anterior se usa el nombre de esa. */
  texto: string
  /** Título de la página, que acompaña al link en la barra fija. */
  titulo?: string
  /** Ancho del contenido de la página, para alinear la barra con él. */
  ancho?: 'narrow' | 'layout'
}

function Enlace({ a, texto, oculto, atras }: { a: string; texto: string; oculto?: boolean; atras?: boolean }) {
  const navigate = useNavigate()
  return (
    <Link
      to={a}
      tabIndex={oculto ? -1 : undefined}
      onClick={(e) => {
        // Como "atrás" del navegador: la pantalla anterior recupera su estado y su posición.
        if (!atras || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey || e.button !== 0) return
        e.preventDefault()
        void navigate(-1)
      }}
      className="inline-flex shrink-0 items-center gap-1 text-sm font-medium">
      <ArrowLeft aria-hidden size={16} strokeWidth={1.5} />
      {texto}
    </Link>
  )
}

/**
 * Regreso a la pantalla anterior (design-system.md § Volver). Va como primer elemento de la página;
 * cuando el link sale de la vista al desplazar, aparece una barra fija arriba con el link y el título.
 * Destino: la pantalla de la que se llegó (como "atrás"); si no la hay, el origen que dejó la pantalla
 * anterior o la pantalla padre.
 */
export function Volver({ a: padre, texto: textoPadre, titulo, ancho = 'narrow' }: VolverProps) {
  const location = useLocation()
  const anterior = pantallaAnterior(location)
  const origen = leerEstadoVolver(location.state)
  // El mismo nombre en todas las pantallas que vuelven a la misma (design-system.md § Volver).
  const textoListado = pantallaDe(padre.split(/[?#]/)[0])?.volver
  const a = anterior?.ruta ?? origen?.a ?? padre
  const texto = anterior?.texto ?? origen?.texto ?? textoListado ?? textoPadre
  const atras = anterior !== undefined
  const ancla = useRef<HTMLDivElement>(null)
  const [fija, setFija] = useState(false)

  useEffect(() => {
    const el = ancla.current
    if (!el) return
    const observador = new IntersectionObserver(([entrada]) => {
      // Solo cuando el link quedó por encima de la vista (no si está más abajo).
      setFija(!entrada.isIntersecting && entrada.boundingClientRect.top < 0)
    })
    observador.observe(el)
    return () => observador.disconnect()
  }, [])

  return (
    <>
      <div ref={ancla}>
        <Enlace a={a} texto={texto} atras={atras} />
      </div>
      <div className="barra-volver" data-visible={fija} aria-hidden={!fija} inert={!fija}>
        <div
          className={`barra-volver-contenido margen-lateral flex h-full w-full items-center gap-2 ${ancho === 'narrow' ? 'max-w-layout-narrow' : 'max-w-layout'}`}
        >
          <Enlace a={a} texto={texto} oculto={!fija} atras={atras} />
          {titulo && (
            <span className="barra-volver-titulo" title={titulo}>
              · {titulo}
            </span>
          )}
        </div>
      </div>
    </>
  )
}
