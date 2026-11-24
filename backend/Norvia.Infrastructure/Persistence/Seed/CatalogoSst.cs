using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>
/// Contenido SST inicial (HU-097, D7, D26): peligros con sus plantillas de tareas y, por rubro, categorías de riesgo
/// con sus preguntas exploratorias. Redacción propia del equipo, basada en la práctica habitual de SST y en el
/// reglamento STSS-053-04; <b>pendiente de revisión por una persona con conocimiento de SST</b> (criterio de HU-097).
/// <para>
/// <see cref="CatalogoSstSeeder"/> lo carga al arrancar y solo agrega lo que falta: no pisa lo que el staff edite o
/// desactive (HU-023). Los Id salen del código y de la posición de cada elemento, así que <b>no se reordena, no se borra
/// y no se cambia el código</b>: lo nuevo se agrega al final de su lista, y lo que sobre se desactiva desde la
/// administración de catálogos. Los códigos siguen su regla (minúsculas, números y guion bajo).
/// </para>
/// </summary>
public static class CatalogoSst
{
    public sealed record PlantillaSemilla(TipoControl? Tipo, string Titulo, string? Descripcion, short Prioridad = 2, bool Evidencia = false);

    public sealed record PeligroSemilla(
        string Codigo, string Nombre, string Descripcion, ClasificacionPeligro Clasificacion, PlantillaSemilla[] Plantillas);

    public sealed record OpcionSemilla(string Texto, bool IndicaExposicion);

    public sealed record PreguntaSemilla(string Texto, string TipoCodigo, string? PeligroCodigo, bool Obligatoria, OpcionSemilla[] Opciones);

    public sealed record CategoriaSemilla(string RubroCodigo, string Clave, string Nombre, string Descripcion, PreguntaSemilla[] Preguntas);

    // ---------- Atajos de redacción ----------

    private static OpcionSemilla No(string texto) => new(texto, false);
    private static OpcionSemilla Expone(string texto) => new(texto, true);

    private static PreguntaSemilla Unica(string texto, string peligro, params OpcionSemilla[] opciones) =>
        new(texto, "seleccion_unica", peligro, true, opciones);

    private static PreguntaSemilla Multiple(string texto, string peligro, params OpcionSemilla[] opciones) =>
        new(texto, "seleccion_multiple", peligro, true, opciones);

    /// <summary>Pregunta abierta y opcional: no indica peligro (D26), sirve para que la persona cuente lo que falta.</summary>
    private static PreguntaSemilla Abierta(string texto) => new(texto, "respuesta_breve", null, false, []);

    private static PlantillaSemilla Control(TipoControl tipo, string titulo, string? descripcion = null, short prioridad = 2, bool evidencia = false) =>
        new(tipo, titulo, descripcion, prioridad, evidencia);

    /// <summary>Tarea modelo: no es un control, entra directo al plan del riesgo (HU-042).</summary>
    private static PlantillaSemilla Tarea(string titulo, string? descripcion = null, short prioridad = 2, bool evidencia = true) =>
        new(null, titulo, descripcion, prioridad, evidencia);

    private const string PreguntaFinal = "¿Hay algo más de tu trabajo que te parezca peligroso o te cause malestar? Cuéntalo con tus palabras.";

    // ---------- Peligros y plantillas ----------

