/**
 * Lo que falta en un documento: el texto de cada `[Completar: …]`, sin repetir y en orden. Espejo de
 * `Marcadores.PorCompletar` (backend), para contarlos mientras se edita el borrador (HU-071).
 */
export function porCompletar(contenido: string): string[] {
  const vistos = new Map<string, string>()
  for (const m of contenido.matchAll(/\[Completar:\s*([^\]\r\n]+?)\s*\]/g)) {
    const texto = m[1].trim()
    if (!vistos.has(texto.toLowerCase())) vistos.set(texto.toLowerCase(), texto)
  }
  return [...vistos.values()]
}
