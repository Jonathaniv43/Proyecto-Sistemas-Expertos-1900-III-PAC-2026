import { Building2 } from 'lucide-react'
import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { Aviso } from '@/shared/components/ui/Aviso'
import { Boton } from '@/shared/components/ui/Boton'
import { CampoTexto } from '@/shared/components/ui/CampoTexto'
import { estadoVolver } from '@/shared/components/ui/estadoVolver'
import { useFormulario } from '@/shared/forms'
import { leerUltimaEmpresa, recordarEmpresa, rutaActual, rutaLogin, type EstadoLogin } from '@/shared/sesion'
import { resolverEmpresaPorRtn, type EmpresaPorRtn } from '../api/sesion'
import { buscarEmpresaEsquema } from '../schemas/login'

/** Solo lo del login (motivo, a dónde volver al entrar): el "Volver" del paso 2 es siempre a este paso. */
const estadoLogin = (estado: unknown): EstadoLogin => {
  const { desde, motivo, rtn } = (estado as EstadoLogin | null) ?? {}
  return { desde, motivo, rtn }
}

/**
 * Paso 1 del login (HU-018): la persona escribe el RTN y GET /api/empresas/por-rtn devuelve el nombre
 * de la empresa y su `idEmpresa` cifrado. Si es la suya, sigue a `/login?empresa=…` (el backend lo
 * descifra al iniciar sesión). El estado de la ruta (motivo, a dónde volver) se conserva.
 */
export function BuscarEmpresa() {
  const location = useLocation()
  const navigate = useNavigate()
  const [ultima] = useState(leerUltimaEmpresa)
  const [encontrada, setEncontrada] = useState<EmpresaPorRtn>()

  const {
    register,
    formState: { errors, isSubmitting },
    enviar,
    errorGeneral,
  } = useFormulario(buscarEmpresaEsquema, {
    // Vacío: la última empresa ya se ofrece aparte ("Entraste aquí la última vez").
    defaultValues: { rtn: '' },
    erroresPorCodigo: {
      'empresas.rtn_no_encontrado': { campo: 'rtn', mensaje: 'No encontramos una empresa activa con ese RTN.' },
    },
  })

  const continuar = (empresa: EmpresaPorRtn) => {
    recordarEmpresa(empresa)
    navigate(rutaLogin(empresa.idEmpresa), { state: estadoLogin(location.state) })
  }

  if (encontrada) {
    return (
      <section
        aria-labelledby="titulo-empresa-encontrada"
        aria-live="polite"
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
      >
        <TarjetaEmpresa empresa={encontrada} idTitulo="titulo-empresa-encontrada" />
        <div className="flex flex-col-reverse items-stretch gap-4 sm:flex-row sm:items-center sm:justify-between">
          <Boton variante="secundario" onClick={() => setEncontrada(undefined)}>
            No es mi empresa
          </Boton>
          <Boton onClick={() => continuar(encontrada)}>Continuar a iniciar sesión</Boton>
        </div>
      </section>
    )
  }

  return (
    <>
      <form
        noValidate
        className="grid gap-4 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6"
        onSubmit={enviar(async ({ rtn }) => setEncontrada(await resolverEmpresaPorRtn(rtn)))}
      >
        {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
        <CampoTexto
          etiqueta="RTN de la empresa"
          required
          inputMode="numeric"
          autoComplete="organization"
          registro={register('rtn')}
          largoExacto={14}
          error={errors.rtn}
          ayuda="14 dígitos. Si trabajas en varias empresas, usa el RTN de la empresa a la que quieres entrar."
        />
        <Boton type="submit" className="w-full sm:w-auto sm:justify-self-start" cargando={isSubmitting} textoCargando="Buscando…">
          Buscar empresa
        </Boton>
      </form>

      {ultima && (
        <section aria-label="Última empresa" className="grid gap-3 rounded-lg border border-border-default bg-bg-surface p-4 sm:p-6">
          <p className="text-xs text-text-secondary">Entraste aquí la última vez</p>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <TarjetaEmpresa empresa={ultima} />
            <Boton variante="secundario" onClick={() => continuar(ultima)}>
              Continuar con esta empresa
            </Boton>
          </div>
        </section>
      )}

      <div className="grid gap-2 text-text-secondary">
        <p>
          ¿Tu empresa aún no está en Norvia?{' '}
          <Link to="/registro" state={estadoVolver(rutaActual(location), 'Volver a iniciar sesión')}>
            Regístrala
          </Link>
        </p>
      </div>
    </>
  )
}

/** Nombre y RTN de la empresa. Más adelante, aquí va su logo. */
export function TarjetaEmpresa({ empresa, idTitulo }: { empresa: EmpresaPorRtn; idTitulo?: string }) {
  return (
    <div className="flex min-w-0 items-start gap-3">
      <Building2 aria-hidden size={24} strokeWidth={1.5} className="mt-1 shrink-0 text-text-tertiary" />
      <div className="grid min-w-0 gap-1">
        <p id={idTitulo} className="text-md font-semibold">
          {empresa.razonSocial}
        </p>
        <p className="text-text-secondary tabular-nums">RTN {empresa.rtn}</p>
      </div>
    </div>
  )
}
