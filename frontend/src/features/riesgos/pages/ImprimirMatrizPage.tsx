import { Printer, ShieldAlert, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Indicador } from '@/shared/components/datos/Indicador'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { EtiquetaRiesgo } from '@/shared/riesgo/EtiquetaRiesgo'
import { obtenerReporteMatriz } from '../api/riesgos'
import { LeyendaNiveles, MapaMatriz } from '../components/MapaMatriz'
import { exigeControl, NOMBRE_TIPO_CONTROL } from '../controles'
import { ETIQUETAS_CLASIFICACION, nombrePeligro } from '../etiquetas'
import type { Evaluacion, RiesgoReporteMatriz } from '../types/riesgos'

const fechaHoraHonduras = (iso: string) =>
  new Date(iso).toLocaleString('es-HN', {
    timeZone: 'America/Tegucigalpa',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })

const fechaCorta = (iso: string) =>
  new Date(iso).toLocaleDateString('es-HN', {
    timeZone: 'America/Tegucigalpa',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  })

/**
 * HU-075: vista oficial e imprimible de la matriz de riesgos de la empresa (art. 47.b, RN-52).
 * Se genera al solicitarla sin guardar archivos; "Imprimir / Guardar PDF" usa `window.print()` con página horizontal.
 */
export function ImprimirMatrizPage() {
  const peticion = usePeticion(obtenerReporteMatriz)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(), [ejecutar])
  useEffect(cargar, [cargar])

  const datos = peticion.datos

  // El título de la pestaña es el nombre sugerido por el navegador al guardar como PDF.
  useEffect(() => {
    if (!datos) return
    const original = document.title
    const antes = () => (document.title = `Matriz de riesgos — ${datos.empresa.razonSocial}`)
    const despues = () => (document.title = original)
    window.addEventListener('beforeprint', antes)
    window.addEventListener('afterprint', despues)
    return () => {
      window.removeEventListener('beforeprint', antes)
      window.removeEventListener('afterprint', despues)
      document.title = original
    }
  }, [datos])

  return (
    <div className="grid w-full max-w-layout grid-cols-1 gap-6">
      <Volver a="/riesgos" texto="Volver a la matriz" titulo="Imprimir matriz de riesgos" ancho="layout" />

      {peticion.error && !datos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudo generar el reporte de la matriz</h1>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !datos ? (
        <div aria-busy="true" aria-label="Generando vista previa de la matriz" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2, 3].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <div className="matriz-imprimible grid gap-6">
          <header className="flex flex-wrap items-start justify-between gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
            <div className="grid gap-2">
              <div className="grid gap-1">
                <h1 className="text-xl font-semibold">Matriz de identificación de peligros, evaluación y control de riesgos</h1>
                <p className="text-sm text-text-secondary">
                  Generada el <span className="tabular-nums">{fechaHoraHonduras(datos.generadoEn)}</span> (hora de Honduras) · Solo
                  riesgos confirmados vigentes
                </p>
              </div>
              <dl className="mt-2 grid gap-x-8 gap-y-2 text-sm sm:grid-cols-2 lg:grid-cols-3">
                <div>
                  <dt className="text-xs text-text-secondary">Empresa</dt>
                  <dd className="font-medium text-text-primary">{datos.empresa.razonSocial}</dd>
                </div>
                <div>
                  <dt className="text-xs text-text-secondary">RTN</dt>
                  <dd className="font-medium tabular-nums text-text-primary">{datos.empresa.rtn}</dd>
                </div>
                <div>
                  <dt className="text-xs text-text-secondary">Rubro</dt>
                  <dd className={datos.empresa.rubroNombre ? 'text-text-primary' : 'text-text-secondary'}>
                    {datos.empresa.rubroNombre ?? 'No registrado'}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs text-text-secondary">Actividad económica</dt>
                  <dd className={datos.empresa.actividadEconomica ? 'text-text-primary' : 'text-text-secondary'}>
                    {datos.empresa.actividadEconomica ?? 'No registrada'}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs text-text-secondary">Dirección</dt>
                  <dd className={datos.empresa.direccion ? 'text-text-primary' : 'text-text-secondary'}>
                    {datos.empresa.direccion ?? 'No registrada'}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs text-text-secondary">Trabajadores permanentes</dt>
                  <dd className="tabular-nums text-text-primary">
                    {datos.empresa.trabajadoresPermanentes !== null ? datos.empresa.trabajadoresPermanentes : 'No registrado'}
                  </dd>
                </div>
              </dl>
            </div>
            <div className="no-imprimir flex flex-col items-start gap-2 sm:items-end">
              <Boton variante="primario" onClick={() => window.print()}>
                <Printer aria-hidden size={16} strokeWidth={1.5} />
                Imprimir / Guardar PDF
              </Boton>
              <span className="text-xs text-text-secondary">El diálogo de impresión permite imprimir o guardar como PDF.</span>
            </div>
          </header>

          {datos.riesgos.length === 0 ? (
            <section className="bloque-impresion flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-12 text-center">
              <ShieldAlert aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
              <h2 className="text-md font-semibold">Todavía no hay riesgos confirmados en la matriz</h2>
              <p className="max-w-layout-narrow text-text-secondary">
                Cuando confirmes riesgos desde una encuesta o agregues riesgos a la matriz, aparecerán aquí para imprimir o descargar.
              </p>
            </section>
          ) : (
            <>
              <section
                aria-label="Resumen y mapa de calor de la matriz"
                className="bloque-impresion grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6 lg:grid-cols-2 lg:items-center"
              >
                <div className="grid gap-4">
                  <dl className="grid gap-3 sm:grid-cols-3">
                    <Indicador etiqueta="Riesgos confirmados" valor={datos.matriz.total} />
                    <Indicador etiqueta="Sin evaluar" valor={datos.matriz.sinEvaluar} />
                    <Indicador etiqueta="Altos / críticos sin control" valor={datos.matriz.altosSinControl} />
                  </dl>
                  <LeyendaNiveles />
                </div>
                <MapaMatriz matriz={datos.matriz} seleccion={null} soloLectura />
              </section>

              <section aria-label="Detalle completo de riesgos confirmados" className="grid gap-2">
                <p className="text-xs text-text-secondary tabular-nums">
                  {datos.riesgos.length} {datos.riesgos.length === 1 ? 'riesgo confirmado' : 'riesgos confirmados'}, ordenados del nivel
                  inicial más alto al más bajo
                </p>
                <div className="tabla-contenedor">
                  <table className="tabla">
                    <thead>
                      <tr>
                        <th scope="col">#</th>
                        <th scope="col">Peligro y actividad</th>
                        <th scope="col">Evaluación inicial</th>
                        <th scope="col">Evaluación residual</th>
                        <th scope="col">Controles aplicados</th>
                      </tr>
                    </thead>
                    <tbody>
                      {datos.riesgos.map((r, indice) => (
                        <FilaReporte key={r.id} riesgo={r} numero={indice + 1} />
                      ))}
                    </tbody>
                  </table>
                </div>
              </section>
            </>
          )}
        </div>
      )}
    </div>
  )
}

