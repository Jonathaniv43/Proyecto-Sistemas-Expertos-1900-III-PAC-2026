import { ROLES_EMPRESA } from '@/shared/sesion'
import { textoOpcional, z } from '@/shared/validacion'
import type { EstadoVinculo } from '../types/vinculos'

/** Estados que quitan el acceso: piden motivo y confirmación. */
export const quitaAcceso = (estado: EstadoVinculo) => estado === 'suspendido' || estado === 'baja'

/**
 * Espejo de ActualizarVinculoValidator (backend) para el modal "Editar acceso" (HU-021):
 * - al pasar a suspendido o de baja, el motivo es obligatorio;
 * - con motivo "otro", hay que describirlo (máximo 1000 caracteres);
 * - si no cambia nada, no hay nada que guardar.
 * `original` es el acceso tal como está: el motivo solo se pide si el estado cambia.
 */
export const editarAccesoEsquema = (original: { rolCodigo: string; estado: EstadoVinculo }) =>
  z
    .object({
      rolCodigo: z.enum(ROLES_EMPRESA, 'Elige el rol que tendrá en la empresa.'),
      estado: z.enum(['activo', 'suspendido', 'baja'], 'Elige el estado del acceso.'),
      // Un grupo de radios sin elegir vale null en React Hook Form.
      motivoCodigo: z.enum(['decision_admin', 'baja_laboral', 'otro']).nullish(),
      motivoDetalle: textoOpcional(1000),
    })
    .superRefine((datos, ctx) => {
      const cambiaEstado = datos.estado !== original.estado
      if (cambiaEstado && quitaAcceso(datos.estado) && !datos.motivoCodigo)
        ctx.addIssue({ code: 'custom', path: ['motivoCodigo'], message: 'Elige el motivo.' })
      if (cambiaEstado && quitaAcceso(datos.estado) && datos.motivoCodigo === 'otro' && !datos.motivoDetalle)
        ctx.addIssue({ code: 'custom', path: ['motivoDetalle'], message: 'Describe el motivo.' })
    })
