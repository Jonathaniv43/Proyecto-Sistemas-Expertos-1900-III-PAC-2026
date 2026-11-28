import { FileText, TriangleAlert } from 'lucide-react'
import { useCallback, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { Volver } from '@/shared/components/ui/Volver'
import { useFormulario } from '@/shared/forms'
import { useCargaVisible } from '@/shared/hooks/useCargaVisible'
import { usePeticion } from '@/shared/hooks/usePeticion'
import { generarDocumento, listarPlantillas } from '../api/documentos'
import { NOMBRE_TIPO } from '../etiquetas'
import { generarDocumentoEsquema } from '../schemas/generar'
import type { PlantillaDisponible } from '../types/documentos'
import type { EstadoDocumentoNavegacion } from './DocumentoPage'

/**
 * HU-070: genera una política o un procedimiento desde una plantilla del rubro de la empresa. Se llena con los datos del
 * perfil y queda como borrador para que la empresa lo revise (RN-49). Solo el Responsable SST.
 */
export function GenerarDocumentoPage() {
  const plantillas = usePeticion(listarPlantillas)
  const cargandoVisible = useCargaVisible(plantillas.cargando)
  const { ejecutar } = plantillas
  const cargar = useCallback(() => void ejecutar(), [ejecutar])
  useEffect(cargar, [cargar])

  const sinRubro = plantillas.error?.codigo === 'documentos.sin_rubro'

  return (
    <div className="grid w-full max-w-layout-narrow gap-6">
      <Volver a="/documentos" texto="Volver a documentos" titulo="Generar documento" />
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Generar documento</h1>
        <p className="text-text-secondary">
          Elige una plantilla. Norvia la llena con los datos del perfil de la empresa y la deja como borrador para que la revises.
        </p>
      </header>

      {sinRubro ? (
        <Aviso tipo="advertencia" titulo="Falta el rubro de la empresa">
          Las plantillas dependen del rubro. <Link to="/empresa">Completa el perfil de la empresa</Link> y vuelve a intentarlo.
        </Aviso>
      ) : plantillas.error ? (
        <section className="flex flex-col items-center gap-4 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <TriangleAlert aria-hidden size={32} strokeWidth={1.5} className="text-error-icon" />
          <h2 className="text-md font-semibold">No se pudieron cargar las plantillas</h2>
          <p className="text-text-secondary">{plantillas.error.mensaje}</p>
          <Boton variante="secundario" onClick={cargar}>
            Reintentar
          </Boton>
        </section>
      ) : !plantillas.datos ? (
        <div aria-busy="true" aria-label="Cargando las plantillas" className="grid gap-3">
          {cargandoVisible &&
            [0, 1, 2].map((i) => <div key={i} className="h-table-row animate-pulse rounded-sm bg-bg-subtle motion-reduce:animate-none" />)}
        </div>
      ) : plantillas.datos.length === 0 ? (
        <section className="flex flex-col items-center gap-2 rounded-lg border border-border-default bg-bg-surface px-4 py-16 text-center">
          <FileText aria-hidden size={32} strokeWidth={1.5} className="text-text-tertiary" />
          <h2 className="text-md font-semibold">No hay plantillas para el rubro de la empresa</h2>
          <p className="max-w-layout-narrow text-text-secondary">El equipo de Norvia las agrega desde sus catálogos.</p>
        </section>
      ) : (
        <Formulario plantillas={plantillas.datos} />
      )}
    </div>
  )
}

function Formulario({ plantillas }: { plantillas: PlantillaDisponible[] }) {
  const navigate = useNavigate()
  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(generarDocumentoEsquema, {
    defaultValues: { plantillaDocumentoId: plantillas.length === 1 ? plantillas[0].id : '', titulo: '' },
    erroresPorCodigo: {
      'documentos.plantilla_no_disponible': {
        campo: 'plantillaDocumentoId',
        mensaje: 'Esa plantilla ya no está disponible. Vuelve a cargar la página y elige otra.',
      },
    },
  })

  return (
    <form
      noValidate
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async (datos) => {
        const documento = await generarDocumento({ plantillaDocumentoId: datos.plantillaDocumentoId, titulo: datos.titulo || undefined })
        // replace: "Volver" desde el documento lleva a la lista, no de nuevo al formulario.
        navigate(`/documentos/${documento.id}`, { replace: true, state: { generado: true } satisfies EstadoDocumentoNavegacion })
      })}
    >
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      <GrupoRadio
        etiqueta="Plantilla"
        required
        registro={register('plantillaDocumentoId')}
        error={errors.plantillaDocumentoId}
        opciones={plantillas.map((p) => ({ valor: p.id, texto: p.titulo, descripcion: NOMBRE_TIPO[p.tipoDocumento] }))}
      />
      <CampoTexto
        etiqueta="Título"
        autoComplete="off"
        registro={register('titulo')}
        error={errors.titulo}
        ayuda="Opcional. Si lo dejas vacío, el documento lleva el nombre de la plantilla."
      />
      <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
        <Link to="/documentos">Cancelar</Link>
        <Boton type="submit" cargando={isSubmitting} textoCargando="Generando…">
          Generar borrador
        </Boton>
      </div>
    </form>
  )
}
