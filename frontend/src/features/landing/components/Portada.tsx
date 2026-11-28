import { ArrowDown } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { UsuarioSesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Insignia } from '@/shared/components/ui/Insignia'
import { clasesBoton } from '@/shared/components/ui/clases'
import { EtiquetaRiesgo } from '@/shared/riesgo/EtiquetaRiesgo'
import { CLASES_NIVEL, clasificacionDe, ETIQUETAS_CLASIFICACION_RIESGO, type ClasificacionRiesgo } from '@/shared/riesgo/niveles'
import { etiquetaRol } from '@/shared/sesion'

/** Dónde se entró, para el aviso de sesión iniciada. */
const lugarDe = (usuario: UsuarioSesion) =>
  usuario.tipo === 'plataforma' ? 'la administración de Norvia' : (usuario.empresa?.razonSocial ?? 'tu empresa')

/**
 * Portada de la landing (design-system.md § Landing): qué gana la empresa y la acción principal. Con sesión
 * iniciada, en lugar de registrar, dice dónde se entró y ofrece ir al inicio.
 */
export function Portada({ usuario }: { usuario: UsuarioSesion | null }) {
  return (
    <section id="inicio" className="py-12 sm:py-16">
      <div className="margen-lateral mx-auto grid w-full max-w-layout items-center gap-12 lg:grid-cols-2">
        <div className="grid gap-6">
          <h1 className="text-xl font-semibold tracking-tight text-balance sm:text-2xl">
            Conoce los riesgos de tu empresa, contrólalos y demuestra que cumples
          </h1>
          <p className="text-md text-text-secondary">
            Norvia guía paso a paso a las empresas hondureñas en la seguridad y salud en el trabajo.
          </p>
          {usuario ? (
            <>
              <Aviso tipo="info">
                Ya entraste a {lugarDe(usuario)} como {etiquetaRol(usuario.rol)}.
              </Aviso>
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                <Link to="/inicio" className={clasesBoton('primario')}>
                  Ir a mi inicio
                </Link>
              </div>
              <p className="text-text-secondary">Para entrar a otra empresa, cierra la sesión desde el menú de tu cuenta.</p>
            </>
          ) : (
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
              <Link to="/registro" className={clasesBoton('primario')}>
                Registrar mi empresa
              </Link>
              <a href="#como-funciona" className="inline-flex min-h-control-touch items-center justify-center gap-1 font-medium no-underline sm:min-h-control">
                Ver cómo funciona
                <ArrowDown aria-hidden size={16} strokeWidth={1.5} />
              </a>
            </div>
          )}
        </div>
        <VistaProducto />
      </div>
    </section>
  )
}

/** Cantidad de riesgos por celda "probabilidad-severidad": una muestra, no lee datos. */
const MUESTRA: Record<string, number> = { '4-5': 1, '5-4': 1, '3-4': 1, '3-3': 1, '4-2': 1, '2-4': 1 }
const NIVELES: ClasificacionRiesgo[] = ['bajo', 'medio', 'alto', 'critico']
const RANGOS: Record<ClasificacionRiesgo, string> = { bajo: '1–4', medio: '5–9', alto: '10–14', critico: '15–25' }

/**
 * Vista del producto armada con piezas reales del design system (mapa de calor y una tarea del plan), en lugar de una
 * foto de archivo. Es decorativa: el texto la resume para lectores de pantalla.
 */
function VistaProducto() {
  return (
    <div className="landing-vista" role="img" aria-label="Ejemplo de Norvia: el mapa de calor de una matriz de riesgos y una tarea del plan">
      <div aria-hidden className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-6 shadow-2">
        <div className="flex items-baseline justify-between gap-2">
          <span className="text-md font-semibold">Matriz de riesgos</span>
          <span className="text-xs text-text-secondary">7 riesgos · 1 sin evaluar</span>
        </div>
        <div className="landing-mapa">
          {[5, 4, 3, 2, 1].map((p) => (
            <Fila key={p} probabilidad={p} />
          ))}
          <span />
          {[1, 2, 3, 4, 5].map((s) => (
            <span key={s} className="text-center text-xs text-text-secondary">
              {s}
            </span>
          ))}
        </div>
        <div className="flex flex-wrap gap-x-4 gap-y-2">
          {NIVELES.map((n) => (
            <span key={n} className="inline-flex items-center gap-1 text-xs text-text-secondary">
              <span className={`cuadro-riesgo ${CLASES_NIVEL[n]}`} />
              {ETIQUETAS_CLASIFICACION_RIESGO[n]} {RANGOS[n]}
            </span>
          ))}
        </div>
      </div>
      <div aria-hidden className="landing-vista-tarea grid gap-3 rounded-lg border border-border-default bg-bg-surface p-6 shadow-2">
        <span className="font-medium">Charla de cinco minutos al inicio de cada jornada</span>
        <span className="flex flex-wrap gap-2">
          <EtiquetaRiesgo nivel={{ nivel: 20, clasificacion: 'critico' }} />
          <Insignia tipo="info">En progreso</Insignia>
        </span>
        <span className="text-xs text-text-secondary">Carla Colaboradora · Hasta el 16 dic 2026</span>
      </div>
    </div>
  )
}

function Fila({ probabilidad }: { probabilidad: number }) {
  return (
    <>
      <span className="self-center text-xs text-text-secondary">{probabilidad}</span>
      {[1, 2, 3, 4, 5].map((s) => (
        <span
          key={s}
          className={`grid aspect-square place-items-center rounded-sm text-xs font-semibold tabular-nums ${CLASES_NIVEL[clasificacionDe(probabilidad * s)]}`}
        >
          {MUESTRA[`${probabilidad}-${s}`] ?? ''}
        </span>
      ))}
    </>
  )
}
