import { useEffect, useMemo, useState } from 'react'
import { useWatch } from 'react-hook-form'
import { extraerError } from '@/shared/api'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { DialogoConfirmacion } from '@/shared/components/ui/DialogoConfirmacion'
import { GrupoRadio } from '@/shared/components/ui/GrupoRadio'
import { CuerpoModal, Modal, PieModal } from '@/shared/components/ui/Modal'
import { useFormulario } from '@/shared/forms'
import { DESCRIPCIONES_ROL_EMPRESA, ETIQUETAS_ROL, ROLES_EMPRESA } from '@/shared/sesion'
import { actualizarVinculo } from '../api/vinculos'
import { editarAccesoEsquema, MAX_MOTIVO_DETALLE, quitaAcceso } from '../schemas/editarAcceso'
import type { ActualizarVinculoRequest, EstadoVinculo, Vinculo } from '../types/vinculos'

const OPCIONES_ESTADO: { valor: EstadoVinculo; texto: string; descripcion: string }[] = [
  { valor: 'activo', texto: 'Activo', descripcion: 'Puede entrar a Norvia en esta empresa.' },
  { valor: 'suspendido', texto: 'Suspendido', descripcion: 'No puede entrar por ahora. Puedes reactivarlo cuando quieras.' },
  { valor: 'baja', texto: 'De baja', descripcion: 'Ya no forma parte de la empresa. Si vuelve, puedes reactivarlo.' },
]

// "Falta de pago de la empresa" no se ofrece: es el motivo de la plataforma al suspender una empresa (D22).
const OPCIONES_MOTIVO = [
  { valor: 'decision_admin', texto: 'Decisión de la administración' },
  { valor: 'baja_laboral', texto: 'Dejó de trabajar en la empresa' },
  { valor: 'otro', texto: 'Otro motivo' },
] as const

interface EditarAccesoModalProps {
  vinculo: Vinculo | undefined
  onCerrar: () => void
  /** El acceso ya actualizado y una frase que describe el cambio. */
  onGuardado: (vinculo: Vinculo, mensaje: string) => void
}

/** HU-021: el modal se monta por persona, así el formulario arranca con sus valores. */
export function EditarAccesoModal({ vinculo, onCerrar, onGuardado }: EditarAccesoModalProps) {
  const [estadoForm, setEstadoForm] = useState({ sucio: false, ocupado: false })
  return (
    <Modal
      abierto={vinculo !== undefined}
      titulo={vinculo ? `Editar acceso de ${vinculo.nombreCompleto}` : ''}
      onCerrar={onCerrar}
      hayCambios={estadoForm.sucio}
      ocupado={estadoForm.ocupado}
    >
      {vinculo && (
        <Formulario key={vinculo.id} vinculo={vinculo} onCerrar={onCerrar} onGuardado={onGuardado} onEstado={setEstadoForm} />
      )}
    </Modal>
  )
}

/**
 * Rol y estado del acceso. Suspender o dar de baja pide el motivo y una confirmación aparte (diálogo
 * de peligro), porque quita el acceso. Reactivar o cambiar el rol se guarda directo.
 */
