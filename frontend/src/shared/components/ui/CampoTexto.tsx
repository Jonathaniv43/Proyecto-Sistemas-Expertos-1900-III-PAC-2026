import { Eye, EyeOff } from 'lucide-react'
import { useId, useState, type InputHTMLAttributes, type ReactNode } from 'react'
import type { FieldError, UseFormRegisterReturn } from 'react-hook-form'
import { useLargoControl } from '@/shared/hooks/useLargoControl'
import { soloDigitos } from '@/shared/validacion/reglas'
import { BotonIcono } from './BotonIcono'
import { clasesControl } from './clases'
import { Contador } from './Contador'
import { Etiqueta, MensajeCampo } from './MensajeCampo'

interface CampoTextoProps extends Omit<InputHTMLAttributes<HTMLInputElement>, keyof UseFormRegisterReturn> {
  etiqueta: string
  /** La etiqueta solo la leen los lectores de pantalla (p. ej. filas de opciones con su número visible). */
  etiquetaOculta?: boolean
  /** Marca el campo con * (la regla real está en el esquema Zod; no se usa la validación nativa). */
  required?: boolean
  /** Resultado de `register('campo')` de React Hook Form. */
  registro: UseFormRegisterReturn
  error?: FieldError
  /** Texto de ayuda bajo el campo; se oculta mientras hay un error. */
  ayuda?: ReactNode
  /** Campos de largo exacto en dígitos (RTN 14, DNI 13): contador "12 / 14" sin guiones ni espacios. */
  largoExacto?: number
  /** Contenido extra bajo el campo (p. ej. requisitos de contraseña). */
  children?: ReactNode
}

const contarDigitos = (valor: string) => soloDigitos(valor).length

/**
 * Label + input + ayuda/error (design-system.md § Estados de componentes, Input).
 * Celular: 44px de alto y texto de 16px (evita el zoom del iPhone); escritorio: 36px y 14px.
 * Con `type="password"` lleva el botón para ver u ocultar la contraseña.
 */
export function CampoTexto({ etiqueta, etiquetaOculta, registro, error, ayuda, required, largoExacto, children, className = '', type, ...props }: CampoTextoProps) {
  const id = useId()
  const idMensaje = `${id}-mensaje`
  const idContador = `${id}-contador`
  const mensaje = error?.message
  const { ref: refRegistro, ...restoRegistro } = registro
  const { largo, ref } = useLargoControl<HTMLInputElement>(contarDigitos, refRegistro)
  const describe = [mensaje || ayuda ? idMensaje : '', largoExacto ? idContador : ''].filter(Boolean).join(' ')
  const esContrasena = type === 'password'
  const [visible, setVisible] = useState(false)

  return (
    <div className={`grid content-start gap-2 ${className}`}>
      {etiquetaOculta ? (
        <label htmlFor={id} className="sr-only">
          {etiqueta}
        </label>
      ) : (
        <Etiqueta htmlFor={id} required={required}>
          {etiqueta}
        </Etiqueta>
      )}
      <div className="relative">
        <input
          id={id}
          type={esContrasena && visible ? 'text' : type}
          aria-invalid={mensaje ? true : undefined}
          aria-describedby={describe || undefined}
          aria-required={required || undefined}
          className={clasesControl(Boolean(mensaje))}
          // Deja lugar al botón de ver la contraseña (el padding de clasesControl es px-3).
          style={esContrasena ? { paddingRight: 'var(--control-height-touch)' } : undefined}
          ref={ref}
          {...restoRegistro}
          {...props}
        />
        {esContrasena && (
          // Dentro de la caja, a la derecha y centrado (.boton-ver-contrasena en index.css).
          <BotonIcono
            etiqueta={visible ? 'Ocultar contraseña' : 'Mostrar contraseña'}
            aria-pressed={visible}
            aria-controls={id}
            disabled={restoRegistro.disabled}
            className="boton-ver-contrasena"
            onClick={() => setVisible((v) => !v)}
          >
            {visible ? <EyeOff aria-hidden size={16} strokeWidth={1.5} /> : <Eye aria-hidden size={16} strokeWidth={1.5} />}
          </BotonIcono>
        )}
      </div>
      {largoExacto ? (
        <div className="flex items-start justify-between gap-4">
          <MensajeCampo id={idMensaje} error={mensaje} ayuda={ayuda} />
          <span className="ml-auto">
            <Contador id={idContador} actual={largo} limite={largoExacto} exacto unidad="dígitos" />
          </span>
        </div>
      ) : (
        <MensajeCampo id={idMensaje} error={mensaje} ayuda={ayuda} />
      )}
      {children}
    </div>
  )
}
