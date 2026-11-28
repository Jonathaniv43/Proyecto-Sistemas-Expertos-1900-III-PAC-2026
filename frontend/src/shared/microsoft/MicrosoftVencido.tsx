import { useState } from 'react'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { iniciarConMicrosoft, type PropositoMicrosoft } from './flujo'

interface AvisoMicrosoftVencidoProps {
  proposito: PropositoMicrosoft
  /** Empresa a la que se quería entrar (solo en el login). */
  idEmpresa?: string
}

/**
 * La validación con Microsoft venció a mitad del flujo: en vez de un error suelto, ofrece volver a Microsoft
 * conservando qué se estaba haciendo. Como Microsoft recuerda la sesión, suele bastar con elegir la cuenta.
 */
export function AvisoMicrosoftVencido({ proposito, idEmpresa }: AvisoMicrosoftVencidoProps) {
  const [yendo, setYendo] = useState(false)
  return (
    <div className="grid gap-4">
      <Aviso tipo="advertencia">
        Tu sesión con Microsoft venció antes de terminar. Vuelve a entrar con Microsoft para continuar donde ibas.
      </Aviso>
      <Boton
        className="justify-self-start"
        cargando={yendo}
        textoCargando="Abriendo Microsoft…"
        onClick={() => {
          setYendo(true)
          void iniciarConMicrosoft(proposito, idEmpresa).catch(() => setYendo(false))
        }}
      >
        Volver a entrar con Microsoft
      </Boton>
    </div>
  )
}
