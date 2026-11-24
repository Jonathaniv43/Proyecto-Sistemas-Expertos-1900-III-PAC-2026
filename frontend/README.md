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
│   │               GrupoRadio, GrupoCasillas, AreaTexto, Aviso, Insignia, BotonIcono,
│   │               DialogoConfirmacion, RequisitosContrasena; clases.ts con las clases compartidas)
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
| Datos (gráficos) | `bg-dato-serie` (la barra) y `bg-dato-pista` (su fondo) | `bg-dato-serie` |
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
| `SoloSinSesion` | Para `/login`, `/plataforma`, `/registro` y `/verificar-cuenta`. Con sesión activa, vuelve a la ruta pedida o a `/inicio`. Solo acepta rutas internas. |

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

## Inicio de sesión (HU-018)

- **`/login` (empresas):**
  - Pide RTN, correo y contraseña. El RTN se convierte en el identificador de empresa con `GET /empresas/por-rtn`.
  - Cualquier dato incorrecto da el mismo mensaje, sin decir cuál falló. También pasa con una cuenta desactivada, un vínculo suspendido o una empresa suspendida.
  - Recuerda en este navegador el RTN del último ingreso.
- **`/plataforma` (equipo de Norvia):** pide correo y contraseña. Está separado del login de empresa.
- **Contraseña temporal:** con `debeCambiarPassword`, `RutaPrivada` solo deja usar `/cambiar-contrasena`. Al cambiarla, el backend cierra todas las sesiones y se vuelve al login con un aviso.
- **Una empresa por navegador:**
  - Todas las pestañas comparten la sesión, así que no se mezclan datos de dos empresas.
  - Mientras hay una sesión abierta, `/login` lleva a ella. Para entrar a otra empresa hay que cerrar sesión, o usar otro navegador o una ventana privada.
  - Si dos pestañas inician sesión en empresas distintas, la primera se cierra y lo explica.
- **Varias empresas (HU-019):** misma cuenta (correo y contraseña) en cada empresa, con el rol de cada una. Cerrar sesión en una no cierra la de otro navegador o dispositivo.
  - Si la sesión se corta sola estando en una página (por ejemplo, `/encuestas/:id`) y después se entra a **otra** empresa, se empieza en el inicio y no en esa página, que es de la empresa anterior. Si se entra a la misma empresa, se vuelve a la página.
  - Cómo: `RutaPrivada` guarda en el estado del login el RTN de la sesión que terminó (`sesion.empresaTerminada()`), y `destinoSeguro` lo compara con el de la sesión nueva.
- **Encabezado del área privada:** muestra la empresa, el rol y el botón para cerrar sesión. El menú de cada módulo se agrega con su HU.

## Agregar personas (HU-020)

- **`/personas/nueva`:** solo para `admin_empresa` (`RequiereRol`). Se llega desde el inicio.
- **Roles:** se elige uno de los 4 de empresa, cada uno con la descripción de lo que puede hacer (`DESCRIPCIONES_ROL_EMPRESA`).
- **Cuenta existente:** si el correo ya tiene cuenta en Norvia, por ejemplo en otra empresa, se reutiliza. La persona entra con su contraseña de siempre y conserva sus datos.
- **Cuenta nueva:**
  - El front genera la contraseña temporal con `generarContrasenaTemporal()`, que cumple la política de HU-114 y no usa caracteres confusos.
  - La confirmación la muestra una sola vez, con un botón para copiarla. El administrador se la entrega a la persona, que debe cambiarla en el primer ingreso.
- **Acceso duplicado:** se avisa junto al correo, sin borrar lo demás.

## Agregar encuestados (HU-115)

- **`/personas/encuestados`:** solo para `admin_empresa`. Permite agregar de a una persona o varias a la vez.
- **Una persona:** el backend genera la contraseña temporal si la cuenta es nueva, y la confirmación la muestra una vez.
- **Varias a la vez:**
  - **Entrada:** se pega una lista desde Excel (tabuladores) o desde un CSV (`;` o `,`), o se carga un archivo.
  - **Columnas:** 3 (correo, primer nombre, primer apellido) o 5 (correo, primer nombre, segundo nombre, primer apellido, segundo apellido). Se omiten un encabezado y las líneas vacías.
  - **Vista previa en vivo** (`lote/leerLista.ts`): valida cada fila con las reglas de `FilaEncuestadoValidator` y marca los correos repetidos dentro de la lista. Las filas con error no se envían.
  - **Límite:** hasta 500 por envío.
