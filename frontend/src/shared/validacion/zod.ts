import { z } from 'zod'

/**
 * Configuración global de Zod. Se importa una sola vez desde main.tsx.
 *
 * Los mensajes dicen qué corregir, con el tono del design system ("Ingresa…", "Revisa…").
 * Cada regla puede traer su propio mensaje; este mapa cubre los casos genéricos y, si no
 * reconoce el error, cae en el locale español de Zod.
 */
z.config(z.locales.es())

const unidad = (origen: string | undefined) =>
  origen === 'array' || origen === 'set' ? 'opciones' : 'caracteres'

z.config({
  customError: (issue) => {
    switch (issue.code) {
      case 'invalid_type':
        return issue.input === undefined || issue.input === null || issue.input === ''
          ? 'Este campo es obligatorio.'
          : 'Revisa el formato de este campo.'
      case 'too_small':
        if (issue.origin === 'string' && issue.minimum === 1) return 'Este campo es obligatorio.'
        if (issue.origin === 'array' && issue.minimum === 1) return 'Elige al menos una opción.'
        if (issue.origin === 'string' || issue.origin === 'array' || issue.origin === 'set')
          return `Debe tener al menos ${issue.minimum} ${unidad(issue.origin)}.`
        return `Ingresa un valor mayor o igual a ${issue.minimum}.`
      case 'too_big':
        if (issue.origin === 'string' || issue.origin === 'array' || issue.origin === 'set')
          return `Debe tener como máximo ${issue.maximum} ${unidad(issue.origin)}.`
        return `Ingresa un valor menor o igual a ${issue.maximum}.`
      case 'invalid_format':
        if (issue.format === 'email') return 'Ingresa un correo válido, por ejemplo nombre@empresa.com.'
        if (issue.format === 'url') return 'Ingresa una dirección web válida, por ejemplo https://empresa.com.'
        return 'Revisa el formato de este campo.'
      case 'invalid_value':
        return 'Elige una de las opciones disponibles.'
      default:
        return undefined
    }
  },
})

export { z }
