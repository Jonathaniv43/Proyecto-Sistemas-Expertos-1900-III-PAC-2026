import { etiquetaRol, useSesion } from '@/shared/sesion'

/** Primera pantalla después de iniciar sesión. Las funciones de cada módulo se agregan con su HU. */
export function InicioPage() {
  const sesion = useSesion()
  if (sesion.estado !== 'activa') return null
  const { usuario } = sesion

  return (
    <section className="grid gap-2">
      <h1 className="text-xl font-semibold">Inicio</h1>
      <p className="text-text-secondary">
        {usuario.tipo === 'plataforma'
          ? `Entraste a la administración de Norvia como ${etiquetaRol(usuario.rol)}.`
          : `Entraste${usuario.empresa ? ` a ${usuario.empresa.razonSocial}` : ''} como ${etiquetaRol(usuario.rol)}.`}
      </p>
    </section>
  )
}
