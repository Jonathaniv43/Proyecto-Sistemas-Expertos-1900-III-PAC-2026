import { createBrowserRouter } from 'react-router-dom'
import { LandingPage } from '@/features/landing/pages/LandingPage'
import { AppLayout } from '@/shared/components/AppLayout'
import { NotFoundPage } from '@/shared/components/NotFoundPage'

/**
 * Rutas de la app. Solo app/ importa páginas de las features.
 * Las rutas protegidas (guardas por sesión y por permiso) se agregan con HU-102.
 */
export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    children: [
      { index: true, element: <LandingPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
