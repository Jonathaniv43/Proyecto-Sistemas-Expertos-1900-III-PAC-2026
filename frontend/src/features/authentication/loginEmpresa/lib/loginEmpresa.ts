import { sesionStore } from '../../sesion/store/sesionStore'
import { postLoginEmpresa } from '../api/loginEmpresaApi'
import type { LoginEmpresaRequestDto } from '../types/loginEmpresa.types'
import type { SesionResponseDto } from '../../sesion/types/sesion.types'

export async function loginEmpresaAsync(
  datos: LoginEmpresaRequestDto,
): Promise<SesionResponseDto> {
  const sesion = await postLoginEmpresa(datos)
  sesionStore.aplicarSesion(sesion)
  return sesion
}
