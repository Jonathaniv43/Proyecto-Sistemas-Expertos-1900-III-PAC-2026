import { createBrowserRouter, Navigate } from 'react-router-dom'
import { CambiarContrasenaPage } from '@/features/auth/pages/CambiarContrasenaPage'
import { LoginPage } from '@/features/auth/pages/LoginPage'
import { MicrosoftCallbackPage } from '@/features/auth/pages/MicrosoftCallbackPage'
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
import { CatalogosLayout } from '@/features/catalogos/components/CatalogosLayout'
import { CategoriasPage } from '@/features/catalogos/pages/CategoriasPage'
import { DocumentoPage } from '@/features/catalogos/pages/DocumentoPage'
import { DocumentosPage } from '@/features/catalogos/pages/DocumentosPage'
import { PeligroPage } from '@/features/catalogos/pages/PeligroPage'
import { PeligrosPage } from '@/features/catalogos/pages/PeligrosPage'
import { PreguntaPage } from '@/features/catalogos/pages/PreguntaPage'
import { PreguntasPage } from '@/features/catalogos/pages/PreguntasPage'
import { RubrosPage } from '@/features/catalogos/pages/RubrosPage'
import { DocumentoPage as DocumentoEmpresaPage } from '@/features/documentos/pages/DocumentoPage'
import { DocumentosPage as DocumentosEmpresaPage } from '@/features/documentos/pages/DocumentosPage'
import { EditarBorradorPage } from '@/features/documentos/pages/EditarBorradorPage'
import { GenerarDocumentoPage } from '@/features/documentos/pages/GenerarDocumentoPage'
import { EstadisticasEventosPage } from '@/features/eventos/pages/EstadisticasEventosPage'
import { EventoPage } from '@/features/eventos/pages/EventoPage'
import { EventosPage } from '@/features/eventos/pages/EventosPage'
import { ReportarEventoPage } from '@/features/eventos/pages/ReportarEventoPage'
import { MisTareasPage } from '@/features/plan/pages/MisTareasPage'
import { NotificacionesPage } from '@/features/notificaciones/pages/NotificacionesPage'
import { NuevaTareaPage } from '@/features/plan/pages/NuevaTareaPage'
import { PlanTrabajoPage } from '@/features/plan/pages/PlanTrabajoPage'
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
import { ROLES, ROLES_EMPRESA, ROLES_GESTION, ROLES_PLATAFORMA, ROLES_TAREAS } from '@/shared/sesion'
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
          // D29, HU-119/120: retorno de Microsoft (OAuth con PKCE).
          { path: 'auth/microsoft/callback', element: <MicrosoftCallbackPage /> },
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
              // HU-117: la bandeja propia (RN-79).
              { path: 'notificaciones', element: <NotificacionesPage /> },
              // HU-047: cualquier persona de la empresa reporta, de forma anónima.
              { path: 'eventos/reportar', element: <ReportarEventoPage /> },
            ],
          },
          {
            // HU-023: catálogos de la plataforma (staff de Norvia; soporte solo consulta).
            element: <RequiereRol roles={ROLES_PLATAFORMA} />,
            children: [
              {
                path: 'catalogos',
                element: <CatalogosLayout />,
                children: [
                  { index: true, element: <Navigate to="categorias" replace /> },
                  { path: 'categorias', element: <CategoriasPage /> },
                  { path: 'preguntas', element: <PreguntasPage /> },
                  { path: 'preguntas/nueva', element: <PreguntaPage /> },
                  { path: 'preguntas/:id', element: <PreguntaPage /> },
                  { path: 'peligros', element: <PeligrosPage /> },
                  { path: 'peligros/:id', element: <PeligroPage /> },
                  { path: 'rubros', element: <RubrosPage /> },
                  { path: 'documentos', element: <DocumentosPage /> },
                  { path: 'documentos/nuevo', element: <DocumentoPage /> },
                  { path: 'documentos/:id', element: <DocumentoPage /> },
                ],
              },
            ],
          },
          {
            element: <RequiereRol roles={ROLES_TAREAS} />,
            children: [{ path: 'mis-tareas', element: <MisTareasPage /> }],
          },
          {
            element: <RequiereRol roles={ROLES_GESTION} />,
            children: [
              { path: 'empresa', element: <PerfilEmpresaPage /> },
              { path: 'riesgos', element: <RiesgosPage /> },
              { path: 'riesgos/nuevo', element: <NuevoRiesgoPage /> },
              { path: 'riesgos/:id', element: <RiesgoPage /> },
              { path: 'plan-de-trabajo', element: <PlanTrabajoPage /> },
              { path: 'eventos', element: <EventosPage /> },
              { path: 'eventos/estadisticas', element: <EstadisticasEventosPage /> },
              { path: 'eventos/:id', element: <EventoPage /> },
              { path: 'plan-de-trabajo/nueva', element: <NuevaTareaPage /> },
              { path: 'encuestas', element: <EncuestasPage /> },
              { path: 'encuestas/nueva', element: <NuevaEncuestaPage /> },
              { path: 'encuestas/:id', element: <EncuestaPage /> },
              { path: 'encuestas/:id/resultados', element: <ResultadosEncuestaPage /> },
              // HU-070: políticas y procedimientos; el admin consulta.
              { path: 'documentos', element: <DocumentosEmpresaPage /> },
              { path: 'documentos/:id', element: <DocumentoEmpresaPage /> },
            ],
          },
          {
            // HU-070 y HU-071: solo el Responsable SST genera documentos y edita el borrador.
            element: <RequiereRol roles={[ROLES.responsableSst]} />,
            children: [
              { path: 'documentos/generar', element: <GenerarDocumentoPage /> },
              { path: 'documentos/:id/editar', element: <EditarBorradorPage /> },
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
