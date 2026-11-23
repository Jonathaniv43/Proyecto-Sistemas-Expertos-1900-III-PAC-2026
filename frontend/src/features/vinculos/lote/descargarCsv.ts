/** Descarga un CSV que Excel abre con tildes (BOM UTF-8 y punto y coma). */
export function descargarCsv(nombreArchivo: string, filas: string[][]) {
  const celda = (valor: string) => (/[;"\n]/.test(valor) ? `"${valor.replace(/"/g, '""')}"` : valor)
  const contenido = '﻿' + filas.map((fila) => fila.map(celda).join(';')).join('\r\n')
  const url = URL.createObjectURL(new Blob([contenido], { type: 'text/csv;charset=utf-8' }))
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = nombreArchivo
  enlace.click()
  URL.revokeObjectURL(url)
}
