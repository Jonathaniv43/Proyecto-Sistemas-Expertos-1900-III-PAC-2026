import { Link } from 'react-router-dom'
import { clasesBoton } from '@/shared/components/ui/clases'

const SECCIONES = [
  { a: '#como-funciona', texto: 'Cómo funciona' },
  { a: '#que-cubre', texto: 'Qué cubre' },
  { a: '#privacidad', texto: 'Privacidad' },
  { a: '#preguntas', texto: 'Preguntas' },
]

/**
 * Barra propia de la landing (design-system.md § Landing): "Norvia", enlaces a las secciones de la página, "Iniciar
 * sesión" y "Registrar mi empresa" (la acción principal queda a la vista). Con sesión iniciada, solo "Ir a mi inicio".
 * En celular quedan "Norvia" y "Iniciar sesión". Es la única pantalla pública que no usa la barra de LayoutPublico.
 */
export function BarraLanding({ conSesion }: { conSesion: boolean }) {
  return (
    <header className="barra-publica">
      <div className="margen-lateral mx-auto flex h-full w-full max-w-layout items-center justify-between gap-8">
        <Link to="/" className="marca text-lg">
          Norvia
        </Link>
        <nav aria-label="Secciones de la página" className="hidden flex-1 gap-6 lg:flex">
          {SECCIONES.map((s) => (
            <a key={s.a} href={s.a} className="font-medium text-text-secondary no-underline hover:text-text-primary">
              {s.texto}
            </a>
          ))}
        </nav>
        {conSesion ? (
          <Link to="/inicio" className={clasesBoton('secundario')}>
            Ir a mi inicio
          </Link>
        ) : (
          <div className="flex items-center gap-2">
            <Link to="/login" className={clasesBoton('secundario')}>
              Iniciar sesión
            </Link>
            <span className="hidden sm:block">
              <Link to="/registro" className={clasesBoton('primario')}>
                Registrar mi empresa
              </Link>
            </span>
          </div>
        )}
      </div>
    </header>
  )
}
