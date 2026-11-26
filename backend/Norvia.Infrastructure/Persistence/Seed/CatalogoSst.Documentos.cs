using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>
/// Plantillas de documento iniciales (HU-070, D13): la política de SST y el procedimiento de investigación de
/// accidentes, para todos los rubros. Contenido en Markdown con los marcadores de 12-modelo-datos § PlantillaDocumento;
/// lo que solo la empresa sabe va como <c>[Completar: …]</c>. Redacción propia basada en el reglamento STSS-053-04
/// (art. 9.j, 46.1 y 47) y en ISO 45001 (§ 5.2, política); <b>pendiente de revisión por una persona con conocimiento de
/// SST</b>, igual que el resto del contenido inicial.
/// <para>Igual que los peligros: no se cambia el código de una plantilla ya publicada.</para>
/// </summary>
public static partial class CatalogoSst
{
    /// <param name="RubroCodigo">Nulo: aplica a todos los rubros.</param>
    public sealed record PlantillaDocumentoSemilla(string Codigo, string Titulo, TipoDocumento Tipo, string? RubroCodigo, string Contenido);

    public static readonly PlantillaDocumentoSemilla[] PlantillasDocumento =
    [
        new("politica_sst", "Política de seguridad y salud en el trabajo", TipoDocumento.Politica, null,
            """
            # Política de seguridad y salud en el trabajo

            **{{RazonSocial}}** (RTN {{RTN}}), con domicilio en {{Direccion}}, se dedica a {{ActividadEconomica}} (rubro: {{Rubro}}). La dirección de la empresa reconoce que las personas que trabajan con nosotros son lo más importante y asume la seguridad y la salud en el trabajo como parte de su forma de trabajar.

            ## Compromisos

            La empresa se compromete a:

            1. **Ofrecer condiciones de trabajo seguras y saludables** para prevenir lesiones y enfermedades relacionadas con el trabajo.
            2. **Cumplir la legislación hondureña** de seguridad y salud en el trabajo, en especial el Reglamento General de Medidas Preventivas de Accidentes de Trabajo y Enfermedades Profesionales (STSS-053-04), y los demás requisitos que la empresa adopte.
            3. **Identificar los peligros, evaluar los riesgos y controlarlos,** con prioridad en eliminarlos y, cuando no sea posible, en reducirlos con cambios en las instalaciones, procedimientos de trabajo y equipo de protección personal.
            4. **Investigar los accidentes, incidentes y enfermedades profesionales,** determinar sus causas y aplicar medidas correctivas para que no se repitan.
            5. **Consultar a los trabajadores y hacerlos participar** en las decisiones de seguridad y salud, y garantizar que puedan reportar condiciones inseguras sin temor a represalias.
            6. **Capacitar a las personas** sobre los riesgos de su puesto y la forma segura de hacer su trabajo.
            7. **Mejorar continuamente** la gestión de la seguridad y la salud, con objetivos que se revisan al menos una vez al año.

            ## Responsabilidades

            - **La dirección** asigna los recursos necesarios, aprueba el plan de trabajo y revisa sus resultados.
            - **El Responsable de SST** coordina la identificación de peligros, la evaluación de riesgos, el plan de trabajo y la investigación de los eventos.
            - **Los jefes y supervisores** se aseguran de que su equipo trabaje según las medidas definidas.
            - **Cada trabajador** cumple las normas de seguridad, usa el equipo de protección que se le entrega y reporta los eventos y las condiciones inseguras.

            ## Comunicación y revisión

            Esta política se comunica a todas las personas que trabajan en la empresa y está disponible para quien la solicite. Se revisa al menos una vez al año, o antes si cambian las actividades, los riesgos o la legislación.

            Emitida el {{Fecha}}.

            **Aprueba:** [Completar: nombre y cargo de quien aprueba]
            """),
        new("procedimiento_investigacion", "Procedimiento de investigación de accidentes e incidentes", TipoDocumento.Procedimiento, null,
            """
            # Procedimiento de investigación de accidentes e incidentes

            **Empresa:** {{RazonSocial}} · **RTN:** {{RTN}} · **Emitido el:** {{Fecha}}

            ## 1. Objetivo

            Definir cómo se reportan, investigan y registran los accidentes de trabajo, los incidentes, las enfermedades profesionales y las condiciones inseguras en {{RazonSocial}}, para conocer sus causas y aplicar medidas correctivas que eviten que se repitan (art. 46.1.a del reglamento STSS-053-04).

            ## 2. Alcance

            Aplica a todas las personas que trabajan en la empresa y a los eventos que ocurran en sus instalaciones ({{Direccion}}) o durante actividades de trabajo fuera de ellas.

            ## 3. Definiciones

            - **Accidente de trabajo:** suceso repentino durante el trabajo o con ocasión de él que causa una lesión a una persona.
            - **Incidente:** suceso que pudo causar una lesión o un daño, pero no lo causó.
            - **Enfermedad profesional:** enfermedad causada por las condiciones o los agentes a los que la persona está expuesta en su trabajo.
            - **Condición insegura:** situación del lugar, del equipo o de la forma de trabajar que puede provocar un accidente.

            ## 4. Responsabilidades

            - **Cada trabajador** reporta de inmediato todo evento o condición insegura que vea, aunque no haya personas lesionadas.
            - **El Responsable de SST** registra el evento, coordina la investigación, da seguimiento a las medidas correctivas y mantiene el registro y las estadísticas (art. 9.j, 46.1.d y 47.e).
            - **La dirección** aprueba y da los recursos para las medidas correctivas.

            ## 5. Pasos

            ### 5.1 Atención inmediata

            1. Atender a la persona afectada: primeros auxilios y, si hace falta, traslado a [Completar: centro de salud de referencia].
            2. Asegurar el área para evitar otro evento y conservar las evidencias: fotos, equipos y materiales involucrados.

            ### 5.2 Reporte

            3. Reportar el evento en Norvia lo antes posible, a más tardar al terminar la jornada. El reporte puede ser anónimo.
            4. Avisar a las autoridades cuando la ley lo exija: [Completar: a quién se avisa y en qué plazo].

            ### 5.3 Investigación

            5. El Responsable de SST inicia la investigación en un plazo de [Completar: plazo, por ejemplo 48 horas] desde el reporte.
            6. Reunir la información: inspección del lugar, conversaciones con quienes estaban presentes y documentos relacionados.
            7. Identificar las **causas inmediatas** (lo que se hizo o la condición que provocó el evento) y las **causas de fondo** (capacitación, mantenimiento, organización del trabajo, supervisión).
            8. Registrar las conclusiones de la investigación.

            ### 5.4 Medidas correctivas

            9. Definir al menos una medida correctiva por evento, con responsable y fecha límite. Las medidas se siguen como tareas del plan de trabajo.
            10. Revisar la evaluación de los riesgos relacionados con el evento y actualizarla si hace falta (art. 47.b).

            ### 5.5 Cierre

            11. El evento se cierra cuando tiene su investigación y sus medidas correctivas.
            12. Un reporte falso, duplicado o que no corresponde se descarta indicando el motivo, y no cuenta en las estadísticas.

            ## 6. Registros

            - El registro de eventos con su investigación y sus medidas (art. 9.j y 46.1.d).
            - Las estadísticas de eventos por tipo, gravedad y mes (art. 47.e).

            ## 7. Confidencialidad

            Los reportes pueden hacerse de forma anónima y nadie recibe represalias por reportar. La investigación busca causas, no culpables.

            **Aprueba:** [Completar: nombre y cargo de quien aprueba]
            """),
    ];
}