    public static readonly PeligroSemilla[] Peligros =
    [
        new("loc_caida_nivel", "Caída al mismo nivel",
            "Resbalones y tropiezos por pisos mojados, desordenados, con desniveles o en mal estado.", ClasificacionPeligro.Locativo,
            [
                Control(TipoControl.Ingenieria, "Reparar desniveles y pisos dañados", "Rellenar huecos, fijar alfombras y colocar piso antideslizante donde haya agua."),
                Control(TipoControl.Administrativo, "Mantener pasillos despejados", "Definir dónde se guardan cajas y cables para que no queden en las zonas de paso."),
                Control(TipoControl.Administrativo, "Señalizar el piso mojado", "Colocar el rótulo de piso mojado mientras se limpia y secar enseguida."),
                Tarea("Inspeccionar pisos y pasillos cada mes", "Recorrer el área, anotar lo que hay que corregir y tomar una foto."),
            ]),
        new("loc_caida_altura", "Caída de altura",
            "Trabajo en techos, andamios, losas, escaleras o bordes sin protección.", ClasificacionPeligro.Locativo,
            [
                Control(TipoControl.Eliminacion, "Hacer el trabajo desde el suelo cuando se pueda", "Armar o preparar las piezas abajo y subirlas listas.", 1),
                Control(TipoControl.Ingenieria, "Instalar barandas o líneas de vida en los bordes", "Barandas firmes con rodapié, o un punto de anclaje para el arnés.", 1, true),
                Control(TipoControl.Administrativo, "Autorizar cada trabajo en altura", "Revisar antes de subir: escalera o andamio en buen estado, clima y persona capacitada.", 1),
                Control(TipoControl.Epp, "Usar arnés de cuerpo completo amarrado a un punto seguro", null, 1),
                Tarea("Revisar andamios, escaleras y arneses antes de usarlos", "Anotar el estado y retirar lo que esté dañado.", 1),
            ]),
        new("loc_almacen", "Caída de objetos almacenados",
            "Estanterías sin anclar, cargas apiladas muy alto o mercadería mal asegurada.", ClasificacionPeligro.Locativo,
            [
                Control(TipoControl.Ingenieria, "Anclar las estanterías a la pared o al piso", null, 2, true),
                Control(TipoControl.Administrativo, "Guardar lo pesado en los niveles bajos", "Lo más pesado abajo y a la altura de la cintura; nunca sobre el último nivel."),
                Control(TipoControl.Administrativo, "Rotular la carga máxima de cada estante"),
                Tarea("Revisar el estado de las estanterías cada trimestre", "Buscar piezas dobladas, tornillos flojos y sobrecarga."),
            ]),
        new("loc_excavacion", "Derrumbe en excavaciones",
            "Trabajo dentro de zanjas o excavaciones cuyas paredes pueden ceder.", ClasificacionPeligro.Locativo,
            [
                Control(TipoControl.Ingenieria, "Entibar o dar talud a las paredes de la excavación", "Apuntalar las paredes o hacerlas inclinadas según el tipo de suelo.", 1),
                Control(TipoControl.Administrativo, "Dejar el material excavado lejos del borde", "Al menos a un metro del borde, para no cargar las paredes.", 1),
                Control(TipoControl.Administrativo, "Colocar una escalera de salida y nunca trabajar solo dentro de la zanja", null, 1),
                Tarea("Revisar las paredes de la excavación al inicio de cada jornada y después de llover", null, 1),
            ]),
        new("mec_maquina", "Atrapamiento con partes de máquinas",
            "Bandas, engranajes, rodillos o agujas en movimiento sin guarda.", ClasificacionPeligro.Mecanico,
            [
                Control(TipoControl.Ingenieria, "Instalar o reponer las guardas de las máquinas", "Que cubran las partes en movimiento sin estorbar el trabajo.", 1, true),
                Control(TipoControl.Ingenieria, "Colocar paro de emergencia al alcance de quien opera", null, 1),
                Control(TipoControl.Administrativo, "Apagar y bloquear la máquina antes de limpiarla o repararla", "Nadie mete las manos con la máquina encendida.", 1),
                Control(TipoControl.Epp, "No usar ropa suelta, anillos ni pelo suelto cerca de la máquina"),
                Tarea("Revisar guardas y paros de emergencia cada mes", "Anotar las máquinas revisadas y lo que falta corregir.", 1),
            ]),
        new("mec_herramienta", "Cortes con herramientas",
            "Tijeras, cuchillas, cúteres, sierras o cortadoras con filo o punta.", ClasificacionPeligro.Mecanico,
            [
                Control(TipoControl.Sustitucion, "Usar cúteres de cuchilla retráctil o con protector"),
                Control(TipoControl.Administrativo, "Guardar las herramientas cortantes en un lugar fijo y con su funda"),
                Control(TipoControl.Administrativo, "Capacitar en el uso seguro de herramientas de corte"),
                Control(TipoControl.Epp, "Usar guantes anticorte en las tareas de corte"),
                Tarea("Revisar el estado de las herramientas de corte cada mes", "Retirar las que tengan el mango flojo o el protector dañado."),
            ]),
        new("mec_vehiculo", "Golpes por montacargas o vehículos",
            "Montacargas, carretillas o vehículos que circulan cerca de las personas.", ClasificacionPeligro.Mecanico,
            [
                Control(TipoControl.Ingenieria, "Separar con barreras el paso de personas y de vehículos", null, 1),
                Control(TipoControl.Administrativo, "Marcar en el piso los pasillos de circulación", null, 1, true),
                Control(TipoControl.Administrativo, "Permitir manejar montacargas solo a personas capacitadas", null, 1),
                Control(TipoControl.Epp, "Usar chaleco reflectivo en las zonas de carga"),
                Tarea("Revisar frenos, bocina y alarma de reversa de los montacargas cada semana", null, 1),
            ]),
        new("ele_contacto", "Contacto eléctrico",
            "Cables dañados, tomacorrientes sobrecargados, extensiones en cadena o tableros sin tapa.", ClasificacionPeligro.Electrico,
            [
                Control(TipoControl.Ingenieria, "Reemplazar cables pelados y tomacorrientes dañados", "Que lo haga un electricista.", 1, true),
                Control(TipoControl.Ingenieria, "Instalar tapas y protección diferencial en los tableros", null, 1),
                Control(TipoControl.Administrativo, "No conectar extensiones una tras otra ni sobrecargar tomacorrientes"),
                Control(TipoControl.Administrativo, "Mantener los tableros eléctricos cerrados y despejados"),
                Tarea("Revisar la instalación eléctrica cada seis meses", "Con un electricista; anotar lo encontrado.", 1),
            ]),
        new("fis_ruido", "Ruido",
            "Máquinas, herramientas o equipos que obligan a alzar la voz para hablar.", ClasificacionPeligro.Fisico,
            [
                Control(TipoControl.Sustitucion, "Elegir equipos más silenciosos al cambiarlos"),
                Control(TipoControl.Ingenieria, "Encerrar o aislar las máquinas más ruidosas", null, 2, true),
                Control(TipoControl.Administrativo, "Rotar a las personas entre puestos ruidosos y tranquilos"),
                Control(TipoControl.Epp, "Usar protectores auditivos en las áreas ruidosas"),
                Tarea("Medir el ruido de las áreas de trabajo una vez al año", "Con un sonómetro; anotar los resultados por área."),
            ]),
        new("fis_calor", "Calor",
            "Trabajo prolongado al sol o en áreas calurosas y poco ventiladas.", ClasificacionPeligro.Fisico,
            [
                Control(TipoControl.Ingenieria, "Mejorar la ventilación o instalar extractores en las áreas calurosas", null, 2, true),
                Control(TipoControl.Ingenieria, "Colocar sombra en las zonas de trabajo al aire libre"),
                Control(TipoControl.Administrativo, "Programar pausas y tener agua potable disponible", "Hacer las tareas más pesadas en las horas más frescas."),
                Control(TipoControl.Epp, "Usar ropa ligera, gorra o casco con ala y bloqueador solar"),
                Tarea("Revisar que haya agua potable y sombra en cada frente de trabajo", null, 2),
            ]),
        new("fis_iluminacion", "Iluminación deficiente",
            "Poca luz, reflejos o luz que parpadea en el puesto de trabajo.", ClasificacionPeligro.Fisico,
            [
                Control(TipoControl.Ingenieria, "Cambiar las lámparas dañadas y agregar luz en los puestos oscuros", null, 3, true),
                Control(TipoControl.Ingenieria, "Usar cortinas o persianas para evitar reflejos en las pantallas", null, 3),
                Control(TipoControl.Administrativo, "Mantener limpias las lámparas y ventanas", null, 3),
                Tarea("Revisar la iluminación de las áreas cada seis meses", null, 3),
            ]),
        new("qui_sustancias", "Sustancias químicas",
            "Limpiadores, solventes, pinturas o combustibles sin etiqueta o sin su hoja de seguridad.", ClasificacionPeligro.Quimico,
            [
                Control(TipoControl.Sustitucion, "Cambiar los productos más peligrosos por otros de menor riesgo", "Por ejemplo, limpiadores neutros en lugar de ácidos."),
                Control(TipoControl.Ingenieria, "Guardar los químicos en un lugar ventilado, lejos del calor y de los alimentos"),
                Control(TipoControl.Administrativo, "Etiquetar todos los envases y tener la hoja de seguridad de cada producto", null, 2, true),
                Control(TipoControl.Administrativo, "No mezclar productos de limpieza", "Por ejemplo, cloro con amoníaco o con ácidos."),
                Control(TipoControl.Epp, "Usar guantes y lentes al manipular químicos"),
                Tarea("Revisar el inventario de químicos y sus etiquetas cada trimestre"),
            ]),
        new("qui_polvo", "Polvo en el aire",
            "Polvo de cemento, madera, textil u otros materiales que se respira.", ClasificacionPeligro.Quimico,
            [
                Control(TipoControl.Ingenieria, "Humedecer el material o usar extracción al cortar o barrer", null, 2, true),
                Control(TipoControl.Administrativo, "Limpiar con aspiradora o trapo húmedo en lugar de barrer en seco"),
                Control(TipoControl.Epp, "Usar mascarilla para polvo en las tareas que lo generan"),
                Tarea("Revisar cada mes que las mascarillas estén disponibles y en buen estado"),
            ]),
        new("bio_agentes", "Agentes biológicos",
            "Contacto con residuos, aguas sucias, plagas o personas enfermas.", ClasificacionPeligro.Biologico,
            [
                Control(TipoControl.Ingenieria, "Instalar lavamanos con jabón cerca de las áreas de trabajo"),
                Control(TipoControl.Administrativo, "Sacar la basura a diario y controlar las plagas", null, 2, true),
                Control(TipoControl.Epp, "Usar guantes al manipular residuos"),
                Tarea("Fumigar y revisar las trampas contra plagas cada trimestre"),
            ]),
        new("erg_pantalla", "Trabajo prolongado frente a pantalla",
            "Muchas horas sentado frente a la computadora, con la pantalla o la silla mal ajustadas.", ClasificacionPeligro.Ergonomico,
            [
                Control(TipoControl.Ingenieria, "Dar sillas ajustables y colocar la pantalla a la altura de los ojos", null, 3, true),
                Control(TipoControl.Administrativo, "Hacer pausas activas de cinco minutos cada hora", null, 3),
                Control(TipoControl.Administrativo, "Capacitar en cómo ajustar el puesto de trabajo", null, 3),
                Tarea("Revisar el ajuste de los puestos con computadora cada seis meses", null, 3),
            ]),
        new("erg_carga", "Levantar y cargar peso a mano",
            "Levantar, cargar o empujar cajas, bultos o materiales pesados.", ClasificacionPeligro.Ergonomico,
            [
                Control(TipoControl.Eliminacion, "Usar carretillas, montacargas o rodos para mover lo pesado"),
                Control(TipoControl.Sustitucion, "Pedir los materiales en presentaciones más livianas", "Por ejemplo, bolsas de 25 kilos en lugar de 50."),
                Control(TipoControl.Administrativo, "Capacitar en cómo levantar cargas y hacerlo entre dos personas"),
                Control(TipoControl.Epp, "Usar guantes y calzado con puntera en las tareas de carga"),
                Tarea("Revisar que las carretillas y rodos estén en buen estado cada mes"),
            ]),
        new("erg_repetitivo", "Movimientos repetitivos",
            "Repetir el mismo movimiento con manos y brazos durante horas, como en costura o empaque.", ClasificacionPeligro.Ergonomico,
            [
                Control(TipoControl.Ingenieria, "Ajustar la altura de mesas y máquinas a la persona"),
                Control(TipoControl.Administrativo, "Rotar a las personas entre tareas distintas durante la jornada"),
                Control(TipoControl.Administrativo, "Hacer pausas activas con estiramientos de manos y hombros"),
                Tarea("Registrar las molestias de manos y hombros que reporte el personal cada trimestre"),
            ]),
        new("erg_de_pie", "Trabajo de pie prolongado",
            "Pasar la mayor parte del turno de pie, sin poder sentarse.", ClasificacionPeligro.Ergonomico,
            [
                Control(TipoControl.Ingenieria, "Colocar alfombras antifatiga y un banco para alternar", null, 3, true),
                Control(TipoControl.Administrativo, "Programar pausas para sentarse durante el turno", null, 3),
                Control(TipoControl.Epp, "Usar calzado cómodo y cerrado", null, 3),
                Tarea("Consultar al personal de piso sobre cansancio y molestias cada seis meses", null, 3),
            ]),
        new("psi_carga", "Sobrecarga de trabajo",
            "Jornadas largas, ritmo impuesto o metas difíciles de cumplir sin pausas.", ClasificacionPeligro.Psicosocial,
            [
                Control(TipoControl.Administrativo, "Revisar la carga de trabajo y las metas con cada equipo"),
                Control(TipoControl.Administrativo, "Respetar las pausas y la hora de salida"),
                Control(TipoControl.Administrativo, "Dar un canal para reportar sobrecarga sin represalias"),
                Tarea("Aplicar una encuesta de clima laboral cada semestre", null, 2, false),
            ]),
        new("psi_violencia", "Violencia de terceros",
            "Asaltos, amenazas o clientes agresivos en la atención al público.", ClasificacionPeligro.Psicosocial,
            [
                Control(TipoControl.Ingenieria, "Instalar cámaras, botón de pánico y caja con poco efectivo", null, 1, true),
                Control(TipoControl.Administrativo, "Capacitar en cómo actuar ante un asalto o un cliente agresivo", null, 1),
                Control(TipoControl.Administrativo, "No dejar a una sola persona en el local al abrir o cerrar", null, 1),
                Tarea("Revisar cada trimestre que cámaras y botón de pánico funcionen", null, 1),
            ]),
        new("otr_incendio", "Incendio",
            "Materiales que arden, instalaciones eléctricas recargadas o falta de extintores y salidas.", ClasificacionPeligro.Otro,
            [
                Control(TipoControl.Ingenieria, "Instalar extintores adecuados y señalizar las salidas de emergencia", null, 1, true),
                Control(TipoControl.Administrativo, "Mantener las salidas y los extintores despejados", null, 1),
                Control(TipoControl.Administrativo, "Hacer un simulacro de evacuación al año", null, 2, true),
                Tarea("Revisar la carga y la fecha de los extintores cada mes", "Anotar cada extintor revisado.", 1),
            ]),
    ];

