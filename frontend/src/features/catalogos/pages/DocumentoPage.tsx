import { useCallback, useEffect } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoSelect } from '@/shared/components/ui/CampoSelect'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { MARCADORES_DOCUMENTO } from '@/shared/documentos'
import { useFormulario } from '@/shared/forms'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { Volver } from '@/shared/components/ui/Volver'
import { crearDocumento, editarDocumento, listarDocumentos, listarRubros } from '../api/catalogos'
import { EstadoCarga } from '../components/EstadoCarga'
import { AvisoConsulta } from '../components/AvisoConsulta'
import { ERRORES_CATALOGO, TIPOS_DOCUMENTO } from '../etiquetas'
import { usePuedeEditar } from '../permisos'
import { documentoEsquema } from '../schemas/catalogos'
import type { PlantillaDocumento, Rubro, TipoDocumento } from '../types/catalogos'
import type { EstadoDocumentoGuardado } from './DocumentosPage'

/**
 * HU-023: crear o editar una plantilla de política o procedimiento. El contenido admite marcadores como
 * {{RazonSocial}}, que se reemplazan con los datos de la empresa al generar su documento (M9). Soporte la ve sin editar.
 */
export function DocumentoPage() {
  const { id } = useParams()
  const documentos = usePeticion(listarDocumentos)
  const rubros = usePeticion(listarRubros)
  const { ejecutar: cargarDocumentos } = documentos
  const { ejecutar: cargarRubros } = rubros
  const cargar = useCallback(() => {
    void cargarRubros()
    if (id) void cargarDocumentos()
  }, [cargarRubros, cargarDocumentos, id])
  useEffect(cargar, [cargar])

  const documento = id ? documentos.datos?.find((d) => d.id === id) : undefined
  if (id && documentos.datos && !documento) return <NotFoundPage />
  const listo = !!rubros.datos && (!id || !!documento)
  const titulo = id ? (documento?.titulo ?? 'Plantilla de documento') : 'Nueva plantilla de documento'

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <Volver a="/catalogos/documentos" texto="Volver a plantillas de documento" titulo={titulo} />
      <h1 className="text-xl font-semibold wrap-break-word">{titulo}</h1>
      <AvisoConsulta />
      <EstadoCarga
        cargando={!listo}
        error={rubros.error ?? documentos.error}
        hayDatos={listo}
        que="la plantilla"
        onReintentar={cargar}
      >
        {listo && <Formulario documento={documento} rubros={rubros.datos!} />}
      </EstadoCarga>
    </div>
  )
}

function Formulario({ documento, rubros }: { documento: PlantillaDocumento | undefined; rubros: Rubro[] }) {
  const navigate = useNavigate()
  const puedeEditar = usePuedeEditar()
  const {
    register,
    formState: { errors, isSubmitting, isDirty },
    enviar,
    errorGeneral,
  } = useFormulario(documentoEsquema, {
    defaultValues: {
      codigo: documento?.codigo ?? '',
      titulo: documento?.titulo ?? '',
      tipoDocumento: documento?.tipoDocumento ?? ('' as TipoDocumento),
      rubroId: documento?.rubroId ? String(documento.rubroId) : '',
      contenido: documento?.contenido ?? '',
    },
    erroresPorCodigo: {
      [ERRORES_CATALOGO.codigoDuplicado]: { campo: 'codigo', mensaje: 'Ya hay una plantilla con ese código.' },
      'catalogos.rubro_invalido': { campo: 'rubroId', mensaje: 'Ese rubro está desactivado. Elige otro.' },
    },
  })

  return (
    <form
      noValidate
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async (d) => {
        const guardada = documento
          ? await editarDocumento(documento.id, { titulo: d.titulo, tipoDocumento: d.tipoDocumento, rubroId: Number(d.rubroId || 0), contenido: d.contenido })
          : await crearDocumento({ ...d, rubroId: d.rubroId ? Number(d.rubroId) : null })
        navigate('/catalogos/documentos', { state: { guardado: guardada.titulo } satisfies EstadoDocumentoGuardado })
      })}
    >
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      <fieldset disabled={!puedeEditar} className="grid gap-6">
        <CampoTexto etiqueta="Título" required autoComplete="off" registro={register('titulo')} error={errors.titulo} />
        <GrupoRadio
          etiqueta="Tipo"
          required
          registro={register('tipoDocumento')}
          error={errors.tipoDocumento}
          opciones={Object.entries(TIPOS_DOCUMENTO).map(([valor, texto]) => ({ valor, texto }))}
        />
        <CampoSelect
          etiqueta="Rubro"
          todos="Todos los rubros"
          registro={register('rubroId')}
          error={errors.rubroId}
          opciones={rubros.filter((r) => r.activo || r.id === documento?.rubroId).map((r) => ({ valor: String(r.id), texto: r.nombre }))}
          ayuda="Si la plantilla es para un solo rubro, solo la verán las empresas de ese rubro."
        />
        <AreaTexto
          etiqueta="Contenido"
          required
          rows={14}
          error={errors.contenido?.message}
          ayuda={
            <>
              En Markdown: <code># Título</code>, <code>- lista</code>, <code>**negrita**</code>. Estos marcadores se reemplazan con
              los datos de la empresa al generar el documento: {MARCADORES_DOCUMENTO.map((m) => m.marcador).join(', ')}. Para lo
              que solo sabe la empresa, escribe <code>[Completar: lo que falta]</code>.
            </>
          }
          {...register('contenido')}
        />
        <CampoTexto
          etiqueta="Código"
          required
          autoComplete="off"
          readOnly={!!documento}
          registro={register('codigo')}
          error={errors.codigo}
          ayuda={documento ? 'El código no se cambia.' : 'Minúsculas, números y guion bajo. Por ejemplo: politica_sst. No se puede cambiar después.'}
        />
      </fieldset>
      {puedeEditar && (
        <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
          <Link to="/catalogos/documentos">Cancelar</Link>
          <Boton type="submit" disabled={!!documento && !isDirty} cargando={isSubmitting} textoCargando="Guardando…">
            {documento ? 'Guardar cambios' : 'Crear plantilla'}
          </Boton>
        </div>
      )}
    </form>
  )
}