- **Resultado:**
  - quiénes recibieron acceso, con la contraseña temporal de las cuentas nuevas o "ya tenía cuenta";
  - "Descargar contraseñas (CSV)", con BOM y `;` para que Excel lo abra bien;
  - las filas por corregir, con el número de línea original. "Corregir las filas con error" las devuelve al cuadro de texto.
- **Tablas:** se usan las clases `.tabla-contenedor` y `.tabla` de `index.css` (design system § Tabla). En pantallas chicas la tabla se desplaza dentro de su caja.

## Nueva encuesta: elegir categorías (HU-029)

- **`/encuestas/nueva`:** para `admin_empresa` y `responsable_sst` (`ROLES_GESTION`). Se llega desde el inicio.
- **Categorías:** salen de `GET /encuestas/categorias-disponibles`, que trae solo las del rubro de la empresa, cada una con sus preguntas.
- **Vista previa:** al marcar categorías se ven, en vivo, las preguntas que tendrá la encuesta, numeradas y agrupadas por categoría.
- **Crear:** con el título, "Crear borrador" llama a `POST /encuestas`. La encuesta queda como borrador y HU-030 sigue con la edición y la publicación.
- **Estados de pantalla:** cargando (skeleton), error al cargar ("Reintentar"), rubro sin categorías y empresa sin rubro.
- **`PreguntaEncuesta`** (`features/encuestas/components`):
  - Sigue el bloque "Pregunta de encuesta" del design system: enunciado numerado, `*` si es obligatoria y la indicación como ayuda.
  - El control depende del tipo: `respuesta_breve` es área de texto, `seleccion_unica` son radios y `seleccion_multiple` son casillas.
  - Hoy se usa en modo de solo lectura. HU-030 (vista del encuestado) y HU-031 (responder) la reutilizan.
- **Catálogo:** no hay categorías ni preguntas sembradas. Las carga el staff con `POST /categorias-riesgo` y `POST /preguntas` (HU-023, todavía sin pantalla).

## Crear y editar encuestas (HU-030)

- **Rutas:** `/encuestas` (lista) y `/encuestas/:id` (editor), para `ROLES_GESTION`. El inicio lleva a la lista, y la confirmación de HU-029 lleva al borrador.
- **Qué se edita:** el borrador, o la versión publicada si no hay borrador.
  - **Campos:** título, instrucciones y preguntas agrupadas por sección (una por categoría). Cada pregunta tiene texto, tipo, obligatoria y opciones (agregar o quitar), y se puede subir o bajar dentro de su sección, o eliminar.
  - **Peligro asociado:** se conserva, y la casilla "Indica exposición al peligro" aparece solo en esas preguntas (HU-033 la usa).
  - **Validación:** el esquema `schemas/borrador.ts` sigue las reglas de `ActualizarEncuestaBorradorValidator`.
- **Versiones:**
  - Editar una encuesta publicada sin borrador crea la versión siguiente al guardar; los encuestados siguen viendo la vigente hasta que se publique la nueva.
  - En ese caso las secciones se envían con ids nuevos (`borrador/convertir.ts`), porque el backend responde 500 si recibe los de la versión publicada.
- **Publicar y cerrar:**
  - "Publicar" guarda primero lo pendiente y pide confirmación.
  - "Cerrar encuesta…" usa el diálogo de peligro, con el foco inicial en "Cancelar". Una encuesta cerrada queda de solo lectura.
- **Quién puede editar:** solo quien creó la encuesta (`creadaPorId` igual a `vinculoId`). Los demás la ven como encuestado, con una explicación.
- **Vista del encuestado** (`VistaEncuestado`): muestra los cambios aún sin guardar y reutiliza `PreguntaEncuesta`.
- **Cambios sin guardar:** el navegador pregunta antes de salir de la página.
- **Errores inesperados:** el router tiene un `errorElement` (`ErrorInesperado`) para que nunca se vea la pantalla técnica.

## Responder encuestas (HU-031)

- **Rutas:** `/mis-encuestas` (pendientes) y `/mis-encuestas/:id` (responder), para los 4 roles de empresa. El inicio tiene "Mis encuestas".
- **Pendientes:** salen de `GET /encuestas/pendientes`, que trae las encuestas publicadas que la persona todavía no respondió, con su versión vigente y sus preguntas.
- **Formulario** (`schemas/respuesta.ts`):
  - Su esquema Zod se arma según el tipo de cada pregunta, como pide 16 §9, y sigue las reglas de `ResponderEncuestaServicio`: obligatorias, máximo de caracteres y una o varias opciones.
  - Se pide al menos una respuesta. Las preguntas opcionales sin responder no se envían.
  - El error de "al menos una respuesta" va en la ruta `sinRespuestas`, aparte de `respuestas`, porque si no tapa los errores de cada pregunta.
