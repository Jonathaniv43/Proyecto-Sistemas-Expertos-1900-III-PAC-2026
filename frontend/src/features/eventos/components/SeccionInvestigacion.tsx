import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useController } from 'react-hook-form'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoFecha } from '@/shared/components/ui/CampoFecha'
import { SelectBusqueda } from '@/shared/components/ui/SelectBusqueda'
import { useFormulario } from '@/shared/forms'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { etiquetaRol, useSesion } from '@/shared/sesion'
import { fechaCorta, hoyHonduras, isoATexto } from '@/shared/validacion'
import { listarPersonas, registrarInvestigacion } from '../api/eventos'
import { investigacionEsquema } from '../schemas/investigacion'
import type { EventoDetalle } from '../types/eventos'

/** Fecha (aaaa-mm-dd) en Honduras de un instante ISO. */
const fechaHonduras = (iso: string) =>
  new Intl.DateTimeFormat('en-CA', { timeZone: 'America/Tegucigalpa', year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date(iso))

interface Props {
  evento: EventoDetalle
  /** Responsable SST: registra la investigación. El admin solo consulta. */
  puedeGestionar: boolean
  onCambio: (evento: EventoDetalle, mensaje: string) => void
}

/**
 * HU-049: la investigación registra causas inmediatas y de fondo, conclusiones, quién investigó y cuándo
 * (art. 46.1.a); guardarla deja el evento "en investigación" y se puede corregir mientras siga abierto.
 * El cierre y el descarte están en `SeccionCierre`.
 */
export function SeccionInvestigacion({ evento, puedeGestionar, onCambio }: Props) {
  const [editando, setEditando] = useState(false)
  const terminado = evento.estado === 'cerrado' || evento.estado === 'descartado'
  const inv = evento.investigacion

  if (evento.estado === 'descartado') return null

  return (
    <section aria-labelledby="titulo-investigacion" className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <header className="grid gap-1">
        <h2 id="titulo-investigacion" className="text-md font-semibold">
          Investigación
        </h2>
        <p className="text-text-secondary">Por qué ocurrió: las causas inmediatas y las de fondo, para que no se repita.</p>
      </header>
      {editando ? (
        <Formulario
          evento={evento}
          onCancelar={() => setEditando(false)}
          onGuardado={(e) => {
            setEditando(false)
            onCambio(e, 'La investigación quedó guardada.')
          }}
        />
      ) : inv ? (
        <>
          <dl className="grid gap-4">
            <Dato etiqueta="Causas inmediatas">{inv.causasInmediatas}</Dato>
            <Dato etiqueta="Causas de fondo">{inv.causasBasicas}</Dato>
            {inv.conclusiones && <Dato etiqueta="Conclusiones">{inv.conclusiones}</Dato>}
            <Dato etiqueta="Investigó">{`${inv.investigadaPorNombre}, el ${fechaCorta(inv.fechaInvestigacion)}`}</Dato>
          </dl>
          {puedeGestionar && !terminado && (
            <div>
              <Boton variante="secundario" onClick={() => setEditando(true)}>
                Editar la investigación
              </Boton>
            </div>
          )}
        </>
      ) : (
        <>
          <p className="text-text-secondary">Todavía no se investiga.</p>
          {puedeGestionar && !terminado && (
            <div>
              <Boton variante="secundario" onClick={() => setEditando(true)}>
                Registrar la investigación
              </Boton>
            </div>
          )}
        </>
      )}
    </section>
  )
}

function Dato({ etiqueta, children }: { etiqueta: string; children: ReactNode }) {
  return (
    <div className="grid gap-1">
      <dt className="text-xs text-text-secondary">{etiqueta}</dt>
      <dd className="wrap-break-word whitespace-pre-line">{children}</dd>
    </div>
  )
}

interface FormularioProps {
  evento: EventoDetalle
  onCancelar: () => void
  onGuardado: (e: EventoDetalle) => void
}

function Formulario({ evento, onCancelar, onGuardado }: FormularioProps) {
  const sesion = useSesion()
  const yo = sesion.estado === 'activa' ? sesion.usuario.vinculoId : null
  const personas = usePeticion(listarPersonas)
  const { ejecutar } = personas
  useEffect(() => {
    void ejecutar()
  }, [ejecutar])

  const desde = fechaHonduras(evento.fechaHoraEvento)
  const hoy = hoyHonduras()
  const esquema = useMemo(() => investigacionEsquema(desde, hoy), [desde, hoy])
  const inv = evento.investigacion
  const {
    register,
    control,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(esquema, {
    defaultValues: {
      causasInmediatas: inv?.causasInmediatas ?? '',
      causasBasicas: inv?.causasBasicas ?? '',
      conclusiones: inv?.conclusiones ?? '',
      investigadaPorId: inv?.investigadaPorId ?? yo ?? '',
      fechaInvestigacion: isoATexto(inv?.fechaInvestigacion ?? hoy),
    },
    erroresPorCodigo: {
      'eventos.investigador_invalido': { campo: 'investigadaPorId', mensaje: 'Esa persona ya no está activa en la empresa. Elige a otra.' },
      'eventos.fecha_investigacion_invalida': { campo: 'fechaInvestigacion', mensaje: 'No puede ser futura ni anterior al evento.' },
      'eventos.terminado': { mensaje: 'El evento ya se cerró o se descartó.' },
    },
  })
  const { field: investigador } = useController({ control, name: 'investigadaPorId' })
  const { field: fecha } = useController({ control, name: 'fechaInvestigacion' })

  return (
    <form
      noValidate
      aria-label="Investigación del evento"
      className="grid gap-6"
      onSubmit={enviar(async (d) =>
        onGuardado(await registrarInvestigacion(evento.id, { ...d, conclusiones: d.conclusiones || undefined })),
      )}
    >
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      <AreaTexto
        etiqueta="Causas inmediatas"
        required
        ayuda="Lo que ocurrió justo antes: un acto o una condición. Por ejemplo: piso mojado sin señalizar."
        error={errors.causasInmediatas?.message}
        {...register('causasInmediatas')}
      />
      <AreaTexto
        etiqueta="Causas de fondo"
        required
        ayuda="Por qué existía esa condición. Por ejemplo: no hay un procedimiento de limpieza."
        error={errors.causasBasicas?.message}
        {...register('causasBasicas')}
      />
      <AreaTexto etiqueta="Conclusiones" ayuda="Opcional." error={errors.conclusiones?.message} {...register('conclusiones')} />
      <div className="grid gap-6 sm:grid-cols-2">
        <SelectBusqueda
          etiqueta="Investigó"
          required
          etiquetaBusqueda="Buscar persona"
          opciones={(personas.datos ?? []).map((p) => ({ valor: p.id, texto: p.nombreCompleto, descripcion: etiquetaRol(p.rolCodigo) }))}
          valor={investigador.value}
          onChange={investigador.onChange}
          onBlur={investigador.onBlur}
          error={errors.investigadaPorId?.message}
        />
        <CampoFecha
          etiqueta="Fecha de la investigación"
          required
          valor={fecha.value}
          onChange={fecha.onChange}
          onBlur={fecha.onBlur}
          error={errors.fechaInvestigacion?.message}
          minimo={desde}
          maximo={hoy}
        />
      </div>
      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-end">
        <Boton variante="secundario" disabled={isSubmitting} onClick={onCancelar}>
          Cancelar
        </Boton>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Guardando…">
          Guardar la investigación
        </Boton>
      </div>
    </form>
  )
}
