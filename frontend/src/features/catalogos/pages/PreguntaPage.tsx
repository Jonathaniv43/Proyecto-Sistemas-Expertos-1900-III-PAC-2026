import { ArrowDown, ArrowLeft, ArrowUp, Check, Plus, X } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { useController, useFieldArray, useWatch } from 'react-hook-form'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { BotonIcono } from '@/shared/components/ui/BotonIcono'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoCasillas } from '@/shared/components/ui/GrupoCasillas'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { SelectBusqueda } from '@/shared/components/ui/SelectBusqueda'
import { useFormulario } from '@/shared/forms'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { crearPregunta, editarPregunta, listarCategorias, listarPeligros, listarPreguntas, listarRubros } from '../api/catalogos'
import { EstadoCarga } from '../components/EstadoCarga'
import { CLASIFICACIONES, TIPOS_RESPUESTA } from '../etiquetas'
import { preguntaEsquema } from '../schemas/catalogos'
import type { CategoriaRiesgo, Peligro, Pregunta, Rubro } from '../types/catalogos'
import { opcionesCategoria } from '../opciones'
import type { EstadoPreguntaGuardada } from './PreguntasPage'

const SIN_PELIGRO = '__ninguno'

/**
 * HU-023: crear o editar una pregunta exploratoria. El tipo decide si lleva opciones (D26); si detecta un peligro,
 * se marca qué opciones indican exposición (HU-033). Las mismas reglas que el backend (RN-68), en el esquema Zod.
 */
export function PreguntaPage() {
  const { id } = useParams()
  const [params] = useSearchParams()
  const rubros = usePeticion(listarRubros)
  const categorias = usePeticion(listarCategorias)
  const peligros = usePeticion(listarPeligros)
  const preguntas = usePeticion(listarPreguntas)

  const { ejecutar: cargarRubros } = rubros
  const { ejecutar: cargarCategorias } = categorias
  const { ejecutar: cargarPeligros } = peligros
  const { ejecutar: cargarPreguntas } = preguntas
  const cargar = useCallback(() => {
    void cargarRubros()
    void cargarCategorias()
    void cargarPeligros()
    if (id) void cargarPreguntas()
  }, [cargarRubros, cargarCategorias, cargarPeligros, cargarPreguntas, id])
  useEffect(cargar, [cargar])

  const pregunta = id ? preguntas.datos?.find((p) => p.id === id) : undefined
  if (id && preguntas.datos && !pregunta) return <NotFoundPage />
  const listo = !!rubros.datos && !!categorias.datos && !!peligros.datos && (!id || !!pregunta)
  const volverA = pregunta?.categoriaRiesgoId ?? params.get('categoria') ?? ''

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <div>
        <Link to={`/catalogos/preguntas${volverA ? `?categoria=${volverA}` : ''}`} className="inline-flex items-center gap-1 text-sm">
          <ArrowLeft aria-hidden size={16} strokeWidth={1.5} />
          Volver a las preguntas
        </Link>
      </div>
      <header className="grid gap-1">
        <h2 className="text-lg font-semibold">{id ? 'Editar pregunta' : 'Nueva pregunta'}</h2>
        <p className="text-text-secondary">Los cambios no alteran las encuestas ya generadas: guardan su propia copia de las preguntas.</p>
      </header>
      <EstadoCarga
        cargando={!listo}
        error={rubros.error ?? categorias.error ?? peligros.error ?? preguntas.error}
        hayDatos={listo}
        que="la pregunta"
        onReintentar={cargar}
      >
        {listo && (
          <Formulario
            pregunta={pregunta}
            categoriaInicial={params.get('categoria') ?? ''}
            rubros={rubros.datos!}
            categorias={categorias.datos!}
            peligros={peligros.datos!}
          />
        )}
      </EstadoCarga>
    </div>
  )
}

interface FormularioProps {
  pregunta: Pregunta | undefined
  categoriaInicial: string
  rubros: Rubro[]
  categorias: CategoriaRiesgo[]
  peligros: Peligro[]
}

