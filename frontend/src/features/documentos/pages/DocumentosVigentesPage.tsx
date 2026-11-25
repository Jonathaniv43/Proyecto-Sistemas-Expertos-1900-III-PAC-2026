import { BookOpen, SearchX, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { Volver } from '@/shared/components/ui/Volver'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarVigentes } from '../api/documentos'
import { fecha, NOMBRE_TIPO } from '../etiquetas'

/**
 * HU-074: las políticas y los procedimientos vigentes de la empresa, para todas las personas con vínculo. Solo las
 * versiones vigentes; se buscan por título.
 */
export function DocumentosVigentesPage() {
  const peticion = usePeticion(listarVigentes)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const { register, control, reset } = useForm({ defaultValues: { buscar: '' } })
  const escrito = useWatch({ control, name: 'buscar' })
  // La búsqueda espera a que se deje de escribir.
  const [buscar, setBuscar] = useState('')
  useEffect(() => {
    const t = setTimeout(() => setBuscar(escrito.trim()), 300)
    return () => clearTimeout(t)
  }, [escrito])

  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(buscar), [ejecutar, buscar])
  useEffect(cargar, [cargar])

  const documentos = peticion.datos
  // Sin búsqueda y sin resultados: la empresa todavía no tiene documentos vigentes.
  const sinDocumentos = documentos?.length === 0 && !buscar

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow grid-cols-1 gap-6">
      <Volver a="/inicio" texto="Volver al inicio" titulo="Políticas y procedimientos" />
      <header className="grid gap-1">
        <h1 className="text-xl font-semibold">Políticas y procedimientos</h1>
        <p className="text-text-secondary">Los documentos de seguridad y salud vigentes en la empresa.</p>
      </header>

      {!sinDocumentos && (
        <form role="search" aria-label="Buscar documentos" onSubmit={(e) => e.preventDefault()}>
          <CampoTexto etiqueta="Buscar" type="search" autoComplete="off" placeholder="Nombre del documento" registro={register('buscar')} />
        </form>
      )}

      {peticion.error && !documentos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar los documentos</h2>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !documentos ? (
        <div aria-busy="true" aria-label="Cargando los documentos" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : sinDocumentos ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <BookOpen aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <div className="grid gap-2">
            <h2 className="text-md font-semibold">Todavía no hay documentos vigentes</h2>
            <p className="text-text-secondary">Cuando el Responsable SST apruebe una política o un procedimiento, aparecerá aquí.</p>
          </div>
        </section>
      ) : documentos.length === 0 ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <SearchX aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">{`No hay documentos para «${buscar}»`}</h2>
          <Boton variante="secundario" onClick={() => reset()}>
            Quitar la búsqueda
          </Boton>
        </section>
      ) : (
        <ul aria-label="Documentos vigentes" className="grid gap-3">
          {documentos.map((d) => (
            <li key={d.id}>
              <Link
                to={`/documentos-vigentes/${d.id}`}
                className="grid gap-1 rounded-lg border border-border-default bg-bg-surface p-4 no-underline hover:bg-bg-subtle"
              >
                <span className="font-medium text-text-link wrap-break-word">{d.titulo}</span>
                <span className="text-xs text-text-secondary">
                  {`${NOMBRE_TIPO[d.tipoDocumento]} · versión ${d.numeroVersion} · vigente desde el ${fecha(d.vigenteDesde)}`}
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
