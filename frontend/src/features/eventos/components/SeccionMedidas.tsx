import { Plus, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useController } from 'react-hook-form'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoFecha } from '@/shared/components/ui/CampoFecha'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoCasillas } from '@/shared/components/ui/GrupoCasillas'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { Insignia } from '@/shared/components/ui/Insignia'
import { CuerpoModal, Modal, PieModal } from '@/shared/components/ui/Modal'
import { SelectBusqueda } from '@/shared/components/ui/SelectBusqueda'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { etiquetaRol } from '@/shared/sesion'
import { ESTADOS_TAREA, OPCIONES_PRIORIDAD, PRIORIDADES, tareaAsignadaEsquema, vencida } from '@/shared/tareas'
import { fechaCorta, hoyHonduras } from '@/shared/validacion'
import { crearMedida, listarMedidas, listarPersonas } from '../api/eventos'
import { fechaHora } from '../etiquetas'
import type { EventoDetalle, MedidaCorrectiva } from '../types/eventos'

interface Props {
  evento: EventoDetalle
  /** Responsable SST: agrega medidas mientras el evento siga abierto. El admin solo consulta. */
  puedeGestionar: boolean
  onCambio: (evento: EventoDetalle, mensaje: string) => void
}

/**
 * HU-050: medidas correctivas del evento. Cada una es una tarea del plan de trabajo con origen en el evento (RN-23);
 * se trabaja desde el plan y aquí se ve su avance. Para cerrar el evento hace falta al menos una (RN-28).
 */
