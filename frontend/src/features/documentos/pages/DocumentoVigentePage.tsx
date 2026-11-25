import { Download, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { ContenidoDocumento } from '@/shared/documentos'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { Boton } from '@/shared/components/ui/Boton'
import { Insignia } from '@/shared/components/ui/Insignia'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { obtenerVigente } from '../api/documentos'
import { fecha, NOMBRE_TIPO } from '../etiquetas'

/**
 * HU-074: la versión vigente de un documento, para todas las personas de la empresa. "Descargar PDF" abre la impresión
 * del navegador con solo el documento (16-arquitectura §9): ahí se guarda como PDF o se imprime. No se guarda nada.
 */
export function DocumentoVigentePage() {
  const { id = '' } = useParams()
  const peticion = usePeticion(obtenerVigente)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(id), [ejecutar, id])
  useEffect(cargar, [cargar])

  const d = peticion.datos?.id === id ? peticion.datos : undefined

  // El título de la pestaña es el nombre que el navegador propone para el PDF.
  useEffect(() => {
    if (!d) return
    const original = document.title
    const antes = () => (document.title = d.titulo)
    const despues = () => (document.title = original)
    window.addEventListener('beforeprint', antes)
    window.addEventListener('afterprint', despues)
    return () => {
      window.removeEventListener('beforeprint', antes)
      window.removeEventListener('afterprint', despues)
      document.title = original
    }
  }, [d])

  if (peticion.error?.status === 404) return <NotFoundPage />

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <Volver a="/documentos-vigentes" texto="Volver a políticas y procedimientos" titulo={d?.titulo} />
      {peticion.error && !d ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h1 className="text-md font-semibold">No se pudo cargar el documento</h1>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !d ? (
        <div aria-busy="true" aria-label="Cargando el documento" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <div className="documento-imprimible grid gap-6">
          <header className="grid gap-2">
            <div className="flex flex-wrap items-start justify-between gap-4">
              <h1 className="text-xl font-semibold wrap-break-word">{d.titulo}</h1>
              <Boton variante="secundario" className="no-imprimir" onClick={() => window.print()}>
                <Download aria-hidden size={16} strokeWidth={1.5} />
                Descargar PDF
              </Boton>
            </div>
            <div className="no-imprimir flex flex-wrap items-center gap-2">
              <Insignia tipo="neutro">{NOMBRE_TIPO[d.tipoDocumento]}</Insignia>
              <Insignia tipo="exito">Vigente</Insignia>
            </div>
            <p className="text-sm text-text-secondary">
              {`${NOMBRE_TIPO[d.tipoDocumento]} · versión ${d.numeroVersion} · vigente desde el ${fecha(d.vigenteDesde)} · aprobada por ${d.aprobadaPorNombre}`}
            </p>
          </header>
          <article aria-label={d.titulo} className="rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
            <ContenidoDocumento contenido={d.contenido} />
          </article>
        </div>
      )}
    </div>
  )
}
