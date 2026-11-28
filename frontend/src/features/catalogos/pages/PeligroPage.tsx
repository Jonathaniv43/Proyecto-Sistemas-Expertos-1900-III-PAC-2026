import { Plus } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useController } from 'react-hook-form'
import { useParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoCasillas } from '@/shared/components/ui/GrupoCasillas'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { Insignia } from '@/shared/components/ui/Insignia'
import { useFormulario } from '@/shared/forms'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { Volver } from '@/shared/components/ui/Volver'
import { crearPlantillaTarea, editarPlantillaTarea, estadoPlantillaTarea, listarPeligros, listarPlantillasTarea } from '../api/catalogos'
import { AvisoMensaje } from '../components/AvisoMensaje'
import { BotonEstado, EstadoActivo } from '../components/Estado'
import { EstadoCarga } from '../components/EstadoCarga'
import { CuerpoFormulario, ModalFormulario, type EstadoFormulario } from '../components/ModalFormulario'
import { AvisoConsulta } from '../components/AvisoConsulta'
import { CLASIFICACIONES, NOMBRE_TIPO_CONTROL, PRIORIDADES, TIPOS_CONTROL } from '../etiquetas'
import { usePuedeEditar } from '../permisos'
import { plantillaTareaEsquema } from '../schemas/catalogos'
import type { PlantillaTarea, TipoControl } from '../types/catalogos'
import { useCambioEstado } from '../useCambioEstado'

const EXPLICACION_TIPO: Record<TipoControl | 'modelo', string> = {
  eliminacion: 'Quitar el peligro. El más efectivo.',
  sustitucion: 'Cambiarlo por algo menos peligroso.',
  ingenieria: 'Separar a las personas del peligro: guardas, barandas, ventilación.',
  administrativo: 'Cambiar la forma de trabajar: procedimientos, capacitación, señalización.',
  epp: 'Protección que usa la persona. La última opción.',
  modelo: 'No es un control: es una tarea que entra directo al plan de trabajo, como una inspección.',
}

/**
 * HU-023: un peligro y sus plantillas de tarea. Las que tienen tipo de control se sugieren al definir los controles de un
 * riesgo con este peligro, de la más efectiva a la menos (HU-039, RN-21); las tareas modelo entran a la propuesta del
 * plan (HU-042). Lo ya aceptado o generado en las empresas guarda su copia y no cambia.
 */