export function SeccionMedidas({ evento, puedeGestionar, onCambio }: Props) {
  const peticion = usePeticion(listarMedidas)
  const cargandoVisible = useCargaVisible(peticion.cargando && !peticion.datos)
  const [agregando, setAgregando] = useState(false)
  const [estadoModal, setEstadoModal] = useState({ sucio: false, ocupado: false })
  const { ejecutar } = peticion
  const cargar = useCallback(() => void ejecutar(evento.id), [ejecutar, evento.id])
  useEffect(cargar, [cargar])

  if (evento.estado === 'descartado') return null
  const puedeAgregar = puedeGestionar && evento.estado !== 'cerrado'
  const medidas = peticion.datos

  return (
    <section aria-labelledby="titulo-medidas" className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div className="grid gap-1">
          <h2 id="titulo-medidas" className="text-md font-semibold">
            Medidas correctivas
          </h2>
          <p className="text-text-secondary">Lo que se hará para que no se repita. Cada medida es una tarea del plan de trabajo.</p>
        </div>
        {puedeAgregar && (
          <Boton variante="secundario" onClick={() => setAgregando(true)}>
            <Plus aria-hidden size={16} strokeWidth={1.5} />
            Agregar una medida
          </Boton>
        )}
      </header>

      {peticion.error && !medidas ? (
        <div className="flex flex-col items-center gap-3 py-6 text-center">
          <TriangleAlert aria-hidden size={24} strokeWidth={1.5} className="text-error-icon" />
          <p className="text-text-secondary">{`No se pudieron cargar las medidas. ${peticion.error.mensaje}`}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </div>
      ) : !medidas ? (
        <div aria-busy="true" aria-label="Cargando las medidas" className="grid gap-2">
          {cargandoVisible &&
            [0, 1].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : medidas.length === 0 ? (
        <p className="text-text-secondary">
          {puedeAgregar ? 'Todavía no hay medidas. Agrega al menos una para poder cerrar el evento.' : 'No tiene medidas correctivas.'}
        </p>
      ) : (
        <>
          <Avance medidas={medidas} />
          <ul className="grid divide-y divide-border-default border-y border-border-default">
            {medidas.map((m) => (
              <Medida key={m.id} medida={m} />
            ))}
          </ul>
        </>
      )}

      <Modal
        abierto={agregando}
        titulo="Agregar una medida correctiva"
        onCerrar={() => setAgregando(false)}
        hayCambios={estadoModal.sucio}
        ocupado={estadoModal.ocupado}
      >
        {agregando && (
          <FormularioMedida
            eventoId={evento.id}
            onCerrar={() => setAgregando(false)}
            onEstado={setEstadoModal}
            onCreada={(m) => {
              setAgregando(false)
              cargar()
              onCambio(
                { ...evento, totalMedidas: evento.totalMedidas + 1 },
                `Se agregó la medida «${m.titulo}». Ya está en el plan de trabajo, a cargo de ${m.responsableNombre ?? 'su responsable'}.`,
              )
            }}
          />
        )}
      </Modal>
    </section>
  )
}

/** "2 de 3 completadas" (las canceladas no cuentan) y cuántas están vencidas. */
function Avance({ medidas }: { medidas: MedidaCorrectiva[] }) {
  const vigentes = medidas.filter((m) => m.estado !== 'cancelada')
  const completadas = vigentes.filter((m) => m.estado === 'completada').length
  const vencidas = vigentes.filter(vencida).length
  const texto =
    vigentes.length === 0
      ? 'Todas las medidas están canceladas.'
      : `${completadas} de ${vigentes.length} ${vigentes.length === 1 ? 'completada' : 'completadas'}${
          vencidas > 0 ? ` · ${vencidas} ${vencidas === 1 ? 'vencida' : 'vencidas'}` : ''
        }`
  return <p className="text-sm font-medium">{texto}</p>
}

function Medida({ medida: m }: { medida: MedidaCorrectiva }) {
  const estado = ESTADOS_TAREA[m.estado]
  const detalle = [
    m.responsableNombre ?? 'Sin responsable',
    m.fechaLimite && `Fecha límite: ${fechaCorta(m.fechaLimite)}`,
    `Prioridad ${PRIORIDADES[m.prioridad].toLowerCase()}`,
  ].filter(Boolean)
  return (
    <li className="grid gap-1 py-3">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <span className={`wrap-break-word font-medium ${m.estado === 'cancelada' ? 'text-text-secondary line-through' : ''}`}>{m.titulo}</span>
        <span className="inline-flex flex-wrap gap-1">
          <Insignia tipo={estado.tipo}>{estado.texto}</Insignia>
          {vencida(m) && <Insignia tipo="error">Vencida</Insignia>}
        </span>
      </div>
      {m.descripcion && <p className="line-clamp-2 text-sm wrap-break-word text-text-secondary">{m.descripcion}</p>}
      <p className="text-xs text-text-secondary">{detalle.join(' · ')}</p>
      {m.estado === 'completada' && m.completadaEn && (
        <p className="text-xs text-text-secondary">{`Completada el ${fechaHora(m.completadaEn)}`}</p>
      )}
    </li>
  )
}

interface FormularioProps {
  eventoId: string
  onCerrar: () => void
  onCreada: (m: MedidaCorrectiva) => void
  onEstado: (e: { sucio: boolean; ocupado: boolean }) => void
}

/** Una medida nace asignada, como una tarea manual del plan: responsable y fecha límite no anterior a hoy. */
function FormularioMedida({ eventoId, onCerrar, onCreada, onEstado }: FormularioProps) {
  const personas = usePeticion(listarPersonas)
  const { ejecutar } = personas
  useEffect(() => {
    void ejecutar()
  }, [ejecutar])

  const {
    register,
    control,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(tareaAsignadaEsquema, {
    defaultValues: { titulo: '', descripcion: '', prioridad: '' as '1', responsableId: '', fechaLimite: '', requiereEvidencia: false },
    erroresPorCodigo: {
      'eventos.responsable_invalido': { campo: 'responsableId', mensaje: 'Esa persona ya no está activa en la empresa. Elige a otra.' },
      'eventos.fecha_pasada': { campo: 'fechaLimite', mensaje: 'La fecha límite no puede ser anterior a hoy.' },
      'eventos.terminado': { mensaje: 'El evento ya se cerró o se descartó: no recibe medidas.' },
    },
  })
  const { field: responsable } = useController({ control, name: 'responsableId' })
  const { field: fecha } = useController({ control, name: 'fechaLimite' })
  const { field: evidencia } = useController({ control, name: 'requiereEvidencia' })
  useEffect(() => onEstado({ sucio: isDirty, ocupado: isSubmitting }), [onEstado, isDirty, isSubmitting])

  return (
    <form noValidate onSubmit={enviar(async (datos) => onCreada(await crearMedida(eventoId, datos)))}>
      <CuerpoModal>
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        {personas.error && <Aviso tipo="error">{`No se pudo cargar a las personas de la empresa. ${personas.error.mensaje}`}</Aviso>}
        <CampoTexto
          etiqueta="Qué hay que hacer"
          required
          autoComplete="off"
          registro={register('titulo')}
          error={errors.titulo}
          ayuda="Por ejemplo: Señalizar el piso mojado de la bodega."
        />
        <AreaTexto etiqueta="Detalle" error={errors.descripcion?.message} ayuda="Opcional." {...register('descripcion')} />
        <GrupoRadio etiqueta="Prioridad" required registro={register('prioridad')} error={errors.prioridad} opciones={OPCIONES_PRIORIDAD} />
        <SelectBusqueda
          etiqueta="Responsable"
          required
          etiquetaBusqueda="Buscar persona"
          opciones={(personas.datos ?? []).map((p) => ({ valor: p.id, texto: p.nombreCompleto, descripcion: etiquetaRol(p.rolCodigo) }))}
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
            { valor: 'si', texto: 'Pide evidencia para completarla', descripcion: 'Quien la haga tendrá que adjuntar una foto o un PDF.' },
          ]}
          valor={evidencia.value ? ['si'] : []}
          onChange={(v) => evidencia.onChange(v.includes('si'))}
        />
      </CuerpoModal>
      <PieModal>
        <Boton variante="secundario" disabled={isSubmitting} onClick={onCerrar}>
          Cancelar
        </Boton>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Agregando…">
          Agregar la medida
        </Boton>
      </PieModal>
    </form>
  )
}
