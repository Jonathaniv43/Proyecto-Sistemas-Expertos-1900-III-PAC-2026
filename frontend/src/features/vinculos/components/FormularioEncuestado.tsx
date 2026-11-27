import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { useFormulario } from '@/shared/forms'
import { crearEncuestado } from '../api/vinculos'
import { encuestadoEsquema } from '../schemas/encuestados'
import type { CrearEncuestadoResponse } from '../types/vinculos'

/** Alta de un encuestado. Si la cuenta es nueva, el backend envía el código de activación por correo. */
export function FormularioEncuestado({ onCreado }: { onCreado: (encuestado: CrearEncuestadoResponse) => void }) {
  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(encuestadoEsquema, {
    defaultValues: { email: '', primerNombre: '', segundoNombre: '', primerApellido: '', segundoApellido: '' },
    erroresPorCodigo: {
      'vinculos.duplicado': { campo: 'email', mensaje: 'Esa persona ya tiene acceso a esta empresa.' },
    },
  })

  return (
    <form
      noValidate
      className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async (datos) => onCreado(await crearEncuestado(datos)))}
    >
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      <CampoTexto
        etiqueta="Correo"
        required
        type="email"
        autoComplete="off"
        registro={register('email')}
        error={errors.email}
        ayuda="Con este correo entrará a responder las encuestas."
      />
      <div className="grid gap-4 sm:grid-cols-2">
        <CampoTexto etiqueta="Primer nombre" required autoComplete="off" registro={register('primerNombre')} error={errors.primerNombre} />
        <CampoTexto etiqueta="Segundo nombre" autoComplete="off" registro={register('segundoNombre')} error={errors.segundoNombre} />
        <CampoTexto etiqueta="Primer apellido" required autoComplete="off" registro={register('primerApellido')} error={errors.primerApellido} />
        <CampoTexto etiqueta="Segundo apellido" autoComplete="off" registro={register('segundoApellido')} error={errors.segundoApellido} />
      </div>
      <div>
        <Boton type="submit" className="w-full sm:w-auto" cargando={isSubmitting} textoCargando="Agregando…">
          Agregar encuestado
        </Boton>
      </div>
    </form>
  )
}
