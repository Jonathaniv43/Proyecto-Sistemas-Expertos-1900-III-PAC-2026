import { Check, Copy } from 'lucide-react'
import { useState } from 'react'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { etiquetaRol } from '@/shared/sesion'
import type { CrearVinculoResponse } from '../types/vinculos'

interface PersonaAgregadaProps {
  persona: CrearVinculoResponse
  /** La contraseña temporal enviada; solo se muestra si se creó la cuenta. */
  contrasenaTemporal: string
  empresa: { razonSocial: string; rtn: string } | null
  onAgregarOtra: () => void
  textoAgregarOtra?: string
}

/** Confirmación del alta: la persona ya tiene acceso a la empresa con el rol elegido. */
export function PersonaAgregada({
  persona,
  contrasenaTemporal,
  empresa,
  onAgregarOtra,
  textoAgregarOtra = 'Agregar otra persona',
}: PersonaAgregadaProps) {
  const [copiada, setCopiada] = useState(false)
  const rol = etiquetaRol(persona.rolCodigo)
  const lugar = empresa?.razonSocial ?? 'la empresa'

  const copiar = async () => {
    try {
      await navigator.clipboard.writeText(contrasenaTemporal)
      setCopiada(true)
    } catch {
      // Sin permiso para el portapapeles: la contraseña sigue visible para copiarla a mano.
    }
  }

  return (
    <section className="grid gap-6" aria-labelledby="titulo-persona-agregada">
      <Aviso tipo="exito" titulo={`${persona.nombreCompleto} ya tiene acceso`}>
        Entra a {lugar} como {rol}.
      </Aviso>

      <div className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
        <h2 id="titulo-persona-agregada" className="text-md font-semibold">
          Cómo entra
        </h2>
        {persona.cuentaNueva ? (
          <>
            <p className="text-text-secondary">
              Creamos su cuenta en Norvia. Entrégale estos datos por un medio seguro. La primera vez que entre, tendrá que
              cambiar la contraseña temporal por una propia.
            </p>
            <dl className="grid gap-3 sm:grid-cols-2">
              <div className="grid gap-1">
                <dt className="text-xs text-text-secondary">Correo</dt>
                <dd className="font-medium break-all">{persona.email}</dd>
              </div>
              {empresa && (
                <div className="grid gap-1">
                  <dt className="text-xs text-text-secondary">RTN de la empresa</dt>
                  <dd className="font-medium tabular-nums">{empresa.rtn}</dd>
                </div>
              )}
              <div className="grid gap-1 sm:col-span-2">
                <dt className="text-xs text-text-secondary">Contraseña temporal</dt>
                <dd className="flex flex-wrap items-center gap-3">
                  <code className="rounded-sm bg-bg-subtle px-2 py-1 font-medium tracking-wide">{contrasenaTemporal}</code>
                  <Boton variante="secundario" onClick={() => void copiar()}>
                    {copiada ? <Check aria-hidden size={16} strokeWidth={1.5} /> : <Copy aria-hidden size={16} strokeWidth={1.5} />}
                    {copiada ? 'Copiada' : 'Copiar'}
                  </Boton>
                </dd>
              </div>
            </dl>
            <Aviso tipo="advertencia">Esta contraseña no se volverá a mostrar. Cópiala antes de salir de esta pantalla.</Aviso>
          </>
        ) : (
          <p className="text-text-secondary">
            <strong className="font-medium text-text-primary">{persona.email}</strong> ya tenía una cuenta en Norvia, así
            que entra con su contraseña de siempre{empresa ? <> usando el RTN {empresa.rtn}</> : null}. No se creó una cuenta
            nueva.
          </p>
        )}
      </div>

      <div>
        <Boton variante="secundario" onClick={onAgregarOtra}>
          {textoAgregarOtra}
        </Boton>
      </div>
    </section>
  )
}