export function PeligroPage() {
  const { id = '' } = useParams()
  const peligros = usePeticion(listarPeligros)
  const plantillas = usePeticion(listarPlantillasTarea)
  const puedeEditar = usePuedeEditar()
  const [editando, setEditando] = useState<PlantillaTarea | 'nueva'>()

  const { ejecutar: cargarPeligros } = peligros
  const { ejecutar: cargarPlantillas } = plantillas
  const recargarPlantillas = useCallback(() => void cargarPlantillas(id), [cargarPlantillas, id])
  const cargar = useCallback(() => {
    void cargarPeligros()
    recargarPlantillas()
  }, [cargarPeligros, recargarPlantillas])
  useEffect(cargar, [cargar])
  const { mensaje, setMensaje, ocupado, cambiar } = useCambioEstado(recargarPlantillas)

  const peligro = peligros.datos?.find((p) => p.id === id)
  if (peligros.datos && !peligro) return <NotFoundPage />

  const lista = plantillas.datos ?? []
  const controles = lista.filter((p) => p.tipoControl)
  const tareas = lista.filter((p) => !p.tipoControl)

  const grupo = (titulo: string, ayuda: string, items: PlantillaTarea[]) => (
    <section aria-label={titulo} className="grid gap-3">
      <div className="grid gap-1">
        <h2 className="font-semibold">
          {titulo} <span className="font-normal text-text-secondary">({items.length})</span>
        </h2>
        <p className="text-sm text-text-secondary">{ayuda}</p>
      </div>
      {items.length === 0 ? (
        <p className="text-text-secondary">Todavía no hay.</p>
      ) : (
        <ul className="grid gap-3">
          {items.map((t) => (
            <li key={t.id} className="grid gap-2 rounded-md border border-border-default bg-bg-surface p-4">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div className="grid gap-1">
                  {t.tipoControl && <span className="text-xs font-medium text-text-secondary">{NOMBRE_TIPO_CONTROL[t.tipoControl]}</span>}
                  <p className="font-medium">{t.titulo}</p>
                </div>
                <EstadoActivo activo={t.activa} />
              </div>
              {t.descripcion && <p className="wrap-break-word text-text-secondary">{t.descripcion}</p>}
              <p className="flex flex-wrap items-center gap-2 text-xs text-text-secondary">
                Prioridad sugerida {PRIORIDADES[t.prioridadSugerida].toLowerCase()}
                {t.requiereEvidencia && <Insignia tipo="neutro">Pide evidencia</Insignia>}
              </p>
              {puedeEditar && (
                <div className="flex flex-wrap items-center gap-1">
                  <Boton variante="secundario" aria-label={`Editar «${t.titulo}»`} onClick={() => setEditando(t)}>
                    Editar
                  </Boton>
                  <BotonEstado
                    activo={t.activa}
                    nombre={t.titulo}
                    ocupado={ocupado !== undefined}
                    onCambiar={(activo) => void cambiar(t.id, t.titulo, activo, () => estadoPlantillaTarea(t.id, activo))}
                  />
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  )

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <Volver a="/catalogos/peligros" texto="Volver a peligros y plantillas" titulo={peligro?.nombre} />
      <AvisoConsulta />
      <EstadoCarga
        cargando={peligros.cargando || plantillas.cargando}
        error={peligros.error ?? plantillas.error}
        hayDatos={!!peligro && !!plantillas.datos}
        que="el peligro"
        onReintentar={cargar}
      >
        {peligro && (
          <>
            <header className="flex flex-wrap items-start justify-between gap-4">
              <div className="grid gap-1">
                <h1 className="flex flex-wrap items-center gap-2 text-xl font-semibold">
                  {peligro.nombre} <EstadoActivo activo={peligro.activo} />
                </h1>
                <p className="text-sm text-text-secondary">
                  {CLASIFICACIONES[peligro.clasificacion]} · {peligro.codigo}
                </p>
                {peligro.descripcion && <p className="text-text-secondary">{peligro.descripcion}</p>}
              </div>
              {puedeEditar && peligro.activo && (
                <Boton variante="secundario" onClick={() => setEditando('nueva')}>
                  <Plus aria-hidden size={16} strokeWidth={1.5} />
                  Nueva plantilla
                </Boton>
              )}
            </header>
            {!peligro.activo && (
              <p className="text-text-secondary">El peligro está inactivo: no se le agregan plantillas nuevas.</p>
            )}
            <AvisoMensaje mensaje={mensaje} />
            {grupo(
              'Controles sugeridos',
              'Norvia los sugiere al definir los controles de un riesgo con este peligro, del más efectivo al menos efectivo.',
              controles,
            )}
            {grupo('Tareas modelo', 'Entran a la propuesta del plan de trabajo de los riesgos con este peligro.', tareas)}
          </>
        )}
      </EstadoCarga>
      <ModalFormulario
        abierto={editando !== undefined}
        titulo={editando === 'nueva' ? 'Nueva plantilla' : 'Editar plantilla'}
        onCerrar={() => setEditando(undefined)}
      >
        {(onEstado) => (
          <FormularioPlantilla
            peligroId={id}
            plantilla={editando === 'nueva' ? undefined : editando}
            onEstado={onEstado}
            onCancelar={() => setEditando(undefined)}
            onGuardada={(t) => {
              setEditando(undefined)
              setMensaje({ tipo: 'exito', texto: `«${t.titulo}» quedó guardada.` })
              recargarPlantillas()
            }}
          />
        )}
      </ModalFormulario>
    </div>
  )
}

interface FormularioPlantillaProps {
  peligroId: string
  plantilla: PlantillaTarea | undefined
  onEstado: (e: EstadoFormulario) => void
  onCancelar: () => void
  onGuardada: (t: PlantillaTarea) => void
}

function FormularioPlantilla({ peligroId, plantilla, onEstado, onCancelar, onGuardada }: FormularioPlantillaProps) {
  const {
    register,
    control,
    formState: { errors, isDirty, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(plantillaTareaEsquema, {
    defaultValues: {
      tipo: plantilla ? (plantilla.tipoControl ?? 'modelo') : ('' as 'modelo'),
      titulo: plantilla?.titulo ?? '',
      descripcion: plantilla?.descripcion ?? '',
      prioridad: plantilla ? (String(plantilla.prioridadSugerida) as '1') : ('' as '1'),
      requiereEvidencia: plantilla?.requiereEvidencia ?? false,
    },
    erroresPorCodigo: { 'catalogos.peligro_invalido': { mensaje: 'El peligro está inactivo: no se le agregan plantillas.' } },
  })
  const { field: evidencia } = useController({ control, name: 'requiereEvidencia' })
  return (
    <CuerpoFormulario
      onSubmit={enviar(async (d) => {
        const datos = {
          tipoControl: d.tipo === 'modelo' ? null : d.tipo,
          titulo: d.titulo,
          descripcion: d.descripcion,
          prioridadSugerida: Number(d.prioridad) as 1 | 2 | 3,
          requiereEvidencia: d.requiereEvidencia,
        }
        onGuardada(plantilla ? await editarPlantillaTarea(plantilla.id, datos) : await crearPlantillaTarea(peligroId, datos))
      })}
      onCancelar={onCancelar}
      onEstado={onEstado}
      sucio={isDirty}
      ocupado={isSubmitting}
      errorGeneral={errorGeneral}
      textoEnviar={plantilla ? 'Guardar cambios' : 'Crear plantilla'}
    >
      <GrupoRadio
        etiqueta="Qué es"
        required
        registro={register('tipo')}
        error={errors.tipo}
        opciones={[
          ...TIPOS_CONTROL.map((t) => ({ valor: t.valor, texto: `Control: ${t.texto.toLowerCase()}`, descripcion: EXPLICACION_TIPO[t.valor] })),
          { valor: 'modelo', texto: 'Tarea modelo', descripcion: EXPLICACION_TIPO.modelo },
        ]}
      />
      <CampoTexto
        etiqueta="Título"
        required
        autoComplete="off"
        registro={register('titulo')}
        error={errors.titulo}
        ayuda="Qué hay que hacer, en pocas palabras. Es el texto que ve la empresa."
      />
      <AreaTexto etiqueta="Descripción" ayuda="Opcional. Cómo hacerlo." error={errors.descripcion?.message} {...register('descripcion')} />
      <GrupoRadio
        etiqueta="Prioridad sugerida"
        required
        registro={register('prioridad')}
        error={errors.prioridad}
        opciones={([1, 2, 3] as const).map((p) => ({ valor: String(p), texto: PRIORIDADES[p] }))}
      />
      <GrupoCasillas
        etiqueta="Evidencia"
        opciones={[{ valor: 'si', texto: 'Pide evidencia para completarla', descripcion: 'Una foto o un PDF de que se hizo.' }]}
        valor={evidencia.value ? ['si'] : []}
        onChange={(v) => evidencia.onChange(v.includes('si'))}
      />
    </CuerpoFormulario>
  )
}
