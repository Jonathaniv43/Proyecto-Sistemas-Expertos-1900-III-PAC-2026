import { createBrowserRouter } from 'react-router-dom'
import { CambiarContrasenaPage } from '@/features/auth/pages/CambiarContrasenaPage'
import { LoginPage } from '@/features/auth/pages/LoginPage'
import { LoginPlataformaPage } from '@/features/auth/pages/LoginPlataformaPage'
import { VerificarCuentaPage } from '@/features/auth/pages/VerificarCuentaPage'
import { RegistroEmpresaPage } from '@/features/empresas/pages/RegistroEmpresaPage'
import { InicioPage } from '@/features/inicio/pages/InicioPage'
import { EncuestaPage } from '@/features/encuestas/pages/EncuestaPage'
import { EncuestasPage } from '@/features/encuestas/pages/EncuestasPage'
import { NuevaEncuestaPage } from '@/features/encuestas/pages/NuevaEncuestaPage'
import { LandingPage } from '@/features/landing/pages/LandingPage'
import { AgregarEncuestadosPage } from '@/features/vinculos/pages/AgregarEncuestadosPage'
import { AgregarPersonaPage } from '@/features/vinculos/pages/AgregarPersonaPage'
import { AppLayout } from '@/shared/components/AppLayout'
import { ErrorInesperado } from '@/shared/components/ErrorInesperado'
import { NotFoundPage } from '@/shared/components/NotFoundPage'
import { ROLES, ROLES_GESTION } from '@/shared/sesion'
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
      {
        element: <SoloSinSesion />,
        children: [
          { path: 'login', element: <LoginPage /> },
          { path: 'plataforma', element: <LoginPlataformaPage /> },
          { path: 'registro', element: <RegistroEmpresaPage /> },
          { path: 'verificar-cuenta', element: <VerificarCuentaPage /> },
        ],
      },
      {
        element: <RutaPrivada />,
        children: [
          { path: 'inicio', element: <InicioPage /> },
          { path: 'cambiar-contrasena', element: <CambiarContrasenaPage /> },
          {
            element: <RequiereRol roles={[ROLES.adminEmpresa]} />,
            children: [
              { path: 'personas/nueva', element: <AgregarPersonaPage /> },
              { path: 'personas/encuestados', element: <AgregarEncuestadosPage /> },
            ],
          },
          {
            element: <RequiereRol roles={ROLES_GESTION} />,
            children: [
              { path: 'encuestas', element: <EncuestasPage /> },
              { path: 'encuestas/nueva', element: <NuevaEncuestaPage /> },
              { path: 'encuestas/:id', element: <EncuestaPage /> },
            ],
          },
        ],
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
