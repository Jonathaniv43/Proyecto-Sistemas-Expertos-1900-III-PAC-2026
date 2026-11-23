import { CircleAlert, CircleCheck, Upload } from 'lucide-react'
import { useMemo, useRef, useState } from 'react'
import { extraerError } from '@/shared/api'
import { AreaTexto } from '@/shared/components/ui/AreaTexto'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { crearEncuestadosLote } from '../api/vinculos'
import { leerLista, type LineaLista } from '../lote/leerLista'
import { MAXIMO_FILAS_LOTE } from '../schemas/encuestados'
import type { CrearEncuestadosLoteResponse } from '../types/vinculos'

export interface ResultadoEnvio {
  respuesta: CrearEncuestadosLoteResponse
  /** Las líneas enviadas, en el mismo orden: la fila N de la respuesta es enviadas[N - 1]. */
  enviadas: LineaLista[]
  /** Líneas que no se enviaron porque tenían errores. */
  omitidas: LineaLista[]
}

const EJEMPLO = 'ana.lopez@empresa.hn, Ana, López\nluis.mejia@empresa.hn, Luis, Mejía'

/**
 * Alta de varios encuestados a la vez (HU-115): se pega la lista (desde Excel o un CSV) y se ve,
 * mientras se escribe, qué filas están listas y cuáles tienen errores. Solo se envían las listas.
 */
export function LoteEncuestados({ textoInicial, onEnviado }: { textoInicial: string; onEnviado: (r: ResultadoEnvio) => void }) {
  const [texto, setTexto] = useState(textoInicial)
  const [enviando, setEnviando] = useState(false)
  const [errorEnvio, setErrorEnvio] = useState<string>()
  const archivo = useRef<HTMLInputElement>(null)

  const lineas = useMemo(() => leerLista(texto), [texto])
  const listas = lineas.filter((l) => l.fila)
  const conError = lineas.filter((l) => l.error)
  const demasiadas = listas.length > MAXIMO_FILAS_LOTE

  const enviar = async () => {
    setEnviando(true)
    setErrorEnvio(undefined)
    try {
      const respuesta = await crearEncuestadosLote(listas.map((l) => l.fila!))
      onEnviado({ respuesta, enviadas: listas, omitidas: conError })
    } catch (causa) {
      setErrorEnvio(extraerError(causa).mensaje)
    } finally {
      setEnviando(false)
    }
  }

  const cargarArchivo = async (seleccionado: File | undefined) => {
    if (seleccionado) setTexto(await seleccionado.text())
    if (archivo.current) archivo.current.value = ''
  }

  return (
    <div className="grid gap-6 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
      <AreaTexto
        etiqueta="Lista de encuestados"
        required
        value={texto}
        onChange={(e) => setTexto(e.target.value)}
        placeholder={EJEMPLO}
        spellCheck={false}
        ayuda={
          <>
            Una persona por línea: <strong className="font-medium">correo, primer nombre, primer apellido</strong>. También
            puedes usar 5 columnas (correo, primer nombre, segundo nombre, primer apellido, segundo apellido) o pegar las
            columnas desde Excel. Hasta {MAXIMO_FILAS_LOTE} personas por envío.
          </>
        }
      />
      <div>
        <input
          ref={archivo}
          type="file"
          accept=".csv,.txt,text/csv,text/plain"
          className="sr-only"
          id="archivo-lista"
          onChange={(e) => void cargarArchivo(e.target.files?.[0])}
        />
        <label htmlFor="archivo-lista" className="inline-flex cursor-pointer items-center gap-2 text-sm font-medium text-text-link hover:text-text-link-hover">
          <Upload aria-hidden size={16} strokeWidth={1.5} />
          Cargar un archivo CSV
        </label>
      </div>

      {lineas.length > 0 && (
        <section className="grid gap-3" aria-labelledby="titulo-vista-previa">
          <h2 id="titulo-vista-previa" className="text-sm font-medium">
            Vista previa: {listas.length} {listas.length === 1 ? 'lista' : 'listas'}
            {conError.length > 0 && `, ${conError.length} con errores (no se enviarán)`}
          </h2>
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Línea</th>
                  <th scope="col">Correo</th>
                  <th scope="col">Nombre</th>
                  <th scope="col">Estado</th>
                </tr>
              </thead>
              <tbody>
                {lineas.map((l) => (
                  <tr key={l.linea}>
                    <td className="tabular-nums text-text-secondary">{l.linea}</td>
                    <td>{l.fila?.email ?? l.texto.split(/[\t;,]/)[0]}</td>
                    <td>{l.fila ? [l.fila.primerNombre, l.fila.segundoNombre, l.fila.primerApellido, l.fila.segundoApellido].filter(Boolean).join(' ') : '—'}</td>
                    <td className="envolver">
                      {l.error ? (
                        <span className="flex items-start gap-1 text-error-text">
                          <span aria-hidden className="flex h-lh shrink-0 items-center text-error-icon">
                            <CircleAlert size={14} strokeWidth={1.5} />
                          </span>
                          {l.error}
                        </span>
                      ) : (
                        <span className="flex items-center gap-1">
                          <CircleCheck aria-hidden size={14} strokeWidth={1.5} className="text-success-icon" />
                          Lista
                        </span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {demasiadas && (
        <Aviso tipo="error">
          La lista tiene {listas.length} personas. Envía como máximo {MAXIMO_FILAS_LOTE} a la vez.
        </Aviso>
      )}
      {errorEnvio && <Aviso tipo="error">{errorEnvio}</Aviso>}

      <div>
        <Boton
          className="w-full sm:w-auto"
          disabled={listas.length === 0 || demasiadas}
          cargando={enviando}
          textoCargando="Agregando…"
          onClick={() => void enviar()}
        >
          {listas.length === 1 ? 'Agregar 1 encuestado' : `Agregar ${listas.length} encuestados`}
        </Boton>
      </div>
    </div>
  )
}
