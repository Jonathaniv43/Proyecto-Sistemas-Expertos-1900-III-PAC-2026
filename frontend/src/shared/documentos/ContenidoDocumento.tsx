import type { ReactNode } from 'react'
import { leerMarkdown, type Linea } from './markdown'

/**
 * Contenido de una política o un procedimiento (design-system.md § Documento). Convierte el Markdown en elementos de
 * React sin interpretar HTML (16-arquitectura §9). Los `[Completar: …]` se resaltan para que se vean al revisar.
 */
export function ContenidoDocumento({ contenido }: { contenido: string }) {
  return (
    <div className="grid gap-4 text-md text-text-primary">
      {leerMarkdown(contenido).map((bloque, i) => {
        switch (bloque.tipo) {
          case 'titulo':
            if (bloque.nivel === 1)
              return (
                <h2 key={i} className="text-lg font-semibold tracking-tight">
                  {enLinea(bloque.contenido)}
                </h2>
              )
            if (bloque.nivel === 2)
              return (
                <h3 key={i} className="mt-4 text-md font-semibold first:mt-0">
                  {enLinea(bloque.contenido)}
                </h3>
              )
            return (
              <h4 key={i} className="text-md font-medium">
                {enLinea(bloque.contenido)}
              </h4>
            )
          case 'parrafo':
            return (
              <p key={i} className="wrap-break-word">
                {enLinea(bloque.contenido)}
              </p>
            )
          case 'lista': {
            const Etiqueta = bloque.ordenada ? 'ol' : 'ul'
            return (
              <Etiqueta
                key={i}
                start={bloque.ordenada && bloque.inicio !== 1 ? bloque.inicio : undefined}
                className={`space-y-2 pl-6 ${bloque.ordenada ? 'list-decimal' : 'list-disc'}`}
              >
                {bloque.items.map((item, j) => (
                  <li key={j} className="wrap-break-word pl-1">
                    {enLinea(item)}
                  </li>
                ))}
              </Etiqueta>
            )
          }
        }
      })}
    </div>
  )
}

function enLinea(partes: Linea[]): ReactNode {
  return partes.map((parte, i) => {
    switch (parte.tipo) {
      case 'texto':
        return parte.texto
      case 'negrita':
        return (
          <strong key={i} className="font-semibold">
            {enLinea(parte.hijos)}
          </strong>
        )
      case 'cursiva':
        return <em key={i}>{enLinea(parte.hijos)}</em>
      case 'completar':
        return (
          <mark key={i} className="rounded-sm border border-warning-border bg-warning-bg px-1 text-warning-text">
            {`[Completar: ${parte.texto}]`}
          </mark>
        )
    }
  })
}
