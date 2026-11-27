/**
 * Marcadores de las plantillas de documento (D13; 12-modelo-datos § PlantillaDocumento). Se reemplazan con los datos
 * de la empresa al generar el documento (HU-070); un marcador fuera de esta lista queda tal cual.
 */
export const MARCADORES_DOCUMENTO = [
  { marcador: '{{RazonSocial}}', dato: 'Razón social' },
  { marcador: '{{RTN}}', dato: 'RTN' },
  { marcador: '{{Direccion}}', dato: 'Dirección' },
  { marcador: '{{ActividadEconomica}}', dato: 'Actividad económica' },
  { marcador: '{{Rubro}}', dato: 'Rubro' },
  { marcador: '{{Fecha}}', dato: 'Fecha en que se genera' },
] as const
