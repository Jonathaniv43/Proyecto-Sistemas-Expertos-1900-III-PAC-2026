import { Bell, Building, FileText, ListChecks, ListTodo, ShieldAlert, Siren } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { UsuarioSesion } from '@/shared/api'
import { Indicador } from '@/shared/components/datos/Indicador'
import { Aviso } from '@/shared/components/ui/Aviso'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { ROLES, ROLES_GESTION, ROLES_PLATAFORMA, ROLES_TAREAS, useSesion, useTieneRol } from '@/shared/sesion'
import { obtenerPerfilCompleto } from '../api/perfil'
import {
  contarDocumentosEnBorrador,
  contarNotificacionesSinLeer,
  obtenerEncuestasPorResponder,
  obtenerEventos,
  obtenerMatriz,
  obtenerMisTareas,
  obtenerPlan,
  obtenerResumenPlataforma,
} from '../api/resumen'
import { AlDia, CargandoPendientes, EncuestaDestacada, FilaError, FilaPendiente, ListaPendientes } from '../components/Pendientes'
import { cantidad, PESO, type Fila } from '../filas'
import { useFuente } from '../hooks/useFuente'

/**
 * Primera pantalla después de iniciar sesión (design-system.md § Inicio): lo que cada persona tiene pendiente, con un
 * toque para atenderlo. Los módulos están en el menú, no aquí. El staff ve el uso de la plataforma.
 */
export function InicioPage() {
  const sesion = useSesion()
  const esStaff = useTieneRol(ROLES_PLATAFORMA)
  if (sesion.estado !== 'activa') return null
  return esStaff ? <InicioPlataforma /> : <InicioEmpresa usuario={sesion.usuario} />
}

function Encabezado({ texto }: { texto: string }) {
  return (
    <div className="grid gap-1">
      <h1 className="text-xl font-semibold">Inicio</h1>
      <p className="text-text-secondary">{texto}</p>
    </div>
  )
}

