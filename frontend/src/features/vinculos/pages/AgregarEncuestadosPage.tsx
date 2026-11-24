import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useSesion } from '@/shared/sesion'
import { Volver } from '@/shared/components/ui/Volver'
import { FormularioEncuestado } from '../components/FormularioEncuestado'
import { LoteEncuestados, type ResultadoEnvio } from '../components/LoteEncuestados'
import { PersonaAgregada } from '../components/PersonaAgregada'
import { ResultadoLote } from '../components/ResultadoLote'
import type { CrearEncuestadoResponse } from '../types/vinculos'

type Forma = 'uno' | 'varios'

/**
 * HU-115: el administrador da acceso como encuestados a los trabajadores, de a uno o con una lista.
 * Las cuentas nuevas reciben una contraseña temporal (generada por Norvia) que cambian al entrar.
 */
export function AgregarEncuestadosPage() {
  const sesion = useSesion()
  const empresa = sesion.estado === 'activa' ? sesion.usuario.empresa : null
  const [forma, setForma] = useState<Forma>('varios')
  const [uno, setUno] = useState<CrearEncuestadoResponse>()
  const [lote, setLote] = useState<ResultadoEnvio>()
  const [textoLote, setTextoLote] = useState('')
  // Cambia la key para reiniciar el formulario o la lista al volver a empezar.
  const [ronda, setRonda] = useState(0)

  const reiniciar = (texto = '') => {
    setUno(undefined)
    setLote(undefined)
    setTextoLote(texto)
    setRonda((r) => r + 1)
  }

  const titulo = uno ? 'Encuestado agregado' : lote ? 'Resultado' : 'Agregar encuestados'

  return (
    <div className="mx-auto grid w-full max-w-layout-narrow gap-6">
      <Volver a="/personas" texto="Volver a personas" titulo={titulo} />
      <header className="grid gap-2">
        <h1 className="text-xl font-semibold">{titulo}</h1>
        {!uno && !lote && (
          <p className="text-text-secondary">
            Los encuestados responden las encuestas de {empresa?.razonSocial ?? 'la empresa'}, consultan las políticas
            vigentes y reportan eventos de forma anónima. Si alguien ya usa Norvia, se reutiliza su cuenta.
          </p>
        )}
      </header>

      {uno ? (
        <PersonaAgregada
          persona={uno}
          contrasenaTemporal={uno.passwordTemporal ?? ''}
          empresa={empresa}
          textoAgregarOtra="Agregar otro encuestado"
          onAgregarOtra={() => reiniciar()}
        />
      ) : lote ? (
        <ResultadoLote resultado={lote} empresa={empresa} onCorregir={(texto) => reiniciar(texto)} onNuevaLista={() => reiniciar()} />
      ) : (
        <>
          <fieldset className="grid gap-2">
            <legend className="mb-2 text-sm font-medium">¿Cuántas personas vas a agregar?</legend>
            <div className="grid justify-items-start sm:gap-3">
              {(
                [
                  ['varios', 'Varias a la vez', 'Pega una lista desde Excel o carga un archivo CSV.'],
                  ['uno', 'Una persona', 'Escribe sus datos en un formulario.'],
                ] as const
              ).map(([valor, texto, descripcion]) => (
                <label key={valor} className="radio min-h-control-touch items-start max-sm:py-2 sm:min-h-auto">
                  <input type="radio" name="forma" value={valor} checked={forma === valor} onChange={() => setForma(valor)} />
                  <span className="flex h-lh items-center">
                    <span aria-hidden className="radio-circulo" />
                  </span>
                  <span className="grid">
                    {texto}
                    <span className="text-xs text-text-secondary">{descripcion}</span>
                  </span>
                </label>
              ))}
            </div>
          </fieldset>

          {forma === 'uno' ? (
            <FormularioEncuestado key={ronda} onCreado={setUno} />
          ) : (
            <LoteEncuestados key={ronda} textoInicial={textoLote} onEnviado={setLote} />
          )}
          <Link to="/personas">Cancelar</Link>
        </>
      )}
    </div>
  )
}
