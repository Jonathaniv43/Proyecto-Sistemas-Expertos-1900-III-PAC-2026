import { useMemo, useState } from 'react'

const VISIBLES = 8
const comparar = new Intl.Collator('es', { sensitivity: 'base' }).compare

/**
 * Respuestas breves sin autor. Se muestran en orden alfabético y no en el que se guardaron: así el
 * orden no permite adivinar quién escribió cada una (anonimato, RN-72).
 */
export function RespuestasBreves({ textos }: { textos: string[] }) {
  const [todas, setTodas] = useState(false)
  const ordenadas = useMemo(() => [...textos].sort(comparar), [textos])
  const visibles = todas ? ordenadas : ordenadas.slice(0, VISIBLES)

  if (ordenadas.length === 0) return <p className="text-text-secondary">Nadie respondió esta pregunta.</p>

  return (
    <div className="grid gap-2">
      <ul className="grid gap-2">
        {visibles.map((t, i) => (
          <li key={i} className="rounded-md bg-bg-subtle px-3 py-2 whitespace-pre-line">
            {t}
          </li>
        ))}
      </ul>
      {ordenadas.length > VISIBLES && (
        <div>
          <button
            type="button"
            className="inline-flex min-h-control-touch items-center text-sm font-medium text-text-link hover:text-text-link-hover sm:min-h-control"
            onClick={() => setTodas((v) => !v)}
          >
            {todas ? 'Ver menos' : `Ver las ${ordenadas.length} respuestas`}
          </button>
        </div>
      )}
    </div>
  )
}