- **`PreguntaEncuesta` en modo responder:** los controles están habilitados, el error va una vez debajo del grupo y las respuestas breves llevan el contador "N / máximo" del design system.
- **Situaciones que se explican:** ya respondida, encuesta cerrada, o cambio de versión mientras se respondía ("Abrir la versión actual"). Al terminar se confirma que la respuesta es anónima.

## Resultados de una encuesta (HU-032)

- **`/encuestas/:id/resultados`:** para `ROLES_GESTION`. Se llega con "Ver resultados" desde la encuesta, cuando ya tiene una versión publicada.
- **Indicadores:** participantes (todas las versiones), respuestas (de la versión vigente) y periodo analizado (primera y última respuesta).
- **Detalle oculto:** con menos de 3 respuestas el backend no manda el detalle (RN-73), y la pantalla explica por qué y cuántas van.
- **Gráfico por pregunta de selección** (`DistribucionPregunta`): una lista de medidores, siguiendo la guía de visualización.
  - Cada opción muestra su nombre, una barra fina y la cifra "N · %", en el orden de la encuesta.
  - Un solo tono y sin leyenda, porque es una sola serie. Todas las cifras están impresas: nada depende de un tooltip.
  - Cada fila es enfocable con el teclado y se anuncia con su cifra.
  - En la selección múltiple se aclara que los porcentajes pueden sumar más de 100 %.
- **Respuestas breves:** sin autor y en orden alfabético, no en el orden en que se guardaron, para que el orden no permita deducir quién escribió cada una.
- **Colores de datos:** `--color-dato-serie` y `--color-dato-pista` (`theme.css`) vienen de la paleta de referencia de visualización (azul), no de la marca. Están validados contra `bg-surface` en claro y en oscuro.

## Peligros sugeridos (HU-033)

- **Dónde:** sección "Peligros sugeridos" de `/encuestas/:id/resultados`, entre los indicadores y el detalle por pregunta (`PeligrosSugeridos`).
- **De dónde salen:** un peligro se sugiere cuando al menos una respuesta eligió una opción que indica exposición en una pregunta asociada a ese peligro (RN-13). Se calculan con las respuestas de **todas** las versiones, así que el total puede no coincidir con el indicador "Respuestas" (que es de la versión vigente).
- **Cada sugerencia** muestra el peligro, cuántas respuestas lo respaldan (con barra y porcentaje sobre el total), las preguntas que lo originaron con su cifra y el estado del riesgo registrado: "Por revisar", "En la matriz" o "Descartado" (se confirma o descarta en HU-035). El orden es el del backend: por respaldo y después por nombre.
- **Estados:** menos de 3 respuestas (no se sugiere nada, RN-73), sin sugerencias, error y carga. Tiene su propia petición: si falla, el resto de los resultados se sigue viendo y se puede reintentar.
- **Ojo:** el `GET` registra las sugerencias nuevas como riesgos sugeridos (sin repetirlas), así que abrir la página no es solo lectura en el backend.

## Personas y acceso (HU-021)

- **`/personas`:** solo para `admin_empresa`. Se llega con "Ver las personas" desde el inicio, y "Cancelar" de los formularios de alta vuelve aquí.
- **Lista:** nombre, correo, rol, estado y fecha de alta. Los filtros (búsqueda sin tildes ni mayúsculas, rol y estado) se aplican en el navegador sobre la lista completa.
- **Editar acceso** (`EditarAccesoModal`, sobre el `Modal` de `shared/components/ui`): cambia el rol y el estado. Se envía solo lo que cambia (`PATCH /vinculos/{id}`).
  - **Cambiar el rol o reactivar:** se guarda directo.
  - **Suspender o dar de baja:** pide el motivo (y el detalle si es "Otro motivo") y después confirma con el diálogo de peligro, porque quita el acceso. El backend cierra las sesiones de esa persona en esta empresa; su acceso a otras empresas no cambia.
  - **Motivos:** se ofrecen "Decisión de la administración", "Dejó de trabajar en la empresa" y "Otro motivo". "Falta de pago de la empresa" no se ofrece: es el motivo de la plataforma al suspender una empresa.
  - **Errores:** si el backend rechaza el cambio (por ejemplo, porque la empresa se quedaría sin administrador activo), el mensaje se ve en el modal y la fila no cambia.
