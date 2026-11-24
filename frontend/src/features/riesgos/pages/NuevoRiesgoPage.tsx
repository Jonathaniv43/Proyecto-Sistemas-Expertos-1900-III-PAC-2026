import { TriangleAlert } from 'lucide-react'
import { useEffect } from 'react'
import { useController, useWatch } from 'react-hook-form'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { SelectBusqueda } from '@/shared/components/ui/SelectBusqueda'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { crearRiesgo, listarPeligrosDisponibles } from '../api/riesgos'
import { ETIQUETAS_CLASIFICACION, nombrePeligro } from '../etiquetas'
import { nuevoRiesgoEsquema } from '../schemas/nuevoRiesgo'
import type { PeligroDisponible } from '../types/riesgos'
import type { EstadoRiesgoAgregado } from './RiesgosPage'

/**
 * HU-036: el Responsable SST agrega a la matriz un riesgo que la encuesta no detectó. Elige un
 * peligro del catálogo (con búsqueda) o lo describe, e indica la actividad relacionada. Queda
 * confirmado en la matriz de la empresa.
 * HU-051: con `?evento=<id>` se agrega a raíz de un evento y queda enlazado con él; después se evalúa en su página.
 */
export function NuevoRiesgoPage() {
  const peligros = usePeticion(listarPeligrosDisponibles)
  const cargandoVisible = useCargaVisible(peligros.cargando)
  const eventoId = useSearchParams()[0].get('evento') ?? undefined
  const { ejecutar: cargar } = peligros
  useEffect(() => {
    void cargar()
  }, [cargar])

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Agregar riesgo</h1>
        <p className="text-text-secondary">
          Para los peligros que la encuesta no detectó. El riesgo queda en la matriz de la empresa, donde después se evalúa.
        </p>
      </header>

      {peligros.error ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar el catálogo de peligros</h2>
          <p className="text-text-secondary">{peligros.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargar()}>
            Reintentar
          </Boton>
        </section>
      ) : peligros.cargando || !peligros.datos ? (
        <div aria-busy="true" aria-label="Cargando el catálogo de peligros" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <Formulario peligros={peligros.datos} eventoId={eventoId} />
      )}
    </div>
  )
}

function Formulario({ peligros, eventoId }: { peligros: PeligroDisponible[]; eventoId?: string }) {
  const navigate = useNavigate()
  const hayCatalogo = peligros.length > 0
  const {
    register,
    control,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(nuevoRiesgoEsquema, {
    defaultValues: { origen: hayCatalogo ? 'catalogo' : 'descrito', peligroId: '', descripcionPeligro: '', actividad: '' },
    erroresPorCodigo: {
      'riesgos.peligro_invalido': { campo: 'peligroId', mensaje: 'Ese peligro ya no está disponible en el catálogo. Elige otro.' },
      'riesgos.duplicado': {
        campo: 'actividad',
        mensaje: eventoId
          ? 'Ese peligro ya está en la matriz con esta actividad. Vuelve al evento y reevalúalo.'
          : 'Ese peligro ya está en la matriz con esta actividad.',
      },
      'riesgos.evento_invalido': { mensaje: 'El evento ya no existe o se descartó, así que el riesgo no se puede enlazar con él.' },
    },
  })
  const origen = useWatch({ control, name: 'origen' })
  const { field: campoPeligro } = useController({ control, name: 'peligroId' })

  return (
    <form
      noValidate
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async ({ origen: o, peligroId, descripcionPeligro, actividad }) => {
        const riesgo = await crearRiesgo({
          ...(o === 'catalogo' ? { peligroId } : { descripcionPeligro: descripcionPeligro.trim() }),
          actividad,
          accidenteId: eventoId,
        })
        // Desde un evento se sigue a evaluarlo; si no, vuelve a la matriz.
        if (eventoId) navigate(`/riesgos/${riesgo.id}?evento=${eventoId}`)
        else navigate('/riesgos', { state: { agregado: nombrePeligro(riesgo) } satisfies EstadoRiesgoAgregado })
      })}
    >
      {eventoId && (
        <Aviso tipo="info" titulo="Riesgo revelado por un evento">
          Quedará enlazado con el evento y después podrás evaluarlo.
        </Aviso>
      )}
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}

      <GrupoRadio
        etiqueta="¿Cuál es el peligro?"
        required
        registro={register('origen')}
        error={errors.origen}
        opciones={[
          {
            valor: 'catalogo',
            texto: 'Lo elijo del catálogo',
            descripcion: hayCatalogo
              ? 'Los peligros que Norvia ya tiene identificados.'
              : 'Todavía no hay peligros en el catálogo.',
          },
          { valor: 'descrito', texto: 'Lo describo', descripcion: 'Si no está en el catálogo.' },
        ]}
      />

      {origen === 'catalogo' ? (
        <SelectBusqueda
          etiqueta="Peligro"
          required
          etiquetaBusqueda="Buscar peligro"
          opciones={peligros.map((p) => ({ valor: p.id, texto: p.nombre, descripcion: ETIQUETAS_CLASIFICACION[p.clasificacion] }))}
          valor={campoPeligro.value}
          onChange={campoPeligro.onChange}
          onBlur={campoPeligro.onBlur}
          error={errors.peligroId?.message}
        />
      ) : (
        <AreaTexto
          etiqueta="Descripción del peligro"
          required
          ayuda="Qué puede causar daño. Por ejemplo: Piso resbaloso junto a la cafetera."
          error={errors.descripcionPeligro?.message}
          {...register('descripcionPeligro')}
        />
      )}

      <CampoTexto
        etiqueta="Actividad relacionada"
        required
        autoComplete="off"
        registro={register('actividad')}
        error={errors.actividad}
        ayuda="Dónde o en qué tarea se presenta. Por ejemplo: Carga de camiones en bodega."
      />

      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
        <Link to={eventoId ? `/eventos/${eventoId}` : '/riesgos'}>Cancelar</Link>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Agregando…">
          Agregar a la matriz
        </Boton>
      </div>
    </form>
  )
}
