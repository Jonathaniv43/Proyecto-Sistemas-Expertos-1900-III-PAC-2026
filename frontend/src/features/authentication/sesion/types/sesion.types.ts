/** Espejo de SesionResponseDto (.NET). Compartido por login y refresh. */
export interface SesionResponseDto {
  accessToken: string
  refreshToken: string
  expiresIn: number
  debeCambiarPassword: boolean
}
