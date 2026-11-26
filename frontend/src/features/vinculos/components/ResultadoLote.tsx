import { CircleAlert, Download } from 'lucide-react'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import type { EmpresaSesion } from '@/shared/api'
import { descargarCsv } from '../lote/descargarCsv'
import type { ResultadoEnvio } from './LoteEncuestados'

interface ResultadoLoteProps {
  resultado: ResultadoEnvio
  empresa: EmpresaSesion | null
  /** Vuelve al cuadro de la lista con las líneas que hay que corregir. */
  onCorregir: (texto: string) => void
  onNuevaLista: () => void
}

/** Quiénes recibieron acceso como encuestados y qué filas requieren corrección (HU-115). */
export function ResultadoLote({ resultado, empresa, onCorregir, onNuevaLista }: ResultadoLoteProps) {
  const { respuesta, enviadas, omitidas } = resultado
  const lineaDe = (fila: number) => enviadas[fila - 1]

  // Filas con error: las que rechazó el backend y las que no se enviaron, en el orden de la lista.
  const errores = [
    ...respuesta.errores.map((e) => ({ linea: lineaDe(e.fila), correo: e.email, motivo: mensajeFila(e.codigo, e.mensaje) })),
    ...omitidas.map((l) => ({ linea: l, correo: l.texto.split(/[\t;,]/)[0].trim(), motivo: l.error ?? '' })),
  ].sort((a, b) => a.linea.linea - b.linea.linea)

  const nuevas = respuesta.cuentas.filter((c) => c.cuentaNueva)

  const descargar = () =>
    descargarCsv('contraseñas-encuestados.csv', [
      ['Correo', 'Nombre', 'Contraseña temporal', 'RTN de la empresa'],
      ...nuevas.map((c) => [c.email, c.nombreCompleto, c.passwordTemporal ?? '', empresa?.rtn ?? '']),
    ])

  return (
    <div className="grid gap-6">
      {respuesta.creados > 0 ? (
        <Aviso tipo="exito" titulo={respuesta.creados === 1 ? '1 encuestado recibió acceso' : `${respuesta.creados} encuestados recibieron acceso`}>
          Ya pueden entrar a {empresa?.razonSocial ?? 'la empresa'} a responder encuestas.
        </Aviso>
      ) : (
        <Aviso tipo="advertencia" titulo="Nadie recibió acceso">
          Ninguna fila se pudo agregar. Revisa los motivos abajo.
        </Aviso>
      )}
      {errores.length > 0 && respuesta.creados > 0 && (
        <Aviso tipo="advertencia">
          {errores.length === 1 ? '1 fila requiere corrección.' : `${errores.length} filas requieren corrección.`}
        </Aviso>
      )}

      {respuesta.cuentas.length > 0 && (
        <section className="grid gap-3" aria-labelledby="titulo-con-acceso">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h2 id="titulo-con-acceso" className="text-md font-semibold">
              Con acceso
            </h2>
            {nuevas.length > 0 && (
              <Boton variante="secundario" onClick={descargar}>
                <Download aria-hidden size={16} strokeWidth={1.5} />
                Descargar contraseñas (CSV)
              </Boton>
            )}
          </div>
          {nuevas.length > 0 && (
            <p className="text-text-secondary">
              Entrega a cada persona su correo, su contraseña temporal
              {empresa ? <> y el RTN {empresa.rtn}</> : null}. La cambiarán la primera vez que entren. Las contraseñas no se
              volverán a mostrar: descárgalas antes de salir.
            </p>
          )}
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Línea</th>
                  <th scope="col">Correo</th>
                  <th scope="col">Nombre</th>
                  <th scope="col">Contraseña temporal</th>
                </tr>
              </thead>
              <tbody>
                {respuesta.cuentas.map((c) => (
                  <tr key={c.vinculoId}>
                    <td className="tabular-nums text-text-secondary">{lineaDe(c.fila).linea}</td>
                    <td>{c.email}</td>
                    <td>{c.nombreCompleto}</td>
                    <td>
                      {c.cuentaNueva ? (
                        <code className="rounded-sm bg-bg-subtle px-2 py-1 font-medium tracking-wide">{c.passwordTemporal}</code>
                      ) : (
                        <span className="text-text-secondary">Ya tenía cuenta: usa su contraseña</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {errores.length > 0 && (
        <section className="grid gap-3" aria-labelledby="titulo-por-corregir">
          <h2 id="titulo-por-corregir" className="text-md font-semibold">
            Por corregir
          </h2>
          <div className="tabla-contenedor">
            <table className="tabla">
              <thead>
                <tr>
                  <th scope="col">Línea</th>
                  <th scope="col">Correo</th>
                  <th scope="col">Motivo</th>
                </tr>
              </thead>
              <tbody>
                {errores.map((e) => (
                  <tr key={e.linea.linea}>
                    <td className="tabular-nums text-text-secondary">{e.linea.linea}</td>
                    <td>{e.correo || '—'}</td>
                    <td className="envolver">
                      <span className="flex items-start gap-1 text-error-text">
                        <span aria-hidden className="flex h-lh shrink-0 items-center text-error-icon">
                          <CircleAlert size={14} strokeWidth={1.5} />
                        </span>
                        {e.motivo}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      <div className="flex flex-col gap-3 sm:flex-row">
        {errores.length > 0 && (
          <Boton variante="secundario" onClick={() => onCorregir(errores.map((e) => e.linea.texto).join('\n'))}>
            Corregir las filas con error
          </Boton>
        )}
        <Boton variante="secundario" onClick={onNuevaLista}>
          Agregar otra lista
        </Boton>
      </div>
    </div>
  )
}

/** Mensajes del backend por fila; el de acceso duplicado se acorta (aún no hay pantalla para editar accesos). */
function mensajeFila(codigo: string, mensaje: string) {
  return codigo === 'vinculos.duplicado' ? 'Ya tiene acceso a esta empresa.' : mensaje
}
