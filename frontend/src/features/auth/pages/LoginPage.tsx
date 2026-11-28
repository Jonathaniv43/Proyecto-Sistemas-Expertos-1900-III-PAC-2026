import { useLocation, useSearchParams } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Volver } from '@/shared/components/ui/Volver'
import type { EstadoLogin } from '@/shared/sesion'
import { AvisoMotivo } from '../components/AvisoMotivo'
import { BuscarEmpresa } from '../components/BuscarEmpresa'
import { CredencialesEmpresa } from '../components/CredencialesEmpresa'

/**
 * HU-018: inicio de sesión en una empresa, en dos pasos sobre la misma ruta:
 * - `/login`: se busca la empresa por su RTN (GET /api/empresas/por-rtn) y se confirma su nombre.
 * - `/login?empresa=…`: login de esa empresa (id cifrado), solo con correo y contraseña.
 * Cada ingreso es a una empresa; para otra se inicia otra sesión (HU-019). Al entrar, SoloSinSesion
 * lleva a la ruta pedida o al inicio.
 */
export function LoginPage() {
  const { state } = useLocation()
  const [parametros] = useSearchParams()
  const motivo = (state as EstadoLogin | null)?.motivo
  // Se llegó con un enlace /login?empresa=… que este navegador no puede usar (CredencialesEmpresa).
  const enlaceSinEmpresa = Boolean((state as { enlaceSinEmpresa?: boolean } | null)?.enlaceSinEmpresa)
  const idEmpresa = parametros.get('empresa')?.trim() ?? ''

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      {idEmpresa ? (
        <Volver a="/login" texto="Volver a buscar tu empresa" titulo="Iniciar sesión" />
      ) : (
        <Volver a="/" texto="Volver a la página principal" titulo="Iniciar sesión" />
      )}
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">Iniciar sesión</h1>
        <p className="text-text-secondary">
          {idEmpresa ? 'Entra a tu empresa con tu correo y tu contraseña.' : 'Primero busca tu empresa con su RTN.'}
        </p>
      </header>

      <AvisoMotivo motivo={motivo} />
      {enlaceSinEmpresa && !idEmpresa && (
        <Aviso tipo="info">El enlace que abriste no sirve en este navegador. Busca tu empresa por su RTN para continuar.</Aviso>
      )}

      {idEmpresa ? <CredencialesEmpresa key={idEmpresa} idEmpresa={idEmpresa} /> : <BuscarEmpresa />}
    </div>
  )
}
