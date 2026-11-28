import { CircleCheck } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useController, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoFecha } from '@/shared/components/ui/CampoFecha'
import { CampoHora } from '@/shared/components/ui/CampoHora'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { GrupoCasillas } from '@/shared/components/ui/GrupoCasillas'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { useFormulario } from '@/shared/forms'
import { hoyHonduras } from '@/shared/validacion'
import { reportarEvento } from '../api/eventos'
import { ESTADOS_ENFERMEDAD, fechaHora, NOMBRE_TIPO, TIPOS_EVENTO } from '../etiquetas'
import { reporteEsquema } from '../schemas/reporte'
import type { Evento } from '../types/eventos'

/**
 * HU-047: cualquier persona con acceso a la empresa (también los encuestados) reporta un accidente, un incidente, una
 * enfermedad profesional o una condición insegura. Es anónimo (RN-27, D11): Norvia guarda lo que se escribe y cuándo
 * llegó, pero no quién lo envió. Adjuntar fotos o PDF llega con el almacenamiento de archivos (HU-100).
 */
export function ReportarEventoPage() {
  const [enviado, setEnviado] = useState<Evento>()
  const [vez, setVez] = useState(0)

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Reportar un evento</h1>
        <p className="text-text-secondary">
          Avisa de un accidente, de algo que estuvo a punto de pasar o de una condición peligrosa, para que se atienda.
        </p>
      </header>

      {enviado ? (
        <section
          role="status"
          className="flex flex-col items-center gap-3 rounded-lg border border-border-default bg-bg-surface px-4 py-12 text-center"
        >
          <CircleCheck aria-hidden size={32} strokeWidth={1.5} className="text-success-icon" />
          <h2 className="text-md font-semibold">Reporte enviado</h2>
          <p className="max-w-layout-narrow text-text-secondary">
            {`Se recibió tu reporte de ${NOMBRE_TIPO[enviado.tipo].toLowerCase()} el ${fechaHora(enviado.registradoEn)}. No se guardó quién lo envió. Gracias por avisar.`}
          </p>
          <div className="mt-2 flex flex-col gap-3 sm:flex-row">
            <Boton
              variante="secundario"
              onClick={() => {
                setEnviado(undefined)
                setVez((v) => v + 1)
              }}
            >
              Reportar otro evento
            </Boton>
            <Link to="/inicio" className={clasesBoton('secundario')}>
              Volver al inicio
            </Link>
          </div>
        </section>
      ) : (
        <Formulario key={vez} onEnviado={setEnviado} />
      )}
    </div>
  )
}

function Formulario({ onEnviado }: { onEnviado: (e: Evento) => void }) {
  const {
    register,
    control,
    trigger,
    getValues,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(reporteEsquema, {
    defaultValues: {
      tipo: '' as 'accidente',
      estadoEnfermedad: '',
      fecha: '',
      hora: '',
      lugar: '',
      descripcion: '',
      esGrave: false,
    },
    erroresPorCodigo: { 'eventos.fecha_futura': { campo: 'hora', mensaje: 'La fecha y hora no pueden ser futuras.' } },
  })
  const tipo = useWatch({ control, name: 'tipo' })
  const { field: fecha } = useController({ control, name: 'fecha' })
  const { field: hora } = useController({ control, name: 'hora' })
  const { field: grave } = useController({ control, name: 'esGrave' })

  // "Esa hora todavía no llega" depende de la fecha: al cambiarla se revisa de nuevo la hora ya escrita.
  useEffect(() => {
    if (getValues('hora')) void trigger('hora')
  }, [fecha.value, getValues, trigger])

  return (
    <form
      noValidate
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async (datos) => onEnviado(await reportarEvento(datos)))}
    >
      <Aviso tipo="info" titulo="Tu reporte es anónimo">
        Norvia guarda lo que escribas y cuándo llegó, pero no quién lo envió. Si no quieres que se sepa, no pongas tu nombre
        en la descripción.
      </Aviso>
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}

      <GrupoRadio etiqueta="¿Qué pasó?" required registro={register('tipo')} error={errors.tipo} opciones={TIPOS_EVENTO} />
      {tipo === 'enfermedad_profesional' && (
        <GrupoRadio
          etiqueta="La enfermedad es"
          required
          registro={register('estadoEnfermedad')}
          error={errors.estadoEnfermedad}
          opciones={ESTADOS_ENFERMEDAD}
        />
      )}

      <div className="grid gap-6 sm:grid-cols-2">
        <CampoFecha
          etiqueta="Fecha"
          required
          valor={fecha.value}
          onChange={fecha.onChange}
          onBlur={fecha.onBlur}
          error={errors.fecha?.message}
          maximo={hoyHonduras()}
        />
        <CampoHora
          etiqueta="Hora"
          required
          valor={hora.value}
          onChange={hora.onChange}
          onBlur={hora.onBlur}
          error={errors.hora?.message}
          ayuda="Aproximada. Por ejemplo: 2:30 pm."
        />
      </div>

      <CampoTexto
        etiqueta="Lugar"
        autoComplete="off"
        registro={register('lugar')}
        error={errors.lugar}
        ayuda="Opcional. Por ejemplo: bodega, junto a la puerta de carga."
      />
      <AreaTexto
        etiqueta="¿Qué ocurrió?"
        required
        error={errors.descripcion?.message}
        ayuda="Cuenta qué pasó y, si hubo lesionados, qué les pasó."
        {...register('descripcion')}
      />
      <GrupoCasillas
        etiqueta="Gravedad"
        opciones={[{ valor: 'si', texto: 'Es grave', descripcion: 'Hubo una lesión seria o pudo haber una muerte.' }]}
        valor={grave.value ? ['si'] : []}
        onChange={(v) => grave.onChange(v.includes('si'))}
      />

      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
        <Link to="/inicio">Cancelar</Link>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Enviando…">
          Enviar reporte
        </Boton>
      </div>
    </form>
  )
}
