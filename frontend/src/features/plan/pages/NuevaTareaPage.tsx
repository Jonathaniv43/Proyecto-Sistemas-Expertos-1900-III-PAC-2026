import { useEffect } from 'react'
import { useController } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { TriangleAlert } from 'lucide-react'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoFecha } from '@/shared/components/ui/CampoFecha'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoCasillas } from '@/shared/components/ui/GrupoCasillas'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { SelectBusqueda } from '@/shared/components/ui/SelectBusqueda'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { hoyHonduras } from '@/shared/validacion'
import { crearTareaManual, listarResponsables } from '../api/plan'
import { opcionesResponsable } from '../etiquetas'
import { tareaManualEsquema } from '../schemas/tareaManual'
import type { Responsable } from '../types/plan'
import type { EstadoPlanNavegacion } from './PlanTrabajoPage'

/**
 * HU-043: tarea manual, que no sale de un riesgo ni de un accidente (RN-23). Nace asignada: responsable (persona
 * activa de la empresa) y fecha límite, que no puede ser anterior a hoy.
 */
export function NuevaTareaPage() {
  const responsables = usePeticion(listarResponsables)
  const cargandoVisible = useCargaVisible(responsables.cargando)
  const { ejecutar: cargar } = responsables
  useEffect(() => {
    void cargar()
  }, [cargar])

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Nueva tarea</h1>
        <p className="text-text-secondary">
          Para lo que hay que hacer y no sale de un riesgo de la matriz. Queda en el plan de trabajo con su responsable.
        </p>
      </header>

      {responsables.error ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudo cargar a las personas de la empresa</h2>
          <p className="text-text-secondary">{responsables.error.mensaje}</p>
          <Boton variante="secundario" onClick={() => void cargar()}>
            Reintentar
          </Boton>
        </section>
      ) : !responsables.datos ? (
        <div aria-busy="true" aria-label="Cargando el formulario" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <Formulario responsables={responsables.datos} />
      )}
    </div>
  )
}

function Formulario({ responsables }: { responsables: Responsable[] }) {
  const navigate = useNavigate()
  const {
    register,
    control,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(tareaManualEsquema, {
    defaultValues: { titulo: '', descripcion: '', prioridad: '' as '1', responsableId: '', fechaLimite: '', requiereEvidencia: false },
    erroresPorCodigo: {
      'plan.responsable_invalido': { campo: 'responsableId', mensaje: 'Esa persona ya no está activa en la empresa. Elige a otra.' },
      'plan.fecha_pasada': { campo: 'fechaLimite', mensaje: 'La fecha límite no puede ser anterior a hoy.' },
    },
  })
  const { field: responsable } = useController({ control, name: 'responsableId' })
  const { field: fecha } = useController({ control, name: 'fechaLimite' })
  const { field: evidencia } = useController({ control, name: 'requiereEvidencia' })

  return (
    <form
      noValidate
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async (datos) => {
        const tarea = await crearTareaManual(datos)
        navigate('/plan-de-trabajo', { state: { creada: tarea.titulo } satisfies EstadoPlanNavegacion })
      })}
    >
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      <CampoTexto
        etiqueta="Qué hay que hacer"
        required
        autoComplete="off"
        registro={register('titulo')}
        error={errors.titulo}
        ayuda="Por ejemplo: Recargar los extintores de la bodega."
      />
      <AreaTexto etiqueta="Detalle" error={errors.descripcion?.message} ayuda="Opcional." {...register('descripcion')} />
      <GrupoRadio
        etiqueta="Prioridad"
        required
        registro={register('prioridad')}
        error={errors.prioridad}
        opciones={[
          { valor: '1', texto: 'Alta', descripcion: 'Hay que atenderla primero.' },
          { valor: '2', texto: 'Media' },
          { valor: '3', texto: 'Baja', descripcion: 'Puede esperar a las demás.' },
        ]}
      />
      <SelectBusqueda
        etiqueta="Responsable"
        required
        etiquetaBusqueda="Buscar persona"
        opciones={opcionesResponsable(responsables)}
        valor={responsable.value}
        onChange={responsable.onChange}
        onBlur={responsable.onBlur}
        error={errors.responsableId?.message}
        ayuda="Personas activas de la empresa."
      />
      <CampoFecha
        etiqueta="Fecha límite"
        required
        valor={fecha.value}
        onChange={fecha.onChange}
        onBlur={fecha.onBlur}
        error={errors.fechaLimite?.message}
        minimo={hoyHonduras()}
      />
      <GrupoCasillas
        etiqueta="Evidencia"
        opciones={[
          {
            valor: 'si',
            texto: 'Pide evidencia para completarla',
            descripcion: 'Quien la haga tendrá que adjuntar una foto o un PDF.',
          },
        ]}
        valor={evidencia.value ? ['si'] : []}
        onChange={(v) => evidencia.onChange(v.includes('si'))}
      />
      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
        <Link to="/plan-de-trabajo">Cancelar</Link>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Creando…">
          Crear tarea
        </Boton>
      </div>
    </form>
  )
}