function FilaReporte({ riesgo: r, numero }: { riesgo: RiesgoReporteMatriz; numero: number }) {
  const sinControlObligatorio = Boolean(r.evaluacion && exigeControl(r) && r.controles.length === 0)

  return (
    <tr>
      <td className="align-top text-xs text-text-secondary tabular-nums">{numero}</td>
      <td className="envolver align-top">
        <div className="font-medium text-text-primary">{nombrePeligro(r)}</div>
        <div className="text-xs text-text-secondary">
          {r.peligroClasificacion ? ETIQUETAS_CLASIFICACION[r.peligroClasificacion] : 'Descrito por la empresa'}
        </div>
        {r.actividad && <div className="mt-1 text-sm text-text-primary">Actividad: {r.actividad}</div>}
        {r.encuestaTitulo && <div className="text-xs text-text-secondary">Origen: encuesta «{r.encuestaTitulo}»</div>}
      </td>
      <td className="envolver align-top">
        <BloqueEvaluacion evaluacion={r.evaluacion} vacio="Sin evaluar" />
      </td>
      <td className="envolver align-top">
        <BloqueEvaluacion evaluacion={r.evaluacionResidual} vacio="Sin evaluación residual" />
      </td>
      <td className="envolver align-top">
        {r.controles.length > 0 ? (
          <ul className="grid gap-2 text-sm">
            {r.controles.map((c) => (
              <li key={c.id} className="grid gap-1">
                <div className="flex flex-wrap items-center gap-2">
                  <span className="font-medium text-text-primary">{NOMBRE_TIPO_CONTROL[c.tipo]}</span>
                  <Insignia tipo={c.estado === 'implementado' ? 'exito' : 'neutro'}>
                    {c.estado === 'implementado' ? 'Implementado' : 'Propuesto'}
                  </Insignia>
                </div>
                <span className="text-text-secondary">{c.descripcion}</span>
              </li>
            ))}
          </ul>
        ) : (
          <div className="grid justify-items-start gap-1">
            <span className="text-sm text-text-secondary">Sin controles registrados</span>
            {sinControlObligatorio && <Insignia tipo="advertencia">Alto o crítico sin control (RN-19)</Insignia>}
          </div>
        )}
      </td>
    </tr>
  )
}

function BloqueEvaluacion({ evaluacion, vacio }: { evaluacion: Evaluacion | null; vacio: string }) {
  if (!evaluacion) return <span className="text-sm text-text-secondary">{vacio}</span>
  return (
    <div className="grid justify-items-start gap-1">
      <EtiquetaRiesgo nivel={evaluacion} />
      <span className="text-xs text-text-secondary tabular-nums">
        Probabilidad {evaluacion.probabilidad} × Severidad {evaluacion.severidad}
      </span>
      <span className="text-xs text-text-secondary">
        {evaluacion.evaluadoPorNombre} · {fechaCorta(evaluacion.evaluadoEn)}
      </span>
    </div>
  )
}
