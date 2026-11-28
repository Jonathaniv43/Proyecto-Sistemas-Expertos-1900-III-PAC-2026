import { useSesion } from '@/shared/sesion'
import { BarraLanding } from '../components/BarraLanding'
import { Portada } from '../components/Portada'
import { Cierre, ComoFunciona, ParaCadaPersona, Pie, Preguntas, Privacidad, Problema, QueCubre } from '../components/Secciones'

/**
 * Landing pública (design-system.md § Landing, aprobada el 28-11-2026): qué problema resuelve Norvia, cómo funciona y
 * qué cubre del reglamento, para que una empresa decida registrarse (HU-017) o inicie sesión. Con sesión iniciada, en
 * lugar de registrar ofrece ir al inicio. Usa su propia barra y franjas de borde a borde: es la excepción del layout
 * público.
 */
export function LandingPage() {
  const sesion = useSesion()
  const usuario = sesion.estado === 'activa' ? sesion.usuario : null

  return (
    <>
      <BarraLanding conSesion={usuario !== null} />
      <main>
        <Portada usuario={usuario} />
        <Problema />
        <ComoFunciona />
        <QueCubre />
        <ParaCadaPersona />
        <Privacidad />
        <Preguntas />
        <Cierre conSesion={usuario !== null} />
      </main>
      <Pie />
    </>
  )
}
