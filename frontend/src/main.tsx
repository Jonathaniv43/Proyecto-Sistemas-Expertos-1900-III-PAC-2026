import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { registrarInterceptoresSesion } from './shared/api/registrarInterceptoresSesion'
import { proveerInterceptoresSesion } from './features/authentication/sesion/lib/proveerInterceptoresSesion'

registrarInterceptoresSesion(proveerInterceptoresSesion())

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
