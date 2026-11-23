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
├── app/            router, providers y rutas/ (RutaPrivada, RequiereRol, SoloSinSesion)
├── shared/
│   ├── api/        cliente Axios, sesión (tokens) y extractor de errores
│   ├── components/ componentes comunes (layout, páginas genéricas) y ui/ (Boton, CampoTexto, CampoSelect,
│   │               GrupoRadio, Aviso, RequisitosContrasena; clases.ts con las clases compartidas)
│   ├── forms/      useFormulario (React Hook Form + Zod)
│   ├── hooks/      usePeticion, useCargaVisible
│   ├── sesion/     estado de la sesión para React: useSesion, SiRol, ROLES, cerrarSesion
│   ├── validacion/ configuración de Zod y reglas reutilizables (espejo de FluentValidation)
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
- Si el backend rechaza el refresh, borra la sesión y avisa a quien se haya suscrito con `sesion.alTerminar(...)`. El router lo usa para volver al login.
- Si no hay conexión, conserva la sesión: la petición falla con su error y la pantalla ofrece reintentar.
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

## Formularios (`@/shared/forms` y `@/shared/validacion`)

Cada formulario sigue cuatro reglas:
- **Un esquema Zod por formulario,** en `features/<modulo>/schemas/`. Es espejo del validador de FluentValidation del caso de uso: mismos campos, obligatorios y límites.
- **Reglas comunes de `@/shared/validacion`:** `textoRequerido(max)`, `textoOpcional(max)`, `correo()`, `contrasenaIngresada()` (login) y `contrasenaNueva()` (política de HU-114, la misma de `PoliticaContrasena.cs`). Si cambia una regla en el backend, se cambia aquí.
- **`useFormulario(esquema, { defaultValues })`** ya trae `zodResolver` y `mode: 'onChange'`: el error aparece mientras se escribe, antes de enviar. Hay que dar siempre `defaultValues` (`''` en los textos).
- **`enviar(accion)`** valida, llama a la API y, si falla, reparte el error:
  - los errores por campo del backend van a su campo, y el primero recibe el foco;
  - si el error no tiene campo (409, sin conexión, etc.), o el campo no existe en el formulario, el mensaje queda en `errorGeneral`;
  - mientras envía, `formState.isSubmitting` es `true`.

```tsx
// features/vinculos/schemas/crearEncuestado.ts
export const crearEncuestadoEsquema = z.object({
  correo: correo(),
  primerNombre: textoRequerido(50),
})

// features/vinculos/components/FormularioEncuestado.tsx
const { register, formState: { errors, isSubmitting }, enviar, errorGeneral } =
  useFormulario(crearEncuestadoEsquema, { defaultValues: { correo: '', primerNombre: '' } })

<form noValidate onSubmit={enviar((datos) => api.post('/vinculos/encuestados', datos))}>
  {errorGeneral && <Aviso tipo="error">{errorGeneral}</Aviso>}
  <CampoTexto etiqueta="Correo" required type="email" registro={register('correo')} error={errors.correo} />
  <Boton type="submit" cargando={isSubmitting} textoCargando="Guardando…">Guardar</Boton>
</form>
```

**Errores de negocio en su campo:** `erroresPorCodigo` lleva un `codigo` del backend a un campo, con un mensaje propio opcional. Por ejemplo, el RTN repetido se muestra junto al RTN sin borrar lo demás:

```ts
useFormulario(esquema, {
  defaultValues,
  erroresPorCodigo: { 'empresas.rtn_duplicado': { campo: 'rtn' } },
})
```

**Controles de selección:**
- Con 5 opciones o menos se usa `GrupoRadio`; con más, `CampoSelect` (design system § Select).
- Un enlace que se ve como botón usa `clasesBoton('primario')` de `ui/clases.ts`.

**Mensajes:**
- Dicen qué corregir ("Ingresa un correo válido, por ejemplo nombre@empresa.com.") y usan el tono del design system (tú).
- Los casos genéricos (obligatorio, mínimo, máximo, formato) ya están traducidos en `shared/validacion/zod.ts`. Solo se escribe un mensaje propio cuando hace falta ser más específico.
- **`noValidate` en el `<form>`:** desactiva la validación nativa del navegador, para que solo valide Zod.

## Rutas protegidas (`app/rutas` y `@/shared/sesion`)

| Envoltorio | Para qué |
|---|---|
| `RutaPrivada` | Exige sesión. Sin sesión, o si vence, lleva a `/login` y recuerda a dónde volver. Al recargar, recupera la sesión con el refresh guardado. Si el backend no responde, muestra "No se pudo conectar" con "Reintentar" y no cierra la sesión. |
| `RequiereRol roles={[...]}` | Va dentro de `RutaPrivada`. Sin el rol se ve igual que una ruta inexistente, para no confirmar que existe. |
| `SoloSinSesion` | Para `/login`. Con sesión activa, vuelve a la ruta pedida o a `/inicio`. Solo acepta rutas internas. |

```tsx
{ element: <RutaPrivada />, children: [
  { path: 'inicio', element: <InicioPage /> },
  { element: <RequiereRol roles={ROLES_GESTION} />, children: [
    { path: 'personas', element: <PersonasPage /> },
  ] },
] }
```

**Dentro de una pantalla:**
- `useSesion()` devuelve `{ estado: 'activa', usuario: { tipo, rol, vinculoId, debeCambiarPassword } }`, o `verificando`, `sin_conexion` o `sin_sesion`.
- `<SiRol roles={ROLES_GESTION}>` oculta botones y menús a los demás roles.
- `cerrarSesion()` avisa al backend y vuelve al login. Si hay varias pestañas abiertas, todas cierran sesión.

**El rol sale del JWT y solo decide qué se muestra.** Quien autoriza de verdad es el backend, en cada petición: si responde 403, se muestra el error.
