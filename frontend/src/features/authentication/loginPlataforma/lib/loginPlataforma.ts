import { sesionStore } from '../../sesion/store/sesionStore'
import { postLoginPlataforma } from '../api/loginPlataformaApi'
import type { LoginPlataformaRequestDto } from '../types/loginPlataforma.types'
import type { SesionResponseDto } from '../../sesion/types/sesion.types'

export async function loginPlataformaAsync(
  datos: LoginPlataformaRequestDto,
): Promise<SesionResponseDto> {
  const sesion = await postLoginPlataforma(datos)
  sesionStore.aplicarSesion(sesion)
  return sesion
}
