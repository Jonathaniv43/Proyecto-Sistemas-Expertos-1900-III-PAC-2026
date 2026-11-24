import { Plus } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { clasesBoton } from '@/shared/components/ui/clases'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { estadoDocumento, listarDocumentos, listarRubros } from '../api/catalogos'
import { AvisoMensaje } from '../components/AvisoMensaje'
import { BotonEstado, EstadoActivo } from '../components/Estado'
import { EstadoCarga } from '../components/EstadoCarga'
import { TIPOS_DOCUMENTO } from '../etiquetas'
import { usePuedeEditar } from '../permisos'
import { useCambioEstado } from '../useCambioEstado'

/** Lo que el editor de plantillas de documento deja en la navegación al guardar. */
export interface EstadoDocumentoGuardado {
  guardado?: string
}

/**
 * HU-023: plantillas de políticas y procedimientos. Las empresas generan sus documentos a partir de ellas (M9,
 * HU-070); una plantilla inactiva deja de ofrecerse.
 */
export function DocumentosPage() {
  const documentos = usePeticion(listarDocumentos)
  const rubros = usePeticion(listarRubros)
  const puedeEditar = usePuedeEditar()
  const { state } = useLocation()
  const guardado = (state as EstadoDocumentoGuardado | null)?.guardado

  const { ejecutar: cargarDocumentos } = documentos
  const { ejecutar: cargarRubros } = rubros
  const cargar = useCallback(() => {
    void cargarDocumentos()
    void cargarRubros()
  }, [cargarDocumentos, cargarRubros])
  useEffect(cargar, [cargar])
  const { mensaje, ocupado, cambiar } = useCambioEstado(() => void cargarDocumentos())
  const nombreRubro = (id: number | null) => (id === null ? 'Todos los rubros' : (rubros.datos?.find((r) => r.id === id)?.nombre ?? '—'))

  return (
    <section aria-labelledby="titulo-documentos" className="grid gap-4">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="grid gap-1">
          <h2 id="titulo-documentos" className="text-md font-semibold">
            Plantillas de documento
          </h2>
          <p className="text-text-secondary">Políticas y procedimientos modelo que cada empresa adapta.</p>
        </div>
        {puedeEditar && (
          <Link to="/catalogos/documentos/nuevo" className={clasesBoton('secundario')}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Nueva plantilla
          </Link>
        )}
      </div>
      <div role="status" className="contents">
        {guardado && !mensaje && <AvisoMensaje mensaje={{ tipo: 'exito', texto: `«${guardado}» quedó guardada.` }} />}
      </div>
      <AvisoMensaje mensaje={mensaje} />
      <EstadoCarga
        cargando={documentos.cargando}
        error={documentos.error ?? rubros.error}
        hayDatos={!!documentos.datos && !!rubros.datos}
        que="las plantillas"
        onReintentar={cargar}
      >
        {documentos.datos?.length === 0 ? (
          <p className="text-text-secondary">Todavía no hay plantillas de documento.</p>
        ) : (
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Plantilla</th>
                  <th scope="col" className="max-sm:hidden">
                    Tipo
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Rubro
                  </th>
                  <th scope="col" className="max-sm:hidden">
                    Estado
                  </th>
                  <th scope="col">
                    <span className="sr-only">Acciones</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {documentos.datos?.map((d) => (
                  <tr key={d.id}>
                    <td className="envolver">
                      <div className="font-medium">{d.titulo}</div>
                      <div className="text-xs text-text-secondary">{d.codigo}</div>
                      <div className="mt-1 grid justify-items-start gap-1 text-sm sm:hidden">
                        <span>
                          {TIPOS_DOCUMENTO[d.tipoDocumento]} · {nombreRubro(d.rubroId)}
                        </span>
                        <EstadoActivo activo={d.activa} />
                      </div>
                    </td>
                    <td className="max-sm:hidden">{TIPOS_DOCUMENTO[d.tipoDocumento]}</td>
                    <td className="max-sm:hidden">{nombreRubro(d.rubroId)}</td>
                    <td className="max-sm:hidden">
                      <EstadoActivo activo={d.activa} />
                    </td>
                    <td className="text-right">
                      <span className="inline-flex flex-wrap items-center justify-end gap-1">
                        <Link to={`/catalogos/documentos/${d.id}`} className={clasesBoton('secundario')} aria-label={`${puedeEditar ? 'Editar' : 'Ver'} «${d.titulo}»`}>
                          {puedeEditar ? 'Editar' : 'Ver'}
                        </Link>
                        {puedeEditar && (
                          <BotonEstado
                            activo={d.activa}
                            nombre={d.titulo}
                            ocupado={ocupado !== undefined}
                            onCambiar={(activo) => void cambiar(d.id, d.titulo, activo, () => estadoDocumento(d.id, activo))}
                          />
                        )}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </EstadoCarga>
    </section>
  )
}
