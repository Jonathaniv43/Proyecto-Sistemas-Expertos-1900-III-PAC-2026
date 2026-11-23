import { Boton } from '@/shared/components/ui/Boton'
import { cerrarSesion } from '@/shared/sesion'
import { useState } from 'react'

/** Primera pantalla después de iniciar sesión. Provisional: el menú y el contenido llegan con cada módulo. */
export function InicioPage() {
  const [cerrando, setCerrando] = useState(false)

  return (
    <section className="flex flex-col items-start gap-6">
      <h1 className="text-xl font-semibold">Inicio</h1>
      <Boton
        variante="secundario"
        cargando={cerrando}
        textoCargando="Cerrando sesión…"
        onClick={async () => {
          setCerrando(true)
          await cerrarSesion()
        }}
      >
        Cerrar sesión
      </Boton>
    </section>
  )
}