function Formulario({ pregunta, categoriaInicial, rubros, categorias, peligros }: FormularioProps) {
  const navigate = useNavigate()
  const {
    register,
    control,
    formState: { errors, isSubmitting, dirtyFields },
    enviar,
    errorGeneral,
  } = useFormulario(preguntaEsquema, {
    defaultValues: {
      categoriaRiesgoId: pregunta?.categoriaRiesgoId ?? categoriaInicial,
      texto: pregunta?.texto ?? '',
      tipoRespuestaCodigo: pregunta?.tipoRespuestaCodigo ?? ('' as 'seleccion_unica'),
      obligatoria: pregunta?.obligatoria ?? true,
      peligroId: pregunta?.peligroId ?? '',
      opciones: pregunta?.opciones.map((o) => ({ texto: o.texto, indicaExposicion: o.indicaExposicion })) ?? [
        { texto: '', indicaExposicion: false },
        { texto: '', indicaExposicion: false },
      ],
    },
    erroresPorCodigo: {
      'catalogos.categoria_invalida': { campo: 'categoriaRiesgoId', mensaje: 'Esa categoría está desactivada. Elige otra.' },
      'catalogos.peligro_invalido': { campo: 'peligroId', mensaje: 'Ese peligro está desactivado. Elige otro.' },
    },
  })
  const opciones = useFieldArray({ control, name: 'opciones' })
  const tipo = useWatch({ control, name: 'tipoRespuestaCodigo' })
  const peligroId = useWatch({ control, name: 'peligroId' })
  const { field: peligro } = useController({ control, name: 'peligroId' })
  const { field: obligatoria } = useController({ control, name: 'obligatoria' })
  const conOpciones = tipo === 'seleccion_unica' || tipo === 'seleccion_multiple'
  const errorOpciones = (errors.opciones as { message?: string; root?: { message?: string } } | undefined)
  const mensajeOpciones = errorOpciones?.root?.message ?? errorOpciones?.message

  const opcionesPeligro = [
    { valor: SIN_PELIGRO, texto: 'Ninguno', descripcion: 'La pregunta no sugiere peligros.' },
    ...peligros
      .filter((p) => p.activo || p.id === pregunta?.peligroId)
      .map((p) => ({ valor: p.id, texto: p.nombre, descripcion: CLASIFICACIONES[p.clasificacion] })),
  ]

  return (
    <form
      noValidate
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async (d) => {
        const bloque = {
          tipoRespuestaCodigo: d.tipoRespuestaCodigo,
          peligroId: d.tipoRespuestaCodigo === 'respuesta_breve' || !d.peligroId ? undefined : d.peligroId,
          opciones:
            d.tipoRespuestaCodigo === 'respuesta_breve'
              ? []
              : d.opciones.map((o, i) => ({ texto: o.texto.trim(), orden: i + 1, indicaExposicion: !!d.peligroId && o.indicaExposicion })),
        }
        let guardada: Pregunta
        if (!pregunta) guardada = await crearPregunta({ categoriaRiesgoId: d.categoriaRiesgoId, texto: d.texto, obligatoria: d.obligatoria, ...bloque })
        else {
          // El bloque (tipo, peligro y opciones) solo se envía si cambió: así las opciones conservan su id.
          const cambiaBloque = !!(dirtyFields.tipoRespuestaCodigo || dirtyFields.peligroId || dirtyFields.opciones)
          guardada = await editarPregunta(pregunta.id, {
            texto: d.texto,
            obligatoria: d.obligatoria,
            categoriaRiesgoId: d.categoriaRiesgoId !== pregunta.categoriaRiesgoId ? d.categoriaRiesgoId : undefined,
            ...(cambiaBloque ? bloque : {}),
          })
        }
        navigate(`/catalogos/preguntas?categoria=${guardada.categoriaRiesgoId}`, {
          state: { guardada: guardada.texto } satisfies EstadoPreguntaGuardada,
        })
      })}
    >
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      <CampoSelect
        etiqueta="Categoría"
        required
        registro={register('categoriaRiesgoId')}
        error={errors.categoriaRiesgoId}
        opciones={opcionesCategoria(
          categorias.filter((c) => c.activa || c.id === pregunta?.categoriaRiesgoId),
          rubros,
        )}
        ayuda={pregunta ? 'Si la cambias, la pregunta pasa al final de la otra categoría.' : 'La pregunta se agrega al final de la categoría.'}
      />
      <AreaTexto etiqueta="Pregunta" required error={errors.texto?.message} ayuda="Como la leerá quien responde la encuesta." {...register('texto')} />
      <GrupoRadio
        etiqueta="Tipo de respuesta"
        required
        registro={register('tipoRespuestaCodigo')}
        error={errors.tipoRespuestaCodigo}
        opciones={TIPOS_RESPUESTA.map((t) => ({ valor: t.valor, texto: t.texto, descripcion: t.descripcion }))}
      />
      <GrupoCasillas
        etiqueta="Respuesta"
        opciones={[{ valor: 'si', texto: 'Es obligatoria', descripcion: 'No se puede enviar la encuesta sin contestarla.' }]}
        valor={obligatoria.value ? ['si'] : []}
        onChange={(v) => obligatoria.onChange(v.includes('si'))}
      />

      {conOpciones && (
        <>
          <SelectBusqueda
            etiqueta="Peligro que detecta"
            etiquetaBusqueda="Buscar peligro"
            opciones={opcionesPeligro}
            valor={peligro.value || SIN_PELIGRO}
            onChange={(v) => peligro.onChange(v === SIN_PELIGRO ? '' : v)}
            onBlur={peligro.onBlur}
            error={errors.peligroId?.message}
            ayuda="Si alguien elige una opción que indica exposición, Norvia sugiere este peligro a la empresa."
          />
          <fieldset className="grid gap-3" aria-describedby={mensajeOpciones ? 'error-opciones' : undefined}>
            <legend className="mb-1 text-sm font-medium">
              Opciones<span aria-hidden className="text-error-text"> *</span>
            </legend>
            {opciones.fields.map((campo, i) => (
              <div key={campo.id} className="grid gap-2 rounded-md border border-border-default p-3">
                <div className="flex items-end gap-2">
                  <div className="min-w-0 flex-1">
                    <CampoTexto
                      etiqueta={`Opción ${i + 1}`}
                      autoComplete="off"
                      registro={register(`opciones.${i}.texto`)}
                      error={errors.opciones?.[i]?.texto}
                    />
                  </div>
                  <BotonIcono etiqueta={`Subir la opción ${i + 1}`} disabled={i === 0} onClick={() => opciones.move(i, i - 1)}>
                    <ArrowUp aria-hidden size={16} strokeWidth={1.5} />
                  </BotonIcono>
                  <BotonIcono etiqueta={`Bajar la opción ${i + 1}`} disabled={i === opciones.fields.length - 1} onClick={() => opciones.move(i, i + 1)}>
                    <ArrowDown aria-hidden size={16} strokeWidth={1.5} />
                  </BotonIcono>
                  <BotonIcono etiqueta={`Quitar la opción ${i + 1}`} disabled={opciones.fields.length <= 2} onClick={() => opciones.remove(i)}>
                    <X aria-hidden size={16} strokeWidth={1.5} />
                  </BotonIcono>
                </div>
                {peligroId && (
                  <label className="casilla">
                    <input type="checkbox" {...register(`opciones.${i}.indicaExposicion`)} />
                    <span className="flex h-lh items-center">
                      <span aria-hidden className="casilla-caja">
                        <Check strokeWidth={2.5} />
                      </span>
                    </span>
                    <span className="text-sm">Indica exposición al peligro</span>
                  </label>
                )}
              </div>
            ))}
            {mensajeOpciones && (
              <p id="error-opciones" className="text-sm text-error-text">
                {mensajeOpciones}
              </p>
            )}
            <div>
              <Boton
                variante="secundario"
                disabled={opciones.fields.length >= 20}
                onClick={() => opciones.append({ texto: '', indicaExposicion: false })}
              >
                <Plus aria-hidden size={16} strokeWidth={1.5} />
                Agregar opción
              </Boton>
            </div>
          </fieldset>
        </>
      )}

      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
        <Link to={`/catalogos/preguntas${pregunta ? `?categoria=${pregunta.categoriaRiesgoId}` : ''}`}>Cancelar</Link>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Guardando…">
          {pregunta ? 'Guardar cambios' : 'Crear pregunta'}
        </Boton>
      </div>
    </form>
  )
}