function Formulario({
  vinculo,
  onCerrar,
  onGuardado,
  onEstado,
}: { vinculo: Vinculo; onEstado: (e: { sucio: boolean; ocupado: boolean }) => void } & Omit<EditarAccesoModalProps, 'vinculo'>) {
  const esquema = useMemo(() => editarAccesoEsquema({ rolCodigo: vinculo.rolCodigo, estado: vinculo.estado }), [vinculo])
  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isDirty },
    errorGeneral,
    setErrorGeneral,
    aplicarErrorApi,
  } = useFormulario(esquema, {
    defaultValues: {
      rolCodigo: vinculo.rolCodigo as (typeof ROLES_EMPRESA)[number],
      estado: vinculo.estado,
      motivoCodigo: undefined,
      motivoDetalle: '',
    },
  })
  const estado = useWatch({ control, name: 'estado' })
  const motivo = useWatch({ control, name: 'motivoCodigo' })
  const pideMotivo = estado !== vinculo.estado && quitaAcceso(estado)

  const [pendiente, setPendiente] = useState<ActualizarVinculoRequest>()
  const [guardando, setGuardando] = useState(false)
  useEffect(() => onEstado({ sucio: isDirty, ocupado: guardando }), [onEstado, isDirty, guardando])

  const guardar = async (cambios: ActualizarVinculoRequest) => {
    setGuardando(true)
    setErrorGeneral(undefined)
    try {
      const actualizado = await actualizarVinculo(vinculo.id, cambios)
      onGuardado(actualizado, describirCambio(vinculo, cambios))
    } catch (causa) {
      setPendiente(undefined)
      aplicarErrorApi(extraerError(causa))
    } finally {
      setGuardando(false)
    }
  }

  const alEnviar = handleSubmit((datos) => {
    // Solo se envía lo que cambia (PATCH parcial).
    const cambios: ActualizarVinculoRequest = {}
    if (datos.rolCodigo !== vinculo.rolCodigo) cambios.rolCodigo = datos.rolCodigo
    if (datos.estado !== vinculo.estado) {
      cambios.estado = datos.estado
      if (quitaAcceso(datos.estado)) {
        cambios.motivoCodigo = datos.motivoCodigo ?? undefined
        cambios.motivoDetalle = datos.motivoDetalle
      }
    }
    if (cambios.estado && quitaAcceso(cambios.estado)) setPendiente(cambios)
    else void guardar(cambios)
  })

  const baja = pendiente?.estado === 'baja'

  return (
    <>
      <form noValidate onSubmit={alEnviar}>
        <CuerpoModal>
          <p className="text-text-secondary">{vinculo.email}</p>
          {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
          <GrupoRadio
            etiqueta="Rol"
            registro={register('rolCodigo')}
            opciones={ROLES_EMPRESA.map((r) => ({ valor: r, texto: ETIQUETAS_ROL[r], descripcion: DESCRIPCIONES_ROL_EMPRESA[r] }))}
            error={errors.rolCodigo}
          />
          <GrupoRadio
            etiqueta="Estado del acceso"
            registro={register('estado')}
            opciones={OPCIONES_ESTADO}
            error={errors.estado}
            ayuda="Solo cambia su acceso a esta empresa; si tiene acceso a otras, sigue igual."
          />
          {pideMotivo && (
            <>
              <GrupoRadio
                etiqueta={estado === 'baja' ? 'Motivo de la baja' : 'Motivo de la suspensión'}
                required
                registro={register('motivoCodigo')}
                opciones={OPCIONES_MOTIVO}
                error={errors.motivoCodigo}
              />
              <AreaTexto
                etiqueta="Detalle del motivo"
                maximo={MAX_MOTIVO_DETALLE}
                required={motivo === 'otro'}
                ayuda="Queda en el historial del acceso."
                error={errors.motivoDetalle?.message}
                {...register('motivoDetalle')}
              />
            </>
          )}
        </CuerpoModal>
        <PieModal>
          <Boton variante="secundario" disabled={guardando} onClick={onCerrar}>
            Cancelar
          </Boton>
          <Boton type="submit" disabled={!isDirty} cargando={guardando && !pendiente} textoCargando="Guardando…">
            Guardar cambios
          </Boton>
        </PieModal>
      </form>

      <DialogoConfirmacion
        abierto={pendiente !== undefined}
        titulo={baja ? `¿Dar de baja a ${vinculo.nombreCompleto}?` : `¿Suspender el acceso de ${vinculo.nombreCompleto}?`}
        textoConfirmar={baja ? 'Dar de baja' : 'Suspender acceso'}
        textoCargando={baja ? 'Dando de baja…' : 'Suspendiendo…'}
        peligro
        cargando={guardando}
        onConfirmar={() => pendiente && void guardar(pendiente)}
        onCancelar={() => setPendiente(undefined)}
      >
        Ya no podrá entrar a Norvia en esta empresa y se cerrarán sus sesiones abiertas. Su acceso a otras empresas no cambia.
      </DialogoConfirmacion>
    </>
  )
}

function describirCambio(antes: Vinculo, cambios: ActualizarVinculoRequest) {
  const nombre = antes.nombreCompleto
  if (cambios.estado === 'baja') return `Se dio de baja a ${nombre}.`
  if (cambios.estado === 'suspendido') return `Se suspendió el acceso de ${nombre}.`
  const rol = cambios.rolCodigo ? ETIQUETAS_ROL[cambios.rolCodigo as (typeof ROLES_EMPRESA)[number]] : undefined
  if (cambios.estado === 'activo') return rol ? `Se reactivó el acceso de ${nombre} como ${rol}.` : `Se reactivó el acceso de ${nombre}.`
  return `${nombre} ahora es ${rol}.`
}
