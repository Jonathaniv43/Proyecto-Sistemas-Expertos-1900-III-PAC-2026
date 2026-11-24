import type { CategoriaRiesgo, Rubro } from './types/catalogos'

/** "Construcción · Trabajo en alturas", para elegir la categoría en una sola lista, por rubro y nombre. */
export const opcionesCategoria = (categorias: CategoriaRiesgo[], rubros: Rubro[]) => {
  const rubro = (id: number) => rubros.find((r) => r.id === id)?.nombre ?? ''
  return [...categorias]
    .sort((a, b) => rubro(a.rubroId).localeCompare(rubro(b.rubroId)) || a.nombre.localeCompare(b.nombre))
    .map((c) => ({ valor: c.id, texto: `${rubro(c.rubroId)} · ${c.nombre}${c.activa ? '' : ' (inactiva)'}` }))
}
