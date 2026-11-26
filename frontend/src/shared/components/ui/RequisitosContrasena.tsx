import { Check, Circle } from 'lucide-react'
import { REQUISITOS_CONTRASENA } from '@/shared/validacion'

/** Lista de requisitos de la política de contraseñas (HU-114) que se marcan mientras se escribe. */
export function RequisitosContrasena({ valor }: { valor: string }) {
  return (
    <ul className="grid gap-1 text-xs" aria-label="Requisitos de la contraseña">
      {REQUISITOS_CONTRASENA.map((requisito) => {
        const cumple = requisito.cumple(valor)
        return (
          <li key={requisito.texto} className={`flex items-start gap-1 ${cumple ? 'text-text-primary' : 'text-text-secondary'}`}>
            <span aria-hidden className={`flex h-lh shrink-0 items-center ${cumple ? 'text-success-icon' : 'text-text-tertiary'}`}>
              {cumple ? <Check size={14} strokeWidth={1.5} /> : <Circle size={14} strokeWidth={1.5} />}
            </span>
            <span>
              {requisito.texto}
              <span className="sr-only">{cumple ? ': cumplido' : ': pendiente'}</span>
            </span>
          </li>
        )
      })}
    </ul>
  )
}
