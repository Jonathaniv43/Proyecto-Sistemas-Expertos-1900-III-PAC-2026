import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { useFormulario } from '@/shared/forms'
import { vincularMicrosoft } from '../api/microsoft'
import { vincularMicrosoftEsquema } from '../schemas/microsoft'

interface VincularMicrosoftFormProps {
  tokenIdentidad: string
  email: string
  /** La cuenta de Microsoft quedó vinculada: sigue el flujo que la persona había empezado. */
  onVinculada: () => void
}

/** HU-120, RN-83: el correo ya tiene cuenta con contraseña; se confirma esa contraseña para vincular Microsoft. */
export function VincularMicrosoftForm({ tokenIdentidad, email, onVinculada }: VincularMicrosoftFormProps) {
  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(vincularMicrosoftEsquema, {
    defaultValues: { password: '' },
    erroresPorCodigo: {
      'auth.credenciales_invalidas': { campo: 'password', mensaje: 'La contraseña no es correcta.' },
    },
  })

  return (
    <form
      noValidate
      className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      onSubmit={enviar(async ({ password }) => {
        await vincularMicrosoft({ tokenIdentidad, password })
        onVinculada()
      })}
    >
      <h2 className="text-md font-semibold">Vincula tu cuenta de Microsoft</h2>
      <p className="text-text-secondary">
        Ya existe una cuenta de Norvia con <strong className="font-medium text-text-primary">{email}</strong>. Escribe su
        contraseña actual para vincular Microsoft. Después podrás entrar con cualquiera de las dos formas.
      </p>
      {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
      <CampoTexto
        etiqueta="Contraseña actual"
        required
        type="password"
        autoComplete="current-password"
        registro={register('password')}
        error={errors.password}
      />
      <Boton type="submit" className="w-full sm:w-auto sm:justify-self-start" cargando={isSubmitting} textoCargando="Vinculando…">
        Vincular Microsoft
      </Boton>
    </form>
  )
}
