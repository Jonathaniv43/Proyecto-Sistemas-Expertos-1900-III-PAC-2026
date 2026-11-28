import { ClipboardList, FolderOpen, TriangleAlert } from 'lucide-react'
import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useController, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { clasesBoton } from '@/shared/components/ui/clases'
import { GrupoCasillas } from '@/shared/components/ui/GrupoCasillas'
import { useFormulario } from '@/shared/forms'
import { ROLES, useTieneRol } from '@/shared/sesion'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { Volver } from '@/shared/components/ui/Volver'
import { crearEncuesta, listarCategoriasDisponibles } from '../api/encuestas'
import { PreguntaEncuesta } from '../components/PreguntaEncuesta'
import { nuevaEncuestaEsquema } from '../schemas/nuevaEncuesta'
import type { CategoriaDisponible, CrearEncuestaResponse } from '../types/encuestas'

const plural = (n: number, uno: string, varios: string) => `${n} ${n === 1 ? uno : varios}`

/**
 * HU-029: el Responsable SST elige las categorías de riesgo del rubro de su empresa y ve, antes de
 * crear la encuesta, las preguntas que incluirá. Termina creando el borrador (HU-030 sigue con la
 * edición y la publicación).
 */
export function NuevaEncuestaPage() {
  const categorias = usePeticion(listarCategoriasDisponibles)
  const cargandoVisible = useCargaVisible(categorias.cargando)
  const [creada, setCreada] = useState<{ encuesta: CrearEncuestaResponse; preguntas: number }>()

  const { ejecutar: cargar } = categorias
  useEffect(() => {
    void cargar()
  }, [cargar])

  const {
    register,
    control,
    reset,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(nuevaEncuestaEsquema, {
    defaultValues: { titulo: '', categoriaRiesgoIds: [] },
    erroresPorCodigo: {
      'encuestas.categoria_invalida': {
        campo: 'categoriaRiesgoIds',
        mensaje: 'Alguna categoría elegida ya no está disponible. Vuelve a cargar la página y elige de nuevo.',
      },
    },
  })
  const { field: campoCategorias } = useController({ control, name: 'categoriaRiesgoIds' })
  const elegidas = useWatch({ control, name: 'categoriaRiesgoIds' })

  // Las preguntas de las categorías elegidas, en el orden en que se muestran las categorías.
  const preguntas = useMemo(
    () =>
      (categorias.datos ?? [])
        .filter((c) => elegidas.includes(c.id))
        .flatMap((c) => [...c.preguntasPreview].sort((a, b) => a.orden - b.orden).map((p) => ({ categoria: c, pregunta: p }))),
    [categorias.datos, elegidas],
  )

  if (creada) {
    return (
      <div className="grid w-full max-w-layout-narrow gap-6">
        <Volver a="/encuestas" texto="Volver a encuestas" titulo="Borrador creado" />
        <h1 className="text-xl font-semibold">Borrador creado</h1>
        <Aviso tipo="exito" titulo={`"${creada.encuesta.titulo}" quedó como borrador`}>
          Incluye {plural(creada.preguntas, 'pregunta', 'preguntas')}. Mientras sea borrador, los encuestados no la ven.
        </Aviso>
        <div className="flex flex-col gap-3 sm:flex-row">
          <Link to={`/encuestas/${creada.encuesta.id}`} className={clasesBoton('primario')}>
            Editar el borrador
          </Link>
          <Boton
            variante="secundario"
            onClick={() => {
              reset({ titulo: '', categoriaRiesgoIds: [] })
              setCreada(undefined)
            }}
          >
            Crear otra encuesta
          </Boton>
          <Link to="/encuestas" className={clasesBoton('secundario')}>
            Ver las encuestas
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <Volver a="/encuestas" texto="Volver a encuestas" titulo="Nueva encuesta" />
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Nueva encuesta</h1>
        <p className="text-text-secondary">
          Elige las categorías de riesgo que quieres explorar. Te mostramos las preguntas que tendrá la encuesta antes de
          crearla.
        </p>
      </header>

      <Contenido
        estado={categorias}
        cargandoVisible={cargandoVisible}
        onReintentar={() => void cargar()}
      >
        {(lista) => (
          <form
            noValidate
            className="grid gap-8"
            onSubmit={enviar(async (datos) => {
              const encuesta = await crearEncuesta(datos)
              setCreada({ encuesta, preguntas: preguntas.length })
            })}
          >
            {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}

            <div className="rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
              <GrupoCasillas
                etiqueta="Categorías de riesgo"
                required
                ayuda="Solo aparecen las categorías que corresponden al rubro de tu empresa."
                opciones={lista.map((c) => ({
                  valor: c.id,
                  texto: c.nombre,
                  descripcion: [c.descripcion, plural(c.preguntasPreview.length, 'pregunta', 'preguntas')]
                    .filter(Boolean)
                    .join(' · '),
                }))}
                valor={campoCategorias.value}
                onChange={campoCategorias.onChange}
                onBlur={campoCategorias.onBlur}
                error={errors.categoriaRiesgoIds?.message ?? errors.categoriaRiesgoIds?.root?.message}
              />
            </div>

            <section className="grid gap-4" aria-labelledby="titulo-vista-previa">
              <h2 id="titulo-vista-previa" className="text-md font-semibold">
                Preguntas que incluirá{preguntas.length > 0 && ` (${preguntas.length})`}
              </h2>
              {preguntas.length === 0 ? (
                <div className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-12 text-center">
                  <ClipboardList aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
                  <p className="text-text-secondary">Elige al menos una categoría para ver sus preguntas.</p>
                </div>
              ) : (
                <div className="grid gap-8 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
                  {preguntas.some((p) => p.pregunta.obligatoria) && (
                    <p className="text-text-secondary">Las preguntas con * son obligatorias.</p>
                  )}
                  {preguntas.map(({ categoria, pregunta }, i) => (
                    <div key={pregunta.id} className="grid gap-2">
                      {(i === 0 || preguntas[i - 1].categoria.id !== categoria.id) && (
                        <p className="text-xs font-medium text-text-secondary">{categoria.nombre}</p>
                      )}
                      <PreguntaEncuesta numero={i + 1} pregunta={pregunta} />
                    </div>
                  ))}
                </div>
              )}
            </section>

            <div className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
              <CampoTexto
                etiqueta="Título de la encuesta"
                required
                autoComplete="off"
                registro={register('titulo')}
                error={errors.titulo}
                ayuda="Así la verán los encuestados. Por ejemplo: Condiciones de trabajo 2026."
              />
              <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
                <Link to="/encuestas">Cancelar</Link>
                <Boton type="submit" cargando={isSubmitting} textoCargando="Creando…">
                  Crear borrador
                </Boton>
              </div>
            </div>
          </form>
        )}
      </Contenido>
    </div>
  )
}

interface ContenidoProps {
  estado: ReturnType<typeof usePeticion<CategoriaDisponible[]>>
  cargandoVisible: boolean
  onReintentar: () => void
  children: (categorias: CategoriaDisponible[]) => ReactNode
}

/** Estados de pantalla del design system: cargando, error al cargar, vacío y con datos. */
function Contenido({ estado, cargandoVisible, onReintentar, children }: ContenidoProps) {
  const administra = useTieneRol([ROLES.adminEmpresa])
  if (estado.error) {
    if (estado.error.codigo === 'encuestas.empresa_sin_rubro') {
      return (
        <EstadoVacio icono={<TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />} titulo="Tu empresa todavía no tiene rubro">
          Las categorías de riesgo dependen del rubro.{' '}
          {administra ? (
            <>
              Elígelo en el <Link to="/empresa">perfil de la empresa</Link>.
            </>
          ) : (
            'Pide a quien administra la empresa que lo complete en el perfil.'
          )}
        </EstadoVacio>
      )
    }
    return (
      <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
        <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
        <h2 className="text-md font-semibold">No se pudieron cargar las categorías de riesgo</h2>
        <p className="text-text-secondary">{estado.error.mensaje}</p>
        <Boton variante="secundario" onClick={onReintentar}>
          Reintentar
        </Boton>
      </section>
    )
  }

  if (estado.cargando || !estado.datos) {
    return (
      <div aria-busy="true" aria-label="Cargando categorías" className="grid gap-3">
        {cargandoVisible &&
          [0, 1, 2].map((i) => (
            <div key={i} className="h-table-row w-full animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />
          ))}
      </div>
    )
  }

  if (estado.datos.length === 0) {
    return (
      <EstadoVacio icono={<FolderOpen aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />} titulo="Todavía no hay categorías de riesgo para tu rubro">
        El equipo de Norvia las agrega al catálogo. Cuando estén disponibles, podrás crear la encuesta desde aquí.
      </EstadoVacio>
    )
  }

  return children(estado.datos)
}

function EstadoVacio({ icono, titulo, children }: { icono: ReactNode; titulo: string; children: ReactNode }) {
  return (
    <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
      {icono}
      <h2 className="text-md font-semibold">{titulo}</h2>
      <p className="max-w-layout-narrow text-text-secondary">{children}</p>
    </section>
  )
}
