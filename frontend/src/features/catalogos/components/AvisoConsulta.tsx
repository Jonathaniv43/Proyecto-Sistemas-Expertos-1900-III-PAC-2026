import { Aviso } from '@/shared/components/ui/Aviso'
import { usePuedeEditar } from '../permisos'

/** Soporte de plataforma solo consulta los catálogos (HU-023): se avisa en las listas y en cada detalle. */
export function AvisoConsulta() {
  const puedeEditar = usePuedeEditar()
  if (puedeEditar) return null
  return <Aviso tipo="info">Tienes acceso de consulta: solo el administrador de plataforma hace cambios.</Aviso>
}
