import { Building2, SearchX, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { Indicador } from '@/shared/components/datos/Indicador'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { listarEmpresasPlataforma } from '../api/plataforma'
import type { EmpresaPlataformaResumen, EstadoEmpresa } from '../types/plataforma'

const ESTADOS_EMPRESA: Record<EstadoEmpresa, { texto: string; tipo: 'exito' | 'advertencia' | 'neutro' }> = {
  activa: { texto: 'Activa', tipo: 'exito' },
  suspendida: { texto: 'Suspendida', tipo: 'advertencia' },
  inactiva: { texto: 'Inactiva', tipo: 'neutro' },
}

const fecha = (iso: string) =>
  new Date(iso).toLocaleDateString('es-HN', { timeZone: 'America/Tegucigalpa', dateStyle: 'medium' })

/**
 * HU-024: empresas registradas en la plataforma para el staff (admin y soporte), con rubro, fecha de registro,
 * estado y cantidad de personas con acceso vigente. No incluye datos SST (RN-05).
 */
export function EmpresasRegistradasPage() {
  const peticion = usePeticion(listarEmpresasPlataforma)
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

  const empresas = peticion.datos
  const sinEmpresas = empresas?.length === 0 && !buscar
  const totalVinculos = empresas?.reduce((acumulado, e) => acumulado + e.vinculosActivos, 0) ?? 0

  return (
    <div className="grid w-full max-w-layout grid-cols-1 gap-6">
      <header className="grid gap-1">
        <h1 className="text-xl font-semibold">Empresas registradas</h1>
        <p className="text-text-secondary">
          Uso general de la plataforma por empresa, sin acceso a su información de seguridad y salud.
        </p>
      </header>

      {empresas && !sinEmpresas && (
        <dl className="grid gap-4 sm:grid-cols-2">
          <Indicador
            etiqueta={buscar ? 'Empresas encontradas' : 'Empresas registradas'}
            valor={empresas.length}
          />
          <Indicador
            etiqueta={buscar ? 'Personas vigentes en el filtro' : 'Personas con acceso vigente'}
            valor={totalVinculos}
          />
        </dl>
      )}

      {!sinEmpresas && (
        <form role="search" aria-label="Buscar empresas" className="max-w-layout-narrow" onSubmit={(e) => e.preventDefault()}>
          <CampoTexto
            etiqueta="Buscar"
            type="search"
            autoComplete="off"
            placeholder="Nombre o rubro de la empresa"
            registro={register('buscar')}
          />
        </form>
      )}

      {peticion.error && !empresas ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar las empresas</h2>
          <p className="text-text-secondary">{peticion.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !empresas ? (
        <div aria-busy="true" aria-label="Cargando las empresas" className="grid gap-2">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : sinEmpresas ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <Building2 aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <div className="grid gap-2">
            <h2 className="text-md font-semibold">Todavía no hay empresas registradas</h2>
            <p className="max-w-layout-narrow text-text-secondary">
              Cuando una empresa cree su espacio en Norvia, aparecerá aquí con su rubro, fecha de registro y accesos vigentes.
            </p>
          </div>
        </section>
      ) : empresas.length === 0 ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <SearchX aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">{`No hay empresas para «${buscar}»`}</h2>
          <Boton variante="secundario" onClick={() => reset()}>
            Quitar la búsqueda
          </Boton>
        </section>
      ) : (
        <div className="tabla-contenedor">
          <table className="tabla">
            <thead>
              <tr>
                <th scope="col">Empresa</th>
                <th scope="col" className="max-sm:hidden">
                  Rubro
                </th>
                <th scope="col">Estado</th>
                <th scope="col" className="max-sm:hidden">
                  Registro
                </th>
                <th scope="col" className="text-right max-sm:hidden">
                  Acceso vigente
                </th>
              </tr>
            </thead>
            <tbody>
              {empresas.map((e) => (
                <FilaEmpresa key={e.empresaId} empresa={e} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

function FilaEmpresa({ empresa: e }: { empresa: EmpresaPlataformaResumen }) {
  const estado = ESTADOS_EMPRESA[e.estado]
  const rubro = e.rubroNombre ?? 'Sin rubro'
  const personasTexto = e.vinculosActivos === 1 ? '1 persona vigente' : `${e.vinculosActivos} personas vigentes`

  return (
    <tr>
      <td className="envolver">
        <div className="font-medium wrap-break-word">{e.razonSocial}</div>
        <div className="text-xs text-text-secondary sm:hidden">
          {`${rubro} · ${fecha(e.registradaEn)} · ${personasTexto}`}
        </div>
      </td>
      <td className={e.rubroNombre ? 'max-sm:hidden' : 'text-text-secondary max-sm:hidden'}>{rubro}</td>
      <td>
        <Insignia tipo={estado.tipo}>{estado.texto}</Insignia>
      </td>
      <td className="tabular-nums max-sm:hidden">{fecha(e.registradaEn)}</td>
      <td className="text-right tabular-nums max-sm:hidden">{e.vinculosActivos}</td>
    </tr>
  )
}
