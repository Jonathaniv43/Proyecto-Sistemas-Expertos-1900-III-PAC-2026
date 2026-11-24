import { Plus, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { extraerError } from '@/shared/api/errores'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { crearControl, listarControles, listarControlesSugeridos } from '../api/riesgos'
import { faltaControl, NOMBRE_TIPO_CONTROL, ordenarControles, TIPOS_CONTROL } from '../controles'
import { ETIQUETAS_CLASIFICACION_RIESGO } from '../niveles'
import { controlEsquema } from '../schemas/control'
import type { Control, ControlSugerido, CrearControlRequest, Riesgo, TipoControl } from '../types/riesgos'

/** Texto con que una sugerencia pasa a ser control: el título y, si la plantilla la tiene, su descripción. */
const textoSugerencia = (s: ControlSugerido) => (s.descripcion ? `${s.titulo.replace(/\.$/, '')}. ${s.descripcion}` : s.titulo)

/** Lo que se está editando: una sugerencia (por su plantilla) o un control propio. */
type Edicion = { plantillaId: string } | 'propio' | null

/**
 * HU-039: controles de un riesgo de la matriz.
 * - Norvia sugiere los de las plantillas del peligro, del más efectivo al menos efectivo (RN-21). Cada
 *   sugerencia se acepta tal cual, se edita antes de aceptarla o se descarta; ninguna se guarda sin
 *   aceptarla (RN-76). Descartar solo la quita de la vista mientras el riesgo está abierto.
 * - También se agrega un control propio, con su tipo y descripción.
 * - Un riesgo alto o crítico sin controles se señala (RN-19).
 */
export function SeccionControles({ riesgo, onCambio }: { riesgo: Riesgo; onCambio: (totalControles: number) => void }) {
  const peticionControles = usePeticion(listarControles)
  const peticionSugeridos = usePeticion(listarControlesSugeridos)
  const cargandoVisible = useCargaVisible(peticionControles.cargando)
  const [controles, setControles] = useState<Control[]>()
  const [sugeridos, setSugeridos] = useState<ControlSugerido[]>([])
  const [descartados, setDescartados] = useState<string[]>([])
  const [edicion, setEdicion] = useState<Edicion>(null)
  const [aceptando, setAceptando] = useState<string>()
  const [errorSugerencia, setErrorSugerencia] = useState<string>()
  const [agregado, setAgregado] = useState<string>()

  const { ejecutar: ejecutarControles } = peticionControles
  const { ejecutar: ejecutarSugeridos } = peticionSugeridos
  const cargar = useCallback(() => {
    void ejecutarControles(riesgo.id).then((d) => d && setControles(d))
    void ejecutarSugeridos(riesgo.id).then((d) => d && setSugeridos(d))
  }, [ejecutarControles, ejecutarSugeridos, riesgo.id])
  useEffect(cargar, [cargar])

  const agregar = (control: Control) => {
    const lista = ordenarControles([...(controles ?? []), control])
    setControles(lista)
    if (control.plantillaId) setSugeridos((s) => s.filter((x) => x.plantillaId !== control.plantillaId))
    setEdicion(null)
    setErrorSugerencia(undefined)
    setAgregado(`Control agregado: ${NOMBRE_TIPO_CONTROL[control.tipo].toLowerCase()}.`)
    onCambio(lista.length)
  }

  const aceptar = async (s: ControlSugerido) => {
    setAceptando(s.plantillaId)
    setAgregado(undefined)
    try {
      agregar(await crearControl(riesgo.id, { tipo: s.tipo, descripcion: textoSugerencia(s), plantillaId: s.plantillaId }))
    } catch (causa) {
      const error = extraerError(causa)
      setErrorSugerencia(
        error.codigo === 'riesgos.sugerencia_aceptada' ? 'Esa sugerencia ya era un control de este riesgo. Actualizamos la lista.' : error.mensaje,
      )
      if (error.codigo === 'riesgos.sugerencia_aceptada' || error.codigo === 'riesgos.sugerencia_invalida') cargar()
    } finally {
      setAceptando(undefined)
    }
  }

  const visibles = sugeridos.filter((s) => !descartados.includes(s.plantillaId))
  const sinControl = controles !== undefined && faltaControl({ ...riesgo, totalControles: controles.length })

  return (
    <section
      aria-labelledby="titulo-controles"
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
    >
      <header className="grid gap-1">
        <h2 id="titulo-controles" className="text-md font-semibold">
          Controles
        </h2>
        <p className="text-text-secondary">
          Medidas para reducir el riesgo. Las más efectivas eliminan o sustituyen el peligro; el equipo de protección
          personal es la última opción.
        </p>
      </header>

      <div role="status" className="contents">
        {agregado && <Aviso tipo="exito">{agregado}</Aviso>}
      </div>

      {sinControl && riesgo.evaluacion && (
        <Aviso tipo="advertencia" titulo="Este riesgo necesita al menos un control">
          {`Es ${ETIQUETAS_CLASIFICACION_RIESGO[riesgo.evaluacion.clasificacion].toLowerCase()}: acepta una sugerencia o agrega un control propio.`}
        </Aviso>
      )}

      {peticionControles.error && !controles ? (
        <div className="flex flex-col items-start gap-3">
          <p className="inline-flex items-center gap-2">
            <TriangleAlert aria-hidden size={16} strokeWidth={1.5} className="text-error-icon" />
            No se pudieron cargar los controles. {peticionControles.error.mensaje}
          </p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </div>
      ) : !controles ? (
        <div aria-busy="true" aria-label="Cargando los controles" className="grid gap-2">
          {cargandoVisible &&
            [0, 1].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : (
        <>
          {controles.length === 0 ? (
            <p className="text-text-secondary">Todavía no tiene controles.</p>
          ) : (
            <ul aria-label="Controles del riesgo" className="grid divide-y divide-border-default border-y border-border-default">
              {controles.map((c) => (
                <li key={c.id} className="grid gap-1 py-3">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="text-xs font-medium text-text-secondary">{NOMBRE_TIPO_CONTROL[c.tipo]}</span>
                    <Insignia tipo={c.estado === 'implementado' ? 'exito' : 'neutro'}>
                      {c.estado === 'implementado' ? 'Implementado' : 'Propuesto'}
                    </Insignia>
                  </div>
                  <p className="wrap-break-word whitespace-pre-line">{c.descripcion}</p>
                </li>
              ))}
            </ul>
          )}

          {(visibles.length > 0 || descartados.length > 0) && (
            <section aria-labelledby="titulo-sugeridos" className="grid gap-3">
              <div className="grid gap-1">
                <h3 id="titulo-sugeridos" className="font-semibold">
                  Sugeridos por Norvia
                </h3>
                <p className="text-sm text-text-secondary">
                  Según el peligro, del más efectivo al menos efectivo. Ninguno se guarda hasta que lo aceptes.
                </p>
              </div>
              {errorSugerencia && <Aviso tipo="error">{errorSugerencia}</Aviso>}
              <ul aria-label="Controles sugeridos" className="grid gap-3">
                {visibles.map((s) => (
                  <li key={s.plantillaId} className="grid gap-3 rounded-md border border-border-default p-4">
                    {edicion !== null && edicion !== 'propio' && edicion.plantillaId === s.plantillaId ? (
                      <FormularioControl
                        riesgoId={riesgo.id}
                        titulo={`Editar la sugerencia «${s.titulo}»`}
                        iniciales={{ tipo: s.tipo, descripcion: textoSugerencia(s), plantillaId: s.plantillaId }}
                        textoEnviar="Aceptar con cambios"
                        onGuardado={agregar}
                        onCancelar={() => setEdicion(null)}
                        onRecargar={cargar}
                      />
                    ) : (
                      <>
                        <div className="grid gap-1">
                          <span className="text-xs font-medium text-text-secondary">{NOMBRE_TIPO_CONTROL[s.tipo]}</span>
                          <p className="font-medium">{s.titulo}</p>
                          {s.descripcion && <p className="wrap-break-word text-text-secondary">{s.descripcion}</p>}
                        </div>
                        <div className="flex flex-wrap items-center gap-2">
                          <Boton
                            variante="secundario"
                            cargando={aceptando === s.plantillaId}
                            textoCargando="Aceptando…"
                            disabled={aceptando !== undefined && aceptando !== s.plantillaId}
                            aria-label={`Aceptar «${s.titulo}»`}
                            onClick={() => void aceptar(s)}
                          >
                            Aceptar
                          </Boton>
                          <Boton
                            variante="secundario"
                            disabled={aceptando !== undefined}
                            aria-label={`Editar «${s.titulo}» antes de aceptarla`}
                            onClick={() => {
                              setAgregado(undefined)
                              setEdicion({ plantillaId: s.plantillaId })
                            }}
                          >
                            Editar
                          </Boton>
                          <button
                            type="button"
                            disabled={aceptando !== undefined}
                            aria-label={`Descartar «${s.titulo}»`}
                            className="inline-flex min-h-control-touch items-center px-2 text-sm font-medium text-text-link hover:text-text-link-hover disabled:text-text-disabled sm:min-h-control"
                            onClick={() => setDescartados((d) => [...d, s.plantillaId])}
                          >
                            Descartar
                          </button>
                        </div>
                      </>
                    )}
                  </li>
                ))}
              </ul>
              {descartados.length > 0 && (
                <p className="text-sm text-text-secondary">
                  {descartados.length === 1 ? '1 sugerencia descartada' : `${descartados.length} sugerencias descartadas`}
                  {' · '}
                  <button
                    type="button"
                    className="font-medium text-text-link hover:text-text-link-hover"
                    onClick={() => setDescartados([])}
                  >
                    Mostrarlas
                  </button>
                </p>
              )}
            </section>
          )}

          {edicion === 'propio' ? (
            <div className="grid gap-3 rounded-md border border-border-default p-4">
              <FormularioControl
                riesgoId={riesgo.id}
                titulo="Control propio"
                textoEnviar="Agregar control"
                onGuardado={agregar}
                onCancelar={() => setEdicion(null)}
                onRecargar={cargar}
              />
            </div>
          ) : (
            <div>
              <Boton
                variante="secundario"
                onClick={() => {
                  setAgregado(undefined)
                  setEdicion('propio')
                }}
              >
                <Plus aria-hidden size={16} strokeWidth={1.5} />
                Agregar control propio
              </Boton>
            </div>
          )}
        </>
      )}
    </section>
  )
}

interface FormularioControlProps {
  riesgoId: string
  titulo: string
  iniciales?: CrearControlRequest
  textoEnviar: string
  onGuardado: (control: Control) => void
  onCancelar: () => void
  /** La sugerencia ya no está disponible o ya se aceptó: hay que volver a pedir las listas. */
  onRecargar: () => void
}

function FormularioControl({ riesgoId, titulo, iniciales, textoEnviar, onGuardado, onCancelar, onRecargar }: FormularioControlProps) {
  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(controlEsquema, {
    defaultValues: { tipo: (iniciales?.tipo ?? '') as TipoControl, descripcion: iniciales?.descripcion ?? '' },
    erroresPorCodigo: {
      'riesgos.sugerencia_aceptada': { mensaje: 'Esa sugerencia ya es un control de este riesgo.' },
      'riesgos.sugerencia_invalida': { mensaje: 'Esa sugerencia ya no está disponible. Puedes agregarlo como control propio.' },
      'riesgos.no_en_matriz': { mensaje: 'Este riesgo ya no está en la matriz. Vuelve a la matriz para ver su estado.' },
    },
  })

  return (
    <form
      noValidate
      aria-label={titulo}
      className="grid gap-4"
      onSubmit={enviar(async (datos) => {
        try {
          onGuardado(await crearControl(riesgoId, { ...datos, plantillaId: iniciales?.plantillaId }))
        } catch (causa) {
          const codigo = extraerError(causa).codigo
          if (codigo === 'riesgos.sugerencia_aceptada' || codigo === 'riesgos.sugerencia_invalida') onRecargar()
          throw causa
        }
      })}
    >
      <p className="font-medium">{titulo}</p>
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      <GrupoRadio
        etiqueta="Tipo de control"
        required
        registro={register('tipo')}
        error={errors.tipo}
        opciones={TIPOS_CONTROL.map((t) => ({ valor: t.valor, texto: t.nombre, descripcion: t.explicacion }))}
      />
      <AreaTexto
        etiqueta="Descripción"
        required
        ayuda="Qué se va a hacer. Por ejemplo: Instalar barandas en la escalera de la bodega."
        error={errors.descripcion?.message}
        {...register('descripcion')}
      />
      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-end">
        <Boton variante="secundario" onClick={onCancelar} disabled={isSubmitting}>
          Cancelar
        </Boton>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Guardando…">
          {textoEnviar}
        </Boton>
      </div>
    </form>
  )
}