function InicioEmpresa({ usuario }: { usuario: UsuarioSesion }) {
  const gestiona = useTieneRol(ROLES_GESTION)
  const tieneTareas = useTieneRol(ROLES_TAREAS)
  const administra = useTieneRol([ROLES.adminEmpresa])

  const encuestas = useFuente(obtenerEncuestasPorResponder)
  const notificaciones = useFuente(contarNotificacionesSinLeer)
  const tareas = useFuente(obtenerMisTareas, tieneTareas)
  const plan = useFuente(obtenerPlan, gestiona)
  const matriz = useFuente(obtenerMatriz, gestiona)
  const eventos = useFuente(obtenerEventos, gestiona)
  const borradores = useFuente(contarDocumentosEnBorrador, gestiona)
  // El perfil solo avisa si falta algo: si no carga, no se muestra nada (no es un pendiente que se pueda reintentar aquí).
  const perfil = useFuente(obtenerPerfilCompleto, administra)

  const fuentes = [encuestas, notificaciones, tareas, plan, matriz, eventos, borradores, perfil]
  const listo = fuentes.every((f) => f.lista)
  const cargandoVisible = useCargaVisible(!listo)

  const lugar = usuario.empresa?.razonSocial ?? 'tu empresa'
  const encabezado = <Encabezado texto={`Lo que tienes pendiente en ${lugar}${lugar.endsWith('.') ? '' : '.'}`} />
  if (!listo)
    return (
      <section className="grid w-full max-w-layout-narrow gap-6">
        {encabezado}
        {cargandoVisible && <CargandoPendientes />}
      </section>
    )

  const error = (clave: string, mensaje: string, fuente: { reintentar: () => void; cargando: boolean }): Fila => ({
    clave,
    peso: PESO.error,
    nodo: <FilaError mensaje={mensaje} onReintentar={fuente.reintentar} reintentando={fuente.cargando} />,
  })

  // Para ti: lo propio de la persona.
  const propias: Fila[] = []
  if (encuestas.error) propias.push(error('encuestas', 'No se pudieron cargar tus encuestas', encuestas))
  if (tareas.error) propias.push(error('tareas', 'No se pudieron cargar tus tareas', tareas))
  else if (tareas.datos?.porHacer) {
    const { porHacer, vencidas } = tareas.datos
    propias.push({
      clave: 'tareas',
      peso: vencidas ? PESO.vencido : PESO.normal,
      nodo: (
        <FilaPendiente
          a="/mis-tareas"
          Icono={ListTodo}
          nombre="Mis tareas"
          detalle={cantidad(porHacer, 'tarea por hacer', 'tareas por hacer')}
          insignia={vencidas ? { texto: cantidad(vencidas, 'vencida', 'vencidas'), tipo: 'error' } : undefined}
        />
      ),
    })
  }
  if (notificaciones.error) propias.push(error('notificaciones', 'No se pudieron cargar tus notificaciones', notificaciones))
  else if (notificaciones.datos)
    propias.push({
      clave: 'notificaciones',
      peso: PESO.aviso,
      nodo: <FilaPendiente a="/notificaciones" Icono={Bell} nombre="Notificaciones" detalle={`${notificaciones.datos} sin leer`} />,
    })

  // En la empresa: lo que la gestión debe atender.
  const empresa: Fila[] = []
  if (gestiona) {
    if (plan.error) empresa.push(error('plan', 'No se pudo cargar el plan de trabajo', plan))
    else if (plan.datos && (plan.datos.vencidas || plan.datos.sinResponsable || plan.datos.propuestas)) {
      const { vencidas, sinResponsable, propuestas } = plan.datos
      const partes = [
        vencidas && cantidad(vencidas, 'tarea vencida', 'tareas vencidas'),
        sinResponsable && `${sinResponsable} sin responsable`,
        propuestas && cantidad(propuestas, 'tarea propuesta por confirmar', 'tareas propuestas por confirmar'),
      ].filter(Boolean)
      empresa.push({
        clave: 'plan',
        peso: vencidas ? PESO.vencido : PESO.normal,
        nodo: (
          <FilaPendiente
            a="/plan-de-trabajo"
            Icono={ListChecks}
            nombre="Plan de trabajo"
            detalle={partes.join(' · ')}
            insignia={vencidas ? { texto: cantidad(vencidas, 'vencida', 'vencidas'), tipo: 'error' } : undefined}
          />
        ),
      })
    }
    if (matriz.error) empresa.push(error('matriz', 'No se pudo cargar la matriz de riesgos', matriz))
    else if (matriz.datos && (matriz.datos.sinControl || matriz.datos.sinEvaluar)) {
      const { sinControl, sinEvaluar } = matriz.datos
      const partes = [
        sinControl && cantidad(sinControl, 'riesgo alto o crítico sin control', 'riesgos altos o críticos sin control'),
        sinEvaluar && `${sinEvaluar} sin evaluar`,
      ].filter(Boolean)
      empresa.push({
        clave: 'matriz',
        peso: sinControl ? PESO.atencion : PESO.normal,
        nodo: (
          <FilaPendiente
            a="/riesgos"
            Icono={ShieldAlert}
            nombre="Matriz de riesgos"
            detalle={partes.join(' · ')}
            insignia={sinControl ? { texto: 'Sin control', tipo: 'advertencia' } : undefined}
          />
        ),
      })
    }
    if (eventos.error) empresa.push(error('eventos', 'No se pudieron cargar los eventos', eventos))
    else if (eventos.datos && (eventos.datos.abiertos || eventos.datos.enInvestigacion)) {
      const { abiertos, enInvestigacion, graves } = eventos.datos
      const partes = [
        abiertos && cantidad(abiertos, 'evento abierto', 'eventos abiertos'),
        enInvestigacion && `${enInvestigacion} en investigación`,
      ].filter(Boolean)
      empresa.push({
        clave: 'eventos',
        peso: graves ? PESO.vencido : PESO.atencion,
        nodo: (
          <FilaPendiente
            a="/eventos"
            Icono={Siren}
            nombre="Eventos"
            detalle={partes.join(' · ')}
            insignia={graves ? { texto: cantidad(graves, 'grave', 'graves'), tipo: 'error' } : undefined}
          />
        ),
      })
    }
    if (borradores.error) empresa.push(error('documentos', 'No se pudieron cargar los documentos', borradores))
    else if (borradores.datos)
      empresa.push({
        clave: 'documentos',
        peso: PESO.normal,
        nodo: (
          <FilaPendiente
            a="/documentos"
            Icono={FileText}
            nombre="Documentos"
            detalle={cantidad(borradores.datos, 'documento en borrador', 'documentos en borrador')}
          />
        ),
      })
  }

  const porResponder = encuestas.datos ?? []
  const perfilIncompleto = perfil.datos === false
  const nada = porResponder.length === 0 && propias.length === 0 && empresa.length === 0 && !perfilIncompleto

  return (
    <section className="grid w-full max-w-layout-narrow gap-6">
      {encabezado}
      {perfilIncompleto && (
        <Aviso tipo="advertencia" titulo="Completa el perfil de la empresa">
          Falta el rubro o el número de trabajadores permanentes. <Link to="/empresa">Completar el perfil</Link>
        </Aviso>
      )}
      <EncuestaDestacada encuestas={porResponder} />
      {nada ? (
        <AlDia />
      ) : gestiona ? (
        <>
          <ListaPendientes titulo="Para ti" filas={propias} />
          <ListaPendientes titulo="En la empresa" filas={empresa} />
        </>
      ) : (
        <ListaPendientes filas={propias} />
      )}
    </section>
  )
}

function InicioPlataforma() {
  const resumen = useFuente(obtenerResumenPlataforma)
  const cargandoVisible = useCargaVisible(!resumen.lista)
  const datos = resumen.datos

  return (
    <section className="grid w-full max-w-layout-narrow gap-6">
      <Encabezado texto="Cómo se está usando Norvia." />
      {!resumen.lista ? (
        cargandoVisible && <CargandoPendientes />
      ) : resumen.error || !datos ? (
        <ul className="lista-pendientes">
          <li>
            <FilaError mensaje="No se pudieron cargar las empresas" onReintentar={resumen.reintentar} reintentando={resumen.cargando} />
          </li>
        </ul>
      ) : (
        <>
          <dl className="grid gap-4 sm:grid-cols-2">
            <Indicador
              etiqueta="Empresas registradas"
              valor={datos.empresas}
              detalle={datos.suspendidas ? cantidad(datos.suspendidas, 'suspendida', 'suspendidas') : 'Ninguna suspendida'}
            />
            <Indicador etiqueta="Personas con acceso vigente" valor={datos.personas} detalle="En todas las empresas" />
          </dl>
          {datos.suspendidas > 0 && (
            <ul className="lista-pendientes">
              <li>
                <FilaPendiente
                  a="/empresas"
                  Icono={Building}
                  nombre="Empresas"
                  detalle={cantidad(datos.suspendidas, 'empresa suspendida', 'empresas suspendidas')}
                  insignia={{ texto: 'Suspendidas', tipo: 'advertencia' }}
                />
              </li>
            </ul>
          )}
        </>
      )}
    </section>
  )
}