- **La propia fila no se edita:** muestra "Tú" y la cambia otra persona administradora. Así nadie se quita el rol o el acceso por error, y la empresa no se queda sin administrador.
- **Celular:** rol y estado bajan debajo del nombre, para que "Editar acceso" quede a la vista sin desplazar la tabla.
- **`Modal`** (design-system.md § Modal): `<dialog>` nativo de 560px con encabezado y pie fijos; Esc cierra; con cambios sin guardar, el clic fuera no cierra; en celular ocupa toda la pantalla.
- **CSS:** `.tabla-contenedor` y `.modal-cuerpo` llevan `position: relative`. Sin eso, lo que está posicionado de forma absoluta adentro (un encabezado `sr-only`, los radios ocultos) se escapa del scroll: ensancha la página o desplaza el modal entero.

## Perfil de la empresa (HU-026)

- **`/empresa`:** para `ROLES_GESTION`. Se llega con "Perfil de la empresa" desde el inicio. La administración lo edita y el Responsable SST lo ve en solo lectura (el backend tampoco le deja guardar).
- **Campos:** razón social, rubro (radios con hasta 5 opciones; si hay más, select), trabajadores permanentes, actividad económica y dirección.
  - **RTN:** se muestra pero no se edita, porque con él se inicia sesión. Su unicidad se valida al registrar la empresa (HU-017).
  - **Obligatorios en el front:** razón social, rubro y trabajadores, porque son los que completan el perfil (`perfilCompleto`) y el backend no deja borrarlos una vez guardados.
  - **Dirección y actividad vacías:** se envían como `""`, y el backend las borra.
- **Rubro marcado:** el perfil trae `rubroNombre`, no el código. Se marca la opción por nombre, que es único en el catálogo (`GET /empresas/rubros-registro`).
- **Guardar:** `PATCH /empresas/perfil` con solo lo que cambió; se confirma con un aviso de éxito y el formulario queda con lo guardado. Si cambió la razón social, el encabezado se actualiza (`sesion.actualizarRazonSocial`).
- **Perfil incompleto:** el inicio de la administración muestra un aviso con el enlace al perfil, y la página también lo indica.
- **Empresa sin rubro:** la nueva encuesta (HU-029) lleva a la administración al perfil para elegirlo.
- **Fuera del formulario:** la fecha de inicio de operaciones, que el backend acepta, no está en los criterios de la HU y necesita el campo de fecha del design system, que todavía no existe.
- **Avisos de resultado** (aquí y en Personas): la región `role="status"` va con `display: contents`. Así no suma un hueco en la grilla cuando está vacía y sigue anunciándose al lector de pantalla.

## Cambiar la contraseña (HU-022)

- **Por qué un enlace:** antes de cambiar la contraseña se confirma que la persona puede entrar a su correo. Se le envía un enlace, y el cambio se hace desde ahí.
- **Pedir el enlace** (`SolicitarCambioContrasenaPage`, `POST /auth/contrasena/solicitud`):
  - **Con sesión** (`/cuenta/contrasena`, "Cambiar contraseña" en el encabezado): va al correo de la cuenta, que viene en el claim `email` del JWT (`usuario.email`).
  - **Sin sesión** (`/recuperar-contrasena`, "¿Olvidaste tu contraseña?" en los dos logins): se escribe el correo. La respuesta es la misma exista o no la cuenta.
- **El enlace** (`/restablecer-contrasena?token=…`, `RestablecerContrasenaPage`): funciona con o sin sesión. Pide la contraseña nueva con los requisitos de HU-114 a la vista.
  - **Errores:** "igual a la actual" se marca en el campo. Un enlace vencido, usado o incompleto (410 `auth.enlace_invalido`, o sin token) muestra "El enlace ya no sirve" y ofrece pedir otro.
  - **Al guardar:** la contraseña vale en todas las empresas y el backend cierra todas las sesiones de la cuenta. Si este navegador tenía una sesión de esa cuenta, se cierra (se comprueba renovando: si el refresh fue rechazado, termina con `contrasena_cambiada`).
- **Motivo del cierre entre pestañas:** al terminar una sesión se guarda el motivo (`norvia.motivoFin`) antes de borrar el refresh. Así las otras pestañas también explican por qué se cerró: contraseña cambiada, sesión vencida, etc.
- **Primer ingreso con contraseña temporal** (`/cambiar-contrasena`): no cambia. Se cambia con la contraseña actual, sin correo.

## Revisar los peligros sugeridos (HU-035)

- **Dónde:** en la misma sección "Peligros sugeridos" de los resultados (HU-033). Arriba se indica cuántas sugerencias quedan por revisar.
- **Confirmar…** (`ConfirmarPeligroModal`): pide la actividad relacionada, opcional y de hasta 200 caracteres. El peligro queda "En la matriz" y se muestra su actividad.
- **Descartar…:** usa el diálogo de confirmación, porque la decisión no se cambia después. Queda "Descartado".
- **Una sola vez:** `PATCH /encuestas/{id}/peligros-sugeridos/{riesgoId}` con `estado` y `actividad`. Si otra persona ya la revisó (409 `encuestas.sugerencia_ya_revisada`), se avisa y la lista se recarga con su decisión.
- **Al recargar:** las decisiones vienen en la misma lista de HU-033 (`estado` y `actividad`).

