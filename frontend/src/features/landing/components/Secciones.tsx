import {
  Building2,
  ChevronDown,
  CircleCheck,
  EyeOff,
  FolderX,
  HardHat,
  Repeat,
  ShieldCheck,
  UserX,
  type LucideIcon,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { clasesBoton } from '@/shared/components/ui/clases'

/** Secciones de la landing (design-system.md § Landing). Textos aprobados el 28-11-2026 (versión 3). */

function Seccion({ id, superficie = false, children }: { id?: string; superficie?: boolean; children: ReactNode }) {
  return (
    <section id={id} className={`py-12 sm:py-16 ${superficie ? 'border-y border-border-default bg-bg-surface' : ''}`}>
      <div className="margen-lateral mx-auto w-full max-w-layout">{children}</div>
    </section>
  )
}

function Titulo({ titulo, texto }: { titulo: string; texto?: string }) {
  return (
    <div className="mb-8 grid max-w-layout-narrow gap-2">
      <h2 className="text-lg font-semibold text-balance">{titulo}</h2>
      {texto && <p className="text-md text-text-secondary">{texto}</p>}
    </div>
  )
}

const PROBLEMAS: [LucideIcon, string, string][] = [
  [EyeOff, 'Riesgos que nadie ve', 'Sin preguntar a quienes hacen el trabajo, la empresa no sabe a qué están expuestos ni qué es más urgente.'],
  [UserX, 'Tareas sin dueño', 'Las acciones se acuerdan en una reunión y se olvidan: no tienen responsable ni fecha límite.'],
  [FolderX, 'Evidencia dispersa', 'Cuando llega una inspección o un cliente pregunta, juntar la matriz, los accidentes y las políticas toma días.'],
]

export function Problema() {
  return (
    <Seccion superficie>
      <Titulo
        titulo="La seguridad de tu empresa no cabe en un archivador"
        texto="La ley exige identificar y controlar los riesgos, registrar los accidentes y tener un plan. Con plantillas de Word, correos y carpetas, eso se pierde."
      />
      <div className="grid gap-6 md:grid-cols-3">
        {PROBLEMAS.map(([Icono, titulo, texto]) => (
          <div key={titulo} className="grid content-start gap-2">
            <Icono aria-hidden size={32} strokeWidth={1.5} className="text-text-secondary" />
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

export function ComoFunciona() {
  return (
    <Seccion id="como-funciona">
      <Titulo titulo="Del primer registro a un plan en marcha" />
      <ol className="grid gap-6 lg:grid-cols-5">
        {PASOS.map(([titulo, texto], i) => (
          <li key={titulo} className="landing-paso">
            <span aria-hidden className="landing-paso-numero grid size-8 place-items-center rounded-full border border-border-strong font-semibold tabular-nums">
              {i + 1}
            </span>
            <h3 className="text-md font-semibold">{titulo}</h3>
            <p className="text-text-secondary">{texto}</p>
          </li>
        ))}
      </ol>
      <p className="mt-8 flex items-center gap-2 text-text-secondary">
        <Repeat aria-hidden size={16} strokeWidth={1.5} className="shrink-0" />
        El ciclo se repite: una nueva encuesta o un accidente actualizan la matriz y el plan.
      </p>
    </Seccion>
  )
}

export function QueCubre() {
  return (
    <Seccion id="que-cubre" superficie>
      <div className="grid max-w-layout-narrow gap-2">
        <h2 className="text-lg font-semibold">Pensado para el reglamento hondureño</h2>
        <p className="text-md text-text-secondary">
          Norvia te ayuda a cumplir lo que exige el acuerdo STSS-053-04: identificar y controlar los riesgos, tener un plan
          para corregirlos e investigar y registrar los accidentes.
        </p>
        <p className="text-xs text-text-secondary">
          También prepara a tu empresa para la norma ISO 45001. La certificación la otorga un organismo externo.
        </p>
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
        {ROLES.map(([Icono, titulo, items]) => (
          <div key={titulo} className="grid content-start gap-3 rounded-lg border border-border-default p-6">
            <Icono aria-hidden size={32} strokeWidth={1.5} />
            <h3 className="text-md font-semibold">{titulo}</h3>
            <ul className="grid list-disc gap-1 pl-4 text-text-secondary">
              {items.map((t) => (
                <li key={t}>{t}</li>
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
      <div className="grid items-start gap-12 lg:grid-cols-2">
        <div className="grid gap-2">
          <h2 className="text-lg font-semibold">Respuestas honestas porque son anónimas</h2>
          <p className="text-md text-text-secondary">
            Un trabajador solo cuenta lo que pasa si sabe que no lo van a señalar. Norvia separa quién participó de lo que
            respondió.
          </p>
        </div>
        <ul className="grid gap-4">
          {['Las respuestas no llevan nombre.', 'Reportes de eventos anónimos.', 'Cada empresa ve solo lo suyo.'].map((t) => (
            <li key={t} className="flex items-center gap-3 text-md font-medium">
              <CircleCheck aria-hidden size={20} strokeWidth={1.5} className="shrink-0 text-success-icon" />
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

export function Preguntas() {
  return (
    <Seccion id="preguntas">
      <Titulo titulo="Preguntas frecuentes" />
      <div className="grid max-w-layout-narrow gap-2">
        {PREGUNTAS.map(([pregunta, respuesta], i) => (
          <details key={pregunta} open={i === 0} className="group rounded-lg border border-border-default bg-bg-surface">
            <summary className="landing-pregunta flex min-h-control-touch cursor-pointer list-none items-center justify-between gap-4 p-4 font-medium">
              {pregunta}
              <ChevronDown aria-hidden size={16} strokeWidth={1.5} className="shrink-0 transition-transform group-open:rotate-180 motion-reduce:transition-none" />
            </summary>
            <p className="px-4 pb-4 text-text-secondary">{respuesta}</p>
          </details>
        ))}
      </div>
    </Seccion>
  )
}

export function Cierre({ conSesion }: { conSesion: boolean }) {
  return (
    <Seccion superficie>
      <div className="grid justify-items-center gap-6 text-center">
        <h2 className="text-xl font-semibold tracking-tight sm:text-2xl">Empieza por conocer tus riesgos</h2>
        <p className="max-w-layout-narrow text-md text-text-secondary">Registra tu empresa, agrega a tu equipo y publica tu primera encuesta.</p>
        {conSesion ? (
          <Link to="/inicio" className={clasesBoton('primario')}>
            Ir a mi inicio
          </Link>
        ) : (
          <div className="flex flex-col items-center gap-4 sm:flex-row">
            <Link to="/registro" className={clasesBoton('primario')}>
              Registrar mi empresa
            </Link>
            <Link to="/login" className="font-medium">
              Ya tengo cuenta: iniciar sesión
            </Link>
          </div>
        )}
      </div>
    </Seccion>
  )
}

export function Pie() {
  return (
    <footer className="border-t border-border-default py-8">
      <div className="margen-lateral mx-auto flex w-full max-w-layout flex-wrap justify-between gap-4 text-xs text-text-secondary">
        <span className="font-semibold text-text-primary">Norvia</span>
        <span>Seguridad y salud en el trabajo para empresas hondureñas · 2026</span>
      </div>
    </footer>
  )
}
