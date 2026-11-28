import { Link } from 'react-router-dom'
import { clasesBoton } from '@/shared/components/ui/clases'

const SECCIONES = [
  { a: '#como-funciona', texto: 'Cómo funciona' },
  { a: '#que-cubre', texto: 'Qué cubre' },
  { a: '#privacidad', texto: 'Privacidad' },
  { a: '#preguntas', texto: 'Preguntas' },
]

/**
 * Barra propia de la landing (design-system.md § Landing): "Norvia", enlaces a las secciones de la página y la entrada
 * a la cuenta. Es la única pantalla pública que no usa la barra de LayoutPublico. En celular quedan "Norvia" y el botón.
 */
export function BarraLanding({ conSesion }: { conSesion: boolean }) {
  return (
    <header className="barra-publica">
      <div className="margen-lateral mx-auto flex h-full w-full max-w-layout items-center justify-between gap-6">
        <Link to="/" className="marca">
          Norvia
        </Link>
        <nav aria-label="Secciones de la página" className="hidden flex-1 gap-6 md:flex">
          {SECCIONES.map((s) => (
            <a key={s.a} href={s.a} className="font-medium text-text-secondary no-underline hover:text-text-primary">
              {s.texto}
            </a>
          ))}
        </nav>
        <Link to={conSesion ? '/inicio' : '/login'} className={clasesBoton('secundario')}>
          {conSesion ? 'Ir a mi inicio' : 'Iniciar sesión'}
        </Link>
      </div>
    </header>
  )
}