## Matriz de riesgos: agregar riesgos (HU-036)

- **`/riesgos`** (`ROLES_GESTION`, "Matriz de riesgos" en el inicio): lista los riesgos confirmados con el peligro, la actividad, la etiqueta de riesgo, el origen y la fecha.
  - **Origen:** "Encuesta …", con enlace a sus resultados, o "Agregado a mano".
  - **Nivel:** por ahora todos son "Sin evaluar"; la evaluación llega con HU-037.
  - **Celular:** la actividad y el origen bajan debajo del peligro.
- **`/riesgos/nuevo`:** el peligro se elige del catálogo o se describe (hasta 300 caracteres); la actividad es obligatoria (hasta 200).
  - **Al guardar:** vuelve a la matriz con un aviso.
  - **Duplicado:** si ya está en la matriz con la misma actividad (409 `riesgos.duplicado`), el error va junto a la actividad.
- **`SelectBusqueda`** (`shared/components/ui`): el select con búsqueda de design-system.md § Select, para listas largas (peligros y, después, responsables).
  - **Cerrado:** igual que el input.
  - **Abierto:**
    - el foco va al buscador, que filtra sin tildes ni mayúsculas;
    - las flechas recorren las opciones, Enter elige y Esc cierra;
    - en los dos últimos casos, el foco vuelve al campo.
  - **Celular:** hoja inferior con el fondo oscurecido.
- **`EtiquetaRiesgo`:** el chip neutro de design-system.md § Etiqueta de riesgo. Los colores de nivel (`--color-riesgo-*` en `theme.css`) se usan desde HU-037.
- Los peligros sugeridos (HU-035) enlazan a la matriz.

## Evaluar un riesgo (HU-037)

- **`/riesgos/:id`:** se llega desde el nombre del riesgo en la matriz. Muestra el peligro, la actividad, el origen y la evaluación vigente: etiqueta "Medio · 6", los dos valores con su nombre y "Evaluado por … el …".
- **Formulario:**
  - **Probabilidad y severidad:** dos listas de radios con la explicación de cada valor (`niveles.ts`, RN-80). El esquema Zod pide las dos, del 1 al 5.
  - **`MapaCalor`:** un atajo sincronizado. Cada celda es un botón (`aria-pressed`) que elige las dos a la vez y se usa con teclado.
  - **Nivel calculado:** se ve al instante (`clasificar`, igual que RN-16). Para alto y crítico avisa que necesitan al menos un control. El valor guardado lo calcula el backend.
- **Volver a evaluar:** parte de la evaluación vigente; la anterior se conserva (RN-20). Un riesgo sugerido o descartado no muestra el formulario.
- **Matriz:** la columna Nivel muestra la etiqueta con el cuadrado de color (`--color-riesgo-*`, clases en `CLASES_NIVEL`).
- **CSS:** `.cuadro-riesgo` y `.mapa-calor*` en `index.css`. La escala de espaciado no tiene 10 ni 44px, y en celular el título "Probabilidad" pasa arriba para que las celdas conserven sus 44px.

## Ver la matriz (HU-038)

- **`/riesgos`:**
  - **Mapa (`MapaMatriz`, de `GET /riesgos/matriz`):** muestra cuántos riesgos hay en cada celda de probabilidad × severidad.
    - Una celda con riesgos es un botón que filtra la lista; tocarla otra vez o la X del filtro lo quita.
    - Al lado van el total, los sin evaluar (enlace que filtra) y la leyenda.
  - **Lista:** se ordena por nivel, el más alto primero, y cada riesgo abre su detalle.
- **Filtros:** buscar, clasificación del peligro, nivel (incluye "Sin evaluar") y estado ("En la matriz", "Por revisar", "Descartados"). Se envían al backend como parámetros de `GET /riesgos`.
  - **Buscar:** espera 300ms después de dejar de escribir; los selects filtran al momento.
  - **Estado distinto de "En la matriz":** el nivel se desactiva, porque solo los riesgos de la matriz tienen nivel. Elegir una celda del mapa vuelve a "En la matriz".
  - **Mientras carga:** `usePeticion` conserva la lista anterior, así que la tabla no parpadea.
- **Búsqueda del backend:** no distingue mayúsculas, pero sí tildes.

