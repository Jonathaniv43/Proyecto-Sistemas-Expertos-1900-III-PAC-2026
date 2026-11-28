import { NavLink, Outlet } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { usePuedeEditar } from '../permisos'

const SECCIONES = [
  { a: '/catalogos/categorias', texto: 'Categorías' },
  { a: '/catalogos/preguntas', texto: 'Preguntas' },
  { a: '/catalogos/peligros', texto: 'Peligros y plantillas' },
  { a: '/catalogos/rubros', texto: 'Rubros' },
  { a: '/catalogos/documentos', texto: 'Plantillas de documento' },
]

/**
 * HU-023: área de catálogos del staff de Norvia. Navegación de sección (design-system.md § Navegación de sección) y,
 * para soporte de plataforma, aviso de solo consulta: solo el administrador de plataforma crea, edita o desactiva.
 */
export function CatalogosLayout() {
  const puedeEditar = usePuedeEditar()
  return (
    <div className="grid w-full max-w-layout grid-cols-1 gap-6">
      <header className="grid gap-1">
        <h1 className="text-xl font-semibold">Catálogos</h1>
        <p className="text-text-secondary">
          El contenido común de todas las empresas: lo que se pregunta en las encuestas, los peligros que se sugieren y sus
          controles y tareas.
        </p>
      </header>
      <nav aria-label="Catálogos" className="nav-seccion">
        {SECCIONES.map((s) => (
          <NavLink key={s.a} to={s.a}>
            {s.texto}
          </NavLink>
        ))}
      </nav>
      {!puedeEditar && <Aviso tipo="info">Tienes acceso de consulta: solo el administrador de plataforma hace cambios.</Aviso>}
      <Outlet />
    </div>
  )
}
