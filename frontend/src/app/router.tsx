import { createBrowserRouter } from 'react-router-dom'
import { CambiarContrasenaPage } from '@/features/auth/pages/CambiarContrasenaPage'
import { LoginPage } from '@/features/auth/pages/LoginPage'
import { LoginPlataformaPage } from '@/features/auth/pages/LoginPlataformaPage'
import { RestablecerContrasenaPage } from '@/features/auth/pages/RestablecerContrasenaPage'
import { SolicitarCambioContrasenaPage } from '@/features/auth/pages/SolicitarCambioContrasenaPage'
import { VerificarCuentaPage } from '@/features/auth/pages/VerificarCuentaPage'
import { PerfilEmpresaPage } from '@/features/empresas/pages/PerfilEmpresaPage'
import { RegistroEmpresaPage } from '@/features/empresas/pages/RegistroEmpresaPage'
import { InicioPage } from '@/features/inicio/pages/InicioPage'
import { EncuestaPage } from '@/features/encuestas/pages/EncuestaPage'
import { EncuestasPage } from '@/features/encuestas/pages/EncuestasPage'
import { MisEncuestasPage } from '@/features/encuestas/pages/MisEncuestasPage'
import { ResponderEncuestaPage } from '@/features/encuestas/pages/ResponderEncuestaPage'
import { ResultadosEncuestaPage } from '@/features/encuestas/pages/ResultadosEncuestaPage'
import { NuevaEncuestaPage } from '@/features/encuestas/pages/NuevaEncuestaPage'
import { LandingPage } from '@/features/landing/pages/LandingPage'
import { NuevoRiesgoPage } from '@/features/riesgos/pages/NuevoRiesgoPage'
import { RiesgoPage } from '@/features/riesgos/pages/RiesgoPage'
import { RiesgosPage } from '@/features/riesgos/pages/RiesgosPage'
import { AgregarEncuestadosPage } from '@/features/vinculos/pages/AgregarEncuestadosPage'
import { AgregarPersonaPage } from '@/features/vinculos/pages/AgregarPersonaPage'
import { PersonasPage } from '@/features/vinculos/pages/PersonasPage'
import { AppLayout } from '@/shared/components/AppLayout'
import { ErrorInesperado } from '@/shared/components/ErrorInesperado'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { registrarHistorial } from '@/shared/components/ui/historialVolver'
import { ROLES, ROLES_EMPRESA, ROLES_GESTION } from '@/shared/sesion'
import { RequiereRol } from './rutas/RequiereRol'
import { RutaPrivada } from './rutas/RutaPrivada'
import { SoloSinSesion } from './rutas/SoloSinSesion'

/**
 * Rutas de la app. Solo app/ importa páginas de las features.
 *
 * - Públicas: landing.
 * - SoloSinSesion: login, registro de empresa y activación de cuenta (con sesión activa redirige).
 * - RutaPrivada: requieren sesión; sin ella llevan al login.
 * - RequiereRol (./rutas/RequiereRol, dentro de RutaPrivada): solo para ciertos roles, p. ej.
 *   { element: <RequiereRol roles={ROLES_GESTION} />, children: [{ path: 'personas', element: <PersonasPage /> }] }
 */
export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    errorElement: <ErrorInesperado />,
    children: [
      { index: true, element: <LandingPage /> },
      // HU-022: el enlace del correo; funciona con o sin sesión.
      { path: 'restablecer-contrasena', element: <RestablecerContrasenaPage /> },
      {
        element: <SoloSinSesion />,
        children: [
          { path: 'login', element: <LoginPage /> },
          { path: 'plataforma', element: <LoginPlataformaPage /> },
          { path: 'registro', element: <RegistroEmpresaPage /> },
          { path: 'verificar-cuenta', element: <VerificarCuentaPage /> },
          { path: 'recuperar-contrasena', element: <SolicitarCambioContrasenaPage /> },
        ],
      },
      {
        element: <RutaPrivada />,
        children: [
          { path: 'inicio', element: <InicioPage /> },
          { path: 'cambiar-contrasena', element: <CambiarContrasenaPage /> },
          { path: 'cuenta/contrasena', element: <SolicitarCambioContrasenaPage /> },
          {
            element: <RequiereRol roles={[ROLES.adminEmpresa]} />,
            children: [
              { path: 'personas', element: <PersonasPage /> },
              { path: 'personas/nueva', element: <AgregarPersonaPage /> },
              { path: 'personas/encuestados', element: <AgregarEncuestadosPage /> },
            ],
          },
          {
            element: <RequiereRol roles={ROLES_EMPRESA} />,
            children: [
              { path: 'mis-encuestas', element: <MisEncuestasPage /> },
              { path: 'mis-encuestas/:id', element: <ResponderEncuestaPage /> },
            ],
          },
          {
            element: <RequiereRol roles={ROLES_GESTION} />,
            children: [
              { path: 'empresa', element: <PerfilEmpresaPage /> },
              { path: 'riesgos', element: <RiesgosPage /> },
              { path: 'riesgos/nuevo', element: <NuevoRiesgoPage /> },
              { path: 'riesgos/:id', element: <RiesgoPage /> },
              { path: 'encuestas', element: <EncuestasPage /> },
              { path: 'encuestas/nueva', element: <NuevaEncuestaPage /> },
              { path: 'encuestas/:id', element: <EncuestaPage /> },
              { path: 'encuestas/:id/resultados', element: <ResultadosEncuestaPage /> },
            ],
          },
        ],
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])

// "Volver" regresa a la pantalla anterior: se registra de dónde se llegó a cada entrada del historial.
registrarHistorial(router)
