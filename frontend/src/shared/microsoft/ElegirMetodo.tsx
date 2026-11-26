import { useState } from 'react'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { iniciarConMicrosoft, microsoftConfigurado, type PropositoMicrosoft } from './flujo'
import { LogoMicrosoft } from './LogoMicrosoft'

interface ElegirMetodoProps {
  proposito: PropositoMicrosoft
  /** Empresa a la que se quiere entrar (solo en el login). */
  idEmpresa?: string
  /** La persona prefiere correo y contraseña (el flujo de siempre). */
  onCorreo: () => void
}

/**
 * Pantalla previa del login y del registro (design-system.md § Elegir cómo entrar): Microsoft arriba como
 * botón secundario, el separador "o" y "Continuar con correo" como único botón primario.
 */
export function ElegirMetodo({ proposito, idEmpresa, onCorreo }: ElegirMetodoProps) {
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState<string>()

  async function continuarConMicrosoft() {
    setError(undefined)
    if (!microsoftConfigurado) {
      setError('El acceso con Microsoft no está disponible por ahora. Continúa con tu correo.')
      return
    }
    setCargando(true)
    try {
      await iniciarConMicrosoft(proposito, idEmpresa)
    } catch {
      setCargando(false)
      setError('No pudimos abrir Microsoft. Intenta de nuevo o continúa con tu correo.')
    }
  }

  return (
    <div className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      {error && <Aviso tipo="error">{error}</Aviso>}
      <Boton variante="secundario" className="w-full" cargando={cargando} textoCargando="Abriendo Microsoft…" onClick={() => void continuarConMicrosoft()}>
        <LogoMicrosoft />
        Continuar con Microsoft
      </Boton>
      <div className="flex items-center gap-4 text-sm text-text-tertiary" aria-hidden="true">
        <span className="h-px flex-1 bg-border-default" />o<span className="h-px flex-1 bg-border-default" />
      </div>
      <Boton className="w-full" onClick={onCorreo}>
        Continuar con correo
      </Boton>
    </div>
  )
}
