# Norvia — frontend web

React + TypeScript + Vite, organizado por funcionalidades. Las reglas completas están en `16-arquitectura-backend-frontend.md` §9, y los colores, el espaciado, los radios y las sombras salen de `design-system.md`. Ambos archivos están en el repositorio de documentación.

## Comandos

Se ejecutan desde `frontend/` y requieren Node 22 o superior y pnpm 10 (`corepack enable` lo activa con la versión de `package.json`).

```bash
pnpm install
pnpm dev       # servidor de desarrollo (http://localhost:5173)
pnpm build     # chequeo de tipos + build de producción en dist/
pnpm lint      # ESLint
pnpm preview   # sirve el build
```

### API local

En desarrollo, las peticiones a `/api` pasan por el proxy de Vite hacia `http://localhost:5167`, el puerto de `Norvia.API`. Así el navegador no necesita CORS. Para usar otro destino, copia `.env.example` como `.env.local` y define `VITE_API_PROXY`. En producción se define `VITE_API_URL`.

## Estructura

```text
src/
├── app/            router, providers y (con HU-102) rutas protegidas
├── shared/
│   ├── api/        cliente Axios, sesión (tokens) y extractor de errores
│   ├── components/ componentes comunes (layout, páginas genéricas, UI base)
│   ├── hooks/      usePeticion, useCargaVisible
│   └── styles/     index.css (entrada) y theme.css (tokens del design system)
└── features/
    └── <modulo>/   landing, auth, empresas, vinculos, encuestas…
        ├── api/         llamadas HTTP de la feature
        ├── components/
        ├── hooks/
        ├── pages/
        ├── schemas/     esquemas Zod (espejo de los validadores del backend)
        └── types/
```

Las subcarpetas de cada feature se crean cuando hacen falta, no vacías.

### Reglas de imports (las revisa `pnpm lint`)

- **Una feature no importa otra feature ni `app/`.** Lo que comparten va en `shared/`.
- **`shared/` no importa features ni `app/`.**
- **Solo `app/` importa páginas de las features,** para armar el router.
- **Dentro de una feature se usan rutas relativas** (`../components/X`). Para salir de la feature se usa el alias `@/` (`@/shared/...`).

## Estilos (Tailwind v4)

`src/shared/styles/theme.css` declara en `@theme` las variables de `design-system.md` con los mismos nombres y valores. Las escalas por defecto de Tailwind están vaciadas.

| Qué | Clases disponibles | Ejemplo |
|---|---|---|
| Color (usar las semánticas) | `bg-bg-*`, `text-text-*`, `border-border-*`, `bg-action-*`, `*-info-*`, `*-success-*`, `*-warning-*`, `*-error-*` | `bg-bg-surface text-text-secondary` |
| Espaciado | solo los pasos 1, 2, 3, 4, 6, 8, 12 y 16; `h-control`, `h-control-touch`, `w-sidebar` | `p-6 gap-4` |
| Tipografía | `text-xs`, `text-sm`, `text-md`, `text-lg`, `text-xl` y `text-2xl` (con interlineado); `font-regular`, `font-medium` y `font-semibold` | `text-sm font-medium` |
| Radios | `rounded-sm`, `rounded-md`, `rounded-lg`, `rounded-xl` y `rounded-full` | `rounded-md` |
| Sombras | `shadow-1`, `shadow-2` y `shadow-3` | `shadow-2` |
| Ancho máximo | `max-w-layout` (1280px) y `max-w-layout-narrow` (720px) | |

- **Clases que no existen:** `bg-blue-500`, `p-5`, `rounded-2xl` y `shadow-lg` no generan CSS.
- **Valores arbitrarios:** Tailwind no puede bloquear los que van entre corchetes (`p-[13px]`). No se usan, y se revisa en code review.
- **Primitivas:** los componentes no usan `brand-*` ni `neutral-*` directamente.
- **Modo oscuro:** `data-theme="dark"` en `<html>` cambia las variables semánticas. No se usa `dark:`.
- **Marca de la empresa:** se personaliza sobrescribiendo `--color-brand-*` en `:root`. Los avisos y el riesgo no cambian.

## Peticiones HTTP (`@/shared/api`)

Las features llaman a la API solo con `api`, el cliente Axios de la app. Nunca usan `axios` ni `fetch` directamente.

```ts
// features/empresas/api/perfil.ts
import { api } from '@/shared/api'

export const obtenerPerfil = (signal: AbortSignal) =>
  api.get<PerfilEmpresa>('/empresas/perfil', { signal }).then((r) => r.data)
```

**Sesión:**
- El access token vive solo en memoria y el refresh token en `localStorage`.
- El login (HU-018) llama a `sesion.guardar(respuesta)`, y el cierre de sesión a `sesion.terminar('cerrada')`.
- Ante un 401, el cliente renueva la sesión una sola vez, aunque fallen varias peticiones a la vez, y repite la petición.
- Si no puede renovarla, borra la sesión y avisa a quien se haya suscrito con `sesion.alTerminar(...)`. El router lo usa para volver al login (HU-102).
- Un 401 del login no intenta renovar: significa credenciales inválidas.

**Errores:** `extraerError(error)` devuelve `{ mensaje, campos, status, codigo, cancelada }`.
- `mensaje` está listo para mostrar. Es el `title` del backend, o uno fijo si no hay conexión, se agotó el tiempo, hay un 403, 404 o 500.
- En 403 y 404 se usa el mismo mensaje fijo, para no confirmar si algo existe en otra empresa.
- `campos` trae los errores de FluentValidation con la ruta del formulario: `Personas[0].Correo` → `personas.0.correo`.

**Estado de carga:**
- `usePeticion(fn)` da `{ ejecutar, cargando, datos, error }` y cancela la petición anterior si la pantalla se cierra.
- Para cargar contenido se usa `useCargaVisible(cargando)`: el skeleton aparece solo si tarda más de ~300 ms.
- Para los botones se usa `cargando` directamente, con el spinner y el texto en gerundio.

```tsx
const { ejecutar, cargando, error } = usePeticion((signal, datos: Formulario) =>
  api.post('/vinculos', datos, { signal }))
```
