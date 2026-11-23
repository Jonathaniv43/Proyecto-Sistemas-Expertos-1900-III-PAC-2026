/**
 * Página de inicio provisional. El formulario para registrar la empresa
 * y su primer administrador se agrega con HU-017.
 */
export function LandingPage() {
  return (
    <section className="mx-auto flex max-w-layout-narrow flex-col gap-6 py-16">
      <h1 className="text-2xl font-semibold tracking-tight">Norvia</h1>
      <p className="text-md text-text-secondary">
        Gestión de la seguridad y salud en el trabajo para empresas hondureñas.
      </p>
      <div className="rounded-lg border border-border-default bg-bg-surface p-6">
        <p>Muy pronto vas a poder registrar tu empresa desde aquí.</p>
      </div>
    </section>
  )
}
