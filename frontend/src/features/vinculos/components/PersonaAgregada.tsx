import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { etiquetaRol } from '@/shared/sesion'
import type { CrearVinculoResponse } from '../types/vinculos'

interface PersonaAgregadaProps {
  persona: CrearVinculoResponse
  empresa: { razonSocial: string; rtn: string } | null
  onAgregarOtra: () => void
  textoAgregarOtra?: string
}

/** Confirmación del alta: la persona ya tiene acceso a la empresa con el rol elegido. */
export function PersonaAgregada({ persona, empresa, onAgregarOtra, textoAgregarOtra = 'Agregar otra persona' }: PersonaAgregadaProps) {
  const rol = etiquetaRol(persona.rolCodigo)
  const lugar = empresa?.razonSocial ?? 'la empresa'

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
          persona.correoActivacionEnviado ? (
            <>
              <p className="text-text-secondary">
                Creamos su cuenta en Norvia. Le enviamos un correo a{' '}
                <strong className="font-medium text-text-primary">{persona.email}</strong> con un código de 6 dígitos:
                debe usarlo para activar su cuenta y elegir su propia contraseña.
              </p>
              {empresa && (
                <dl className="grid gap-1">
                  <dt className="text-xs text-text-secondary">RTN de la empresa</dt>
                  <dd className="font-medium tabular-nums">{empresa.rtn}</dd>
                </dl>
              )}
            </>
          ) : (
            <Aviso tipo="advertencia" titulo="No pudimos enviarle el correo">
              Creamos su cuenta, pero el correo con el código de activación no se envió. Pídele que use "Reenviar
              verificación" en la pantalla de activación, o inténtalo de nuevo más tarde.
            </Aviso>
          )
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