    // ---------- Categorías y preguntas por rubro ----------

    public static readonly CategoriaSemilla[] Categorias =
    [
        // Oficina administrativa
        new("oficina", "pantallas", "Puesto de trabajo y pantallas",
            "Postura, pantalla e iluminación en los puestos con computadora.",
            [
                Unica("¿Cuántas horas al día trabajas frente a una computadora?", "erg_pantalla",
                    No("Menos de 2 horas"), No("Entre 2 y 4 horas"), Expone("Más de 4 horas")),
                Multiple("¿Qué molestias has sentido al terminar tu jornada?", "erg_pantalla",
                    Expone("Dolor de espalda o cuello"), Expone("Dolor en muñecas o manos"), Expone("Vista cansada"), No("Ninguna")),
                Unica("¿La luz de tu puesto te deja leer sin forzar la vista?", "fis_iluminacion",
                    No("Sí"), Expone("A veces"), Expone("No")),
            ]),
        new("oficina", "instalaciones", "Instalaciones y orden",
            "Pasillos, instalación eléctrica y emergencias en la oficina.",
            [
                Unica("¿Hay cables sueltos, alfombras levantadas o cajas en los pasillos?", "loc_caida_nivel",
                    Expone("Sí, con frecuencia"), Expone("A veces"), No("No")),
                Unica("¿Has visto tomacorrientes sobrecargados, extensiones una tras otra o cables pelados?", "ele_contacto",
                    Expone("Sí"), No("No"), No("No me he fijado")),
                Unica("¿Sabes dónde están el extintor y la salida de emergencia más cercanos?", "otr_incendio",
                    No("Sí, los dos"), Expone("Solo uno de los dos"), Expone("No")),
            ]),
        new("oficina", "bienestar", "Carga de trabajo y bienestar",
            "Horas extra, ritmo de trabajo y lo que el personal quiera contar.",
            [
                Unica("¿Con qué frecuencia te quedas más horas para terminar tu trabajo?", "psi_carga",
                    No("Casi nunca"), No("Algunas veces al mes"), Expone("Casi todas las semanas")),
                Abierta(PreguntaFinal),
            ]),

        // Comercio o tienda
        new("comercio", "bodega", "Bodega y almacenamiento",
            "Estanterías, mercadería en lo alto y carga manual.",
            [
                Unica("¿Las estanterías de la bodega están ancladas a la pared o al piso?", "loc_almacen",
                    No("Sí"), Expone("No"), No("No sé")),
                Unica("¿Cargas a mano cajas o bultos pesados?", "erg_carga",
                    No("Nunca"), Expone("A veces"), Expone("Todos los días")),
                Unica("¿Cómo alcanzas la mercadería que está en lo alto?", "loc_caida_altura",
                    No("No tengo que alcanzar nada en lo alto"), No("Con una escalera en buen estado"),
                    Expone("Con una escalera floja o dañada"), Expone("Subiéndome a sillas, cajas o estantes")),
            ]),
        new("comercio", "atencion", "Atención al público y caja",
            "Trato con clientes, seguridad del local y tiempo de pie.",
            [
                Unica("¿Has vivido o sabes de asaltos, amenazas o clientes agresivos en el local?", "psi_violencia",
                    No("No"), Expone("Sí, alguna vez"), Expone("Sí, con frecuencia")),
                Unica("¿Pasas casi todo tu turno de pie, sin poder sentarte?", "erg_de_pie",
                    No("No"), Expone("Sí")),
            ]),
        new("comercio", "limpieza", "Limpieza y local",
            "Productos de limpieza, pisos y lo que el personal quiera contar.",
            [
                Multiple("¿Qué productos usas para limpiar el local?", "qui_sustancias",
                    Expone("Cloro o lejía"), Expone("Desengrasantes o ácidos"), No("Solo agua y jabón"), No("No hago limpieza")),
                Unica("¿El piso queda mojado o resbaloso mientras la gente camina?", "loc_caida_nivel",
                    No("Nunca"), Expone("A veces"), Expone("Casi siempre")),
                Abierta(PreguntaFinal),
            ]),

        // Maquila o fábrica
        new("maquila_fabrica", "maquinas", "Máquinas y herramientas",
            "Guardas, herramientas de corte y circulación de montacargas.",
            [
                Unica("¿Las máquinas que usas tienen sus guardas o protecciones puestas?", "mec_maquina",
                    No("Sí, todas"), Expone("Algunas no"), Expone("No tienen"), No("No uso máquinas")),
                Unica("¿Usas tijeras, cuchillas o cúteres en tu trabajo?", "mec_herramienta",
                    No("No"), Expone("Sí, a veces"), Expone("Sí, todo el día")),
                Unica("¿Pasan montacargas o carretillas cerca de donde trabajas?", "mec_vehiculo",
                    No("No"), Expone("Sí, por pasillos marcados"), Expone("Sí, sin pasillos marcados")),
            ]),
        new("maquila_fabrica", "ambiente", "Ambiente de la planta",
            "Ruido, calor y polvo en las áreas de producción.",
            [
                Unica("¿Tienes que alzar la voz para hablar con alguien a un metro de distancia?", "fis_ruido",
                    No("No"), Expone("A veces"), Expone("Casi siempre")),
                Unica("¿Hace mucho calor en tu área durante la jornada?", "fis_calor",
                    No("No"), Expone("A veces"), Expone("Casi siempre")),
                Unica("¿Hay polvo o pelusa en el aire de tu área?", "qui_polvo",
                    No("No"), No("Un poco"), Expone("Mucho")),
            ]),
        new("maquila_fabrica", "tareas", "Tareas y ritmo de trabajo",
            "Movimientos repetitivos, metas de producción y lo que el personal quiera contar.",
            [
                Multiple("¿Qué molestias has sentido en las últimas semanas?", "erg_repetitivo",
                    Expone("Dolor en muñecas o manos"), Expone("Dolor de hombros o cuello"), Expone("Hormigueo en los dedos"), No("Ninguna")),
                Unica("¿Cumplir la meta de producción te obliga a saltarte las pausas?", "psi_carga",
                    No("Nunca"), Expone("A veces"), Expone("Casi siempre")),
                Abierta(PreguntaFinal),
            ]),

        // Construcción
        new("construccion", "alturas", "Trabajo en alturas",
            "Techos, andamios, losas y escaleras.",
            [
                Unica("¿Trabajas en techos, andamios, losas o escaleras?", "loc_caida_altura",
                    No("No"), Expone("Sí, a veces"), Expone("Sí, casi todos los días")),
                Unica("Cuando trabajas en altura, ¿usas arnés amarrado a un punto seguro?", "loc_caida_altura",
                    No("Siempre"), Expone("A veces"), Expone("Nunca"), No("No trabajo en altura")),
            ]),
        new("construccion", "obra", "Excavaciones, materiales y herramientas",
            "Zanjas, carga de materiales y herramientas de corte.",
            [
                Unica("¿Trabajas dentro de zanjas o excavaciones más profundas que tu cintura?", "loc_excavacion",
                    No("No"), Expone("Sí")),
                Unica("¿Cargas a mano bolsas de cemento, bloques o varillas?", "erg_carga",
                    No("No"), Expone("A veces"), Expone("Todos los días")),
                Unica("¿Usas sierras, cortadoras o pulidoras?", "mec_herramienta",
                    No("No"), Expone("Sí")),
            ]),
        new("construccion", "sitio", "Condiciones del sitio",
            "Sol, instalación eléctrica provisional y polvo en la obra.",
            [
                Unica("¿Trabajas a pleno sol la mayor parte del día?", "fis_calor",
                    No("No"), Expone("Parte del día"), Expone("Casi todo el día")),
                Unica("¿Hay cables o extensiones eléctricas tirados por el suelo de la obra?", "ele_contacto",
                    No("No"), Expone("Sí")),
                Unica("¿Hay polvo de cemento, arena o madera en el aire donde trabajas?", "qui_polvo",
                    No("No"), Expone("A veces"), Expone("Casi siempre")),
                Abierta(PreguntaFinal),
            ]),
    ];
}
