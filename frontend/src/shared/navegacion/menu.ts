import {
  BookOpen,
  Building,
  Building2,
  ClipboardCheck,
  ClipboardList,
  Contact,
  FileText,
  History,
  House,
  ListChecks,
  ListTodo,
  Megaphone,
  ScrollText,
  ShieldAlert,
  Siren,
  type LucideIcon,
} from 'lucide-react'
import { ROLES, type Rol } from '@/shared/sesion'

/** Un destino del menú: una pantalla de nivel 1 (design-system.md § Layouts y navegación). */
export interface ItemMenu {
  ruta: string
  nombre: string
  Icono: LucideIcon
}

/** Grupo del menú; sin `titulo` va sin rótulo (los menús de cinco destinos o menos no llevan grupos). */
export interface GrupoMenu {
  titulo?: string
  items: readonly ItemMenu[]
}

const I = {
  inicio: { ruta: '/inicio', nombre: 'Inicio', Icono: House },
  misEncuestas: { ruta: '/mis-encuestas', nombre: 'Mis encuestas', Icono: ClipboardCheck },
  misTareas: { ruta: '/mis-tareas', nombre: 'Mis tareas', Icono: ListTodo },
  politicas: { ruta: '/documentos-vigentes', nombre: 'Políticas y procedimientos', Icono: ScrollText },
  reportar: { ruta: '/eventos/reportar', nombre: 'Reportar un evento', Icono: Megaphone },
  encuestas: { ruta: '/encuestas', nombre: 'Encuestas', Icono: ClipboardList },
  riesgos: { ruta: '/riesgos', nombre: 'Matriz de riesgos', Icono: ShieldAlert },
  plan: { ruta: '/plan-de-trabajo', nombre: 'Plan de trabajo', Icono: ListChecks },
  eventos: { ruta: '/eventos', nombre: 'Eventos', Icono: Siren },
  documentos: { ruta: '/documentos', nombre: 'Documentos', Icono: FileText },
  personas: { ruta: '/personas', nombre: 'Personas', Icono: Contact },
  perfil: { ruta: '/empresa', nombre: 'Perfil de la empresa', Icono: Building2 },
  auditoria: { ruta: '/auditoria', nombre: 'Auditoría', Icono: History },
  empresas: { ruta: '/empresas', nombre: 'Empresas', Icono: Building },
  catalogos: { ruta: '/catalogos', nombre: 'Catálogos', Icono: BookOpen },
} satisfies Record<string, ItemMenu>

const GESTION: GrupoMenu = { titulo: 'Gestión SST', items: [I.encuestas, I.riesgos, I.plan, I.eventos, I.documentos] }
const PARA_MI: GrupoMenu = { titulo: 'Para mí', items: [I.misTareas, I.misEncuestas, I.politicas, I.reportar] }
const PLATAFORMA: readonly GrupoMenu[] = [{ items: [I.inicio, I.empresas, I.catalogos] }]

/**
 * Menú de cada rol. Lo que muestra es solo navegación: cada ruta la protege además RequiereRol y el backend.
 * El admin también gestiona SST (decisión del 27-11-2026); el Responsable SST no ve Personas.
 */
const MENUS: Record<Rol, readonly GrupoMenu[]> = {
  [ROLES.encuestado]: [{ items: [I.inicio, I.misEncuestas, I.politicas, I.reportar] }],
  [ROLES.colaborador]: [{ items: [I.inicio, I.misTareas, I.misEncuestas, I.politicas, I.reportar] }],
  [ROLES.responsableSst]: [{ items: [I.inicio] }, GESTION, PARA_MI, { titulo: 'Empresa', items: [I.perfil, I.auditoria] }],
  [ROLES.adminEmpresa]: [{ items: [I.inicio] }, GESTION, PARA_MI, { titulo: 'Empresa', items: [I.personas, I.perfil, I.auditoria] }],
  [ROLES.adminPlataforma]: PLATAFORMA,
  [ROLES.soportePlataforma]: PLATAFORMA,
}

export const menuDe = (rol: string | null): readonly GrupoMenu[] =>
  rol && rol in MENUS ? MENUS[rol as Rol] : [{ items: [I.inicio] }]

/**
 * Ítem marcado para una ruta: el de ruta más larga que la contiene. Así un detalle marca su módulo ("/riesgos/12" →
 * Matriz de riesgos) y "/eventos/reportar" marca "Reportar un evento", no "Eventos".
 */
export function itemActual(grupos: readonly GrupoMenu[], pathname: string): string | undefined {
  let actual: string | undefined
  for (const { ruta } of grupos.flatMap((g) => g.items)) {
    const contiene = pathname === ruta || pathname.startsWith(`${ruta}/`)
    if (contiene && (!actual || ruta.length > actual.length)) actual = ruta
  }
  return actual
}
