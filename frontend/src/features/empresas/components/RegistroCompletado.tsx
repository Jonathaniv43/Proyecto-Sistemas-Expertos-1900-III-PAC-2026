import { Link } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { clasesBoton } from '@/shared/components/ui/clases'
import type { RegistrarEmpresaResponse } from '../types/registro'

/** Confirmación del registro: la empresa existe y quien la registró es su administrador. */
export function RegistroCompletado({ registro }: { registro: RegistrarEmpresaResponse }) {
  const verificar = `/verificar-cuenta?${new URLSearchParams({ correo: registro.email, rtn: registro.rtn })}`

  return (
    <section className="grid gap-6" aria-labelledby="titulo-registro-completado">
      <Aviso tipo="exito" titulo="Tu empresa quedó registrada">
        {registro.razonSocial} ya está en Norvia y tú eres su administrador.
      </Aviso>

      <div className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-6">
        <h2 id="titulo-registro-completado" className="text-md font-semibold">
          Activa tu cuenta
        </h2>
        {registro.correoEnviado ? (
          <p className="text-text-secondary">
            Te enviamos un correo a <strong className="font-medium text-text-primary">{registro.email}</strong> con tu
            contraseña temporal. Úsala para activar tu cuenta y elegir tu contraseña definitiva.
          </p>
        ) : (
          <Aviso tipo="advertencia" titulo="No pudimos enviarte el correo">
            Tu empresa quedó registrada, pero el correo con tu contraseña temporal no se envió. Comunícate con el equipo
            de Norvia para activar tu cuenta.
          </Aviso>
        )}
        <dl className="grid gap-1">
          <dt className="text-xs text-text-secondary">Para iniciar sesión usarás el RTN de tu empresa</dt>
          <dd className="font-medium tabular-nums">{registro.rtn}</dd>
        </dl>
        {registro.correoEnviado && (
          <Link to={verificar} className={`${clasesBoton('primario')} justify-self-start`}>
            Activar mi cuenta
          </Link>
        )}
      </div>
    </section>
  )
}
