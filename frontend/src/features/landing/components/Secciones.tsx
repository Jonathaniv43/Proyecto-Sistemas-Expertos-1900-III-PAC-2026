import {
  Award,
  Building2,
  ChevronDown,
  CircleCheck,
  EyeOff,
  FolderX,
  HardHat,
  Lock,
  Repeat,
  Scale,
  ShieldCheck,
  UserX,
  type LucideIcon,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { clasesBoton } from '@/shared/components/ui/clases'

/** Secciones de la landing (design-system.md § Landing). Textos aprobados (versión 3) y composición de la versión 4. */

function Seccion({ id, superficie = false, children }: { id?: string; superficie?: boolean; children: ReactNode }) {
  return (
    <section id={id} className={`py-12 lg:py-16 ${superficie ? 'border-y border-border-default bg-bg-surface' : ''}`}>
      <div className="margen-lateral mx-auto w-full max-w-layout">{children}</div>
    </section>
  )
}

/** Título de sección centrado, en Título 1. */
function Titulo({ titulo, texto }: { titulo: string; texto?: string }) {
  return (
    <div className="mx-auto mb-8 grid max-w-layout-narrow gap-3 text-center lg:mb-12">
      <h2 className="text-xl font-semibold tracking-tight text-balance">{titulo}</h2>
      {texto && <p className="text-md text-text-secondary">{texto}</p>}
    </div>
  )
}

/** Ícono de una tarjeta o de la franja de respaldo, en un cuadro neutro. */
function Icono({ Icono }: { Icono: LucideIcon }) {
  return (
    <span aria-hidden className="landing-icono">
      <Icono size={20} strokeWidth={1.5} />
    </span>
  )
}

/** Marca de verificación alineada con la primera línea del texto (como el ícono de Aviso). */
function Check({ size = 16 }: { size?: 16 | 20 }) {
  return (
    <span aria-hidden className="flex h-lh shrink-0 items-center text-success-icon">
      <CircleCheck size={size} strokeWidth={1.5} />
    </span>
  )
}

const RESPALDO: [LucideIcon, string, string][] = [
  [Scale, 'Acuerdo STSS-053-04', 'Reglamento hondureño'],
  [Award, 'ISO 45001', 'Prepara a tu empresa'],
  [Lock, 'Respuestas anónimas', 'Sin nombre de quién responde'],
  [Building2, 'Cada empresa ve lo suyo', 'Datos separados'],
]

/** Franja de respaldo bajo la portada: lo que un cliente busca antes de confiar. */
export function Respaldo() {
  return (
    <div className="border-b border-border-default">
      <ul className="margen-lateral mx-auto grid w-full max-w-layout gap-4 py-8 sm:grid-cols-2 lg:grid-cols-4 lg:gap-6">
        {RESPALDO.map(([I, titulo, texto]) => (
          <li key={titulo} className="flex items-center gap-3">
            <Icono Icono={I} />
            <span className="grid">
              <span className="font-medium">{titulo}</span>
              <span className="text-text-secondary">{texto}</span>
            </span>
          </li>
        ))}
      </ul>
    </div>
  )
}

const PROBLEMAS: [LucideIcon, string, string][] = [
  [EyeOff, 'Riesgos que nadie ve', 'Sin preguntar a quienes hacen el trabajo, la empresa no sabe a qué están expuestos ni qué es más urgente.'],
  [UserX, 'Tareas sin dueño', 'Las acciones se acuerdan en una reunión y se olvidan: no tienen responsable ni fecha límite.'],
  [FolderX, 'Evidencia dispersa', 'Cuando llega una inspección o un cliente pregunta, juntar la matriz, los accidentes y las políticas toma días.'],
]

const tarjeta = 'grid content-start gap-3 rounded-lg border border-border-default bg-bg-surface p-6'

export function Problema() {
  return (
    <Seccion superficie>
      <Titulo
        titulo="La seguridad de tu empresa no cabe en un archivador"
        texto="La ley exige identificar y controlar los riesgos, registrar los accidentes y tener un plan. Con plantillas de Word, correos y carpetas, eso se pierde."
      />
      <div className="grid gap-6 md:grid-cols-3">
        {PROBLEMAS.map(([I, titulo, texto]) => (
          <div key={titulo} className={tarjeta}>
            <Icono Icono={I} />
            <h3 className="text-md font-semibold">{titulo}</h3>
            <p className="text-text-secondary">{texto}</p>
          </div>
        ))}
      </div>
    </Seccion>
  )
}

const PASOS: [string, string][] = [
  ['Registra tu empresa', 'Con su RTN y su rubro, y agrega a tu equipo.'],
  ['Pregunta a tus trabajadores', 'Una encuesta anónima que responden desde el teléfono.'],
  ['Arma tu matriz de riesgos', 'Norvia sugiere los peligros y los controles.'],
  ['Convierte los controles en un plan', 'Tareas con responsable y fecha límite.'],
  ['Aprende de lo que pasa', 'Reporta e investiga accidentes e incidentes.'],
]

/** Pasos conectados por una línea: horizontal en escritorio, vertical en celular y tablet. */
export function ComoFunciona() {
  return (
    <Seccion id="como-funciona">
      <Titulo titulo="Del primer registro a un plan en marcha" />
      <ol className="landing-pasos">
        {PASOS.map(([titulo, texto], i) => (
          <li key={titulo} className="landing-paso">
            <span aria-hidden className="landing-paso-numero">
              {i + 1}
            </span>
            <h3 className="text-md font-semibold">{titulo}</h3>
            <p className="text-text-secondary">{texto}</p>
          </li>
        ))}
      </ol>
      <p className="mt-12 flex items-center justify-center gap-2 text-center text-text-secondary">
        <Repeat aria-hidden size={16} strokeWidth={1.5} className="shrink-0" />
        El ciclo se repite: una nueva encuesta o un accidente actualizan la matriz y el plan.
      </p>
    </Seccion>
  )
}

export function QueCubre() {
  return (
    <Seccion id="que-cubre" superficie>
      <div className="grid items-center gap-8 lg:grid-cols-2 lg:gap-12">
        <div className="grid gap-4">
          <h2 className="text-xl font-semibold tracking-tight">Pensado para el reglamento hondureño</h2>
          <p className="text-md text-text-secondary">
            Norvia te ayuda a cumplir lo que exige el acuerdo STSS-053-04: identificar y controlar los riesgos, tener un
            plan para corregirlos e investigar y registrar los accidentes.
          </p>
          <p className="text-xs text-text-secondary">
            También prepara a tu empresa para la norma ISO 45001. La certificación la otorga un organismo externo.
          </p>
        </div>
        <ul className="grid gap-4 rounded-lg border border-border-default bg-bg-canvas p-6">
          <li className="flex items-center gap-4">
            <Icono Icono={Scale} />
            <span className="grid">
              <span className="font-semibold">Acuerdo STSS-053-04</span>
              <span className="text-text-secondary">Reglamento General de Medidas Preventivas</span>
            </span>
          </li>
          <li className="flex items-center gap-4 border-t border-border-default pt-4">
            <Icono Icono={Award} />
            <span className="grid">
              <span className="font-semibold">ISO 45001</span>
              <span className="text-text-secondary">Prepara la evidencia; la certificación es externa</span>
            </span>
          </li>
        </ul>
      </div>
    </Seccion>
  )
}

const ROLES: [LucideIcon, string, string[]][] = [
  [ShieldCheck, 'Responsable de SST', ['Crea y publica las encuestas', 'Evalúa los riesgos y elige los controles', 'Arma el plan e investiga los eventos', 'Genera y aprueba las políticas']],
  [Building2, 'Administración', ['Registra la empresa y su perfil', 'Da acceso a las personas y define su rol', 'Sigue el plan y la matriz', 'Consulta el registro de acciones']],
  [HardHat, 'Trabajadores', ['Responden las encuestas desde el teléfono', 'Reportan un evento sin dar su nombre', 'Atienden las tareas que se les asignan', 'Consultan las políticas vigentes']],
]

export function ParaCadaPersona() {
  return (
    <Seccion>
      <Titulo titulo="Cada quien ve lo que le toca" />
      <div className="grid gap-6 md:grid-cols-3">
        {ROLES.map(([I, titulo, items]) => (
          <div key={titulo} className={tarjeta}>
            <Icono Icono={I} />
            <h3 className="text-md font-semibold">{titulo}</h3>
            <ul className="grid gap-2">
              {items.map((t) => (
                <li key={t} className="flex gap-2 text-text-secondary">
                  <Check />
                  {t}
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </Seccion>
  )
}

export function Privacidad() {
  return (
    <Seccion id="privacidad" superficie>
      <div className="grid items-center gap-8 lg:grid-cols-2 lg:gap-12">
        <div className="grid gap-3">
          <h2 className="text-xl font-semibold tracking-tight">Respuestas honestas porque son anónimas</h2>
          <p className="text-md text-text-secondary">
            Un trabajador solo cuenta lo que pasa si sabe que no lo van a señalar. Norvia separa quién participó de lo que
            respondió.
          </p>
        </div>
        <ul className="grid gap-3">
          {['Las respuestas no llevan nombre.', 'Reportes de eventos anónimos.', 'Cada empresa ve solo lo suyo.'].map((t) => (
            <li key={t} className="flex items-center gap-3 rounded-lg border border-border-default bg-bg-surface p-4 font-medium">
              <Check size={20} />
              {t}
            </li>
          ))}
        </ul>
      </div>
    </Seccion>
  )
}

const PREGUNTAS: [string, string][] = [
  ['¿Norvia certifica a mi empresa en ISO 45001?', 'No. Norvia organiza el trabajo y la evidencia que pide la norma, pero la certificación la otorga un organismo externo.'],
  ['¿Reemplaza a un profesional de seguridad y salud?', 'No. Norvia sugiere peligros, controles y documentos, y las personas de la empresa los revisan y validan. Los documentos generados deben adaptarse a la realidad de cada empresa.'],
  ['¿Mis trabajadores tienen que instalar algo?', 'No. Responden desde el navegador del teléfono o del computador. Cada persona necesita un correo propio para activar su cuenta y recibir los avisos.'],
  ['¿Para qué empresas es?', 'Para empresas hondureñas de 10 o más trabajadores de cuatro rubros: construcción, maquila o fábrica, comercio o tienda y oficina administrativa.'],
  ['Trabajo en varias empresas. ¿Necesito varias cuentas?', 'No. Usas el mismo correo y entras a cada empresa por separado, buscándola por su RTN.'],
]

/** Preguntas frecuentes: columna angosta centrada, separadas por líneas. */
export function Preguntas() {
  return (
    <Seccion id="preguntas">
      <Titulo titulo="Preguntas frecuentes" />
      <div className="mx-auto w-full max-w-layout-narrow border-t border-border-default">
        {PREGUNTAS.map(([pregunta, respuesta], i) => (
          <details key={pregunta} open={i === 0} className="group border-b border-border-default">
            <summary className="landing-pregunta flex min-h-control-touch cursor-pointer list-none items-center justify-between gap-4 py-4 font-medium">
              {pregunta}
              <ChevronDown aria-hidden size={16} strokeWidth={1.5} className="shrink-0 transition-transform group-open:rotate-180 motion-reduce:transition-none" />
            </summary>
            <p className="pb-4 text-text-secondary">{respuesta}</p>
          </details>
        ))}
      </div>
    </Seccion>
  )
}

/** Cierre enmarcado: la acción principal para quien llegó hasta el final. */
export function Cierre({ conSesion }: { conSesion: boolean }) {
  return (
    <section className="pb-12 lg:pb-16">
      <div className="margen-lateral mx-auto w-full max-w-layout">
        <div className="grid justify-items-center gap-6 rounded-xl border border-border-default bg-bg-surface px-4 py-12 text-center shadow-1 sm:px-8">
          <h2 className="text-xl font-semibold tracking-tight lg:text-2xl">Empieza por conocer tus riesgos</h2>
          <p className="max-w-layout-narrow text-md text-text-secondary">Registra tu empresa, agrega a tu equipo y publica tu primera encuesta.</p>
          {conSesion ? (
            <Link to="/inicio" className={clasesBoton('primario')}>
              Ir a mi inicio
            </Link>
          ) : (
            <div className="flex w-full flex-col justify-center gap-3 sm:w-auto sm:flex-row">
              <Link to="/registro" className={clasesBoton('primario')}>
                Registrar mi empresa
              </Link>
              <Link to="/login" className={clasesBoton('secundario')}>
                Iniciar sesión
              </Link>
            </div>
          )}
        </div>
      </div>
    </section>
  )
}

/** Pie con columnas: la marca, las secciones de la página y la cuenta. */
export function Pie({ conSesion }: { conSesion: boolean }) {
  const enlace = 'text-text-secondary no-underline hover:text-text-primary'
  return (
    <footer className="border-t border-border-default bg-bg-surface">
      <div className="margen-lateral mx-auto grid w-full max-w-layout gap-8 pt-12 pb-8 sm:grid-cols-2 lg:grid-cols-4">
        <div className="grid content-start gap-2 lg:col-span-2">
          <Link to="/" className="marca text-lg">
            Norvia
          </Link>
          <p className="text-xs text-text-secondary">Seguridad y salud en el trabajo para empresas hondureñas.</p>
        </div>
        <nav aria-label="Producto" className="grid content-start gap-3">
          <h2 className="font-semibold">Producto</h2>
          <ul className="grid gap-2">
            <li>
              <a href="#como-funciona" className={enlace}>Cómo funciona</a>
            </li>
            <li>
              <a href="#que-cubre" className={enlace}>Qué cubre</a>
            </li>
            <li>
              <a href="#privacidad" className={enlace}>Privacidad</a>
            </li>
            <li>
              <a href="#preguntas" className={enlace}>Preguntas</a>
            </li>
          </ul>
        </nav>
        <nav aria-label="Cuenta" className="grid content-start gap-3">
          <h2 className="font-semibold">Cuenta</h2>
          <ul className="grid gap-2">
            {conSesion ? (
              <li>
                <Link to="/inicio" className={enlace}>Ir a mi inicio</Link>
              </li>
            ) : (
              <>
                <li>
                  <Link to="/login" className={enlace}>Iniciar sesión</Link>
                </li>
                <li>
                  <Link to="/registro" className={enlace}>Registrar mi empresa</Link>
                </li>
              </>
            )}
          </ul>
        </nav>
        <p className="border-t border-border-default pt-6 text-xs text-text-secondary sm:col-span-2 lg:col-span-4">© 2026 Norvia</p>
      </div>
    </footer>
  )
}
