using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo;
using Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarPeligro;
using Norvia.Application.Features.Catalogos.ActualizarPeligro.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarPregunta;
using Norvia.Application.Features.Catalogos.ActualizarPregunta.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarRubro;
using Norvia.Application.Features.Catalogos.ActualizarRubro.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo;
using Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoPeligro;
using Norvia.Application.Features.Catalogos.CambiarEstadoPeligro.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoPregunta;
using Norvia.Application.Features.Catalogos.CambiarEstadoPregunta.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoRubro;
using Norvia.Application.Features.Catalogos.CambiarEstadoRubro.DTOs;
using Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo;
using Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo.DTOs;
using Norvia.Application.Features.Catalogos.CrearPeligro;
using Norvia.Application.Features.Catalogos.CrearPeligro.DTOs;
using Norvia.Application.Features.Catalogos.CrearPlantillaDocumento;
using Norvia.Application.Features.Catalogos.CrearPlantillaDocumento.DTOs;
using Norvia.Application.Features.Catalogos.CrearPregunta;
using Norvia.Application.Features.Catalogos.CrearPregunta.DTOs;
using Norvia.Application.Features.Catalogos.CrearRubro;
using Norvia.Application.Features.Catalogos.CrearRubro.DTOs;
using Norvia.Application.Features.Catalogos.ListarCategoriasRiesgo;
using Norvia.Application.Features.Catalogos.ListarPeligros;
using Norvia.Application.Features.Catalogos.ListarPlantillasDocumento;
using Norvia.Application.Features.Catalogos.ListarPreguntas;
using Norvia.Application.Features.Catalogos.ListarRubros;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaTarea;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaTarea.DTOs;
using Norvia.Application.Features.Catalogos.CrearPlantillaTarea;
using Norvia.Application.Features.Catalogos.CrearPlantillaTarea.DTOs;
using Norvia.Application.Features.Catalogos.ListarPlantillasTarea;

namespace Norvia.API.Controllers;

/// <summary>
/// Módulo Catalogos (HU-023): el staff de plataforma mantiene los catálogos globales.
/// Las rutas no llevan prefijo; el alcance plataforma lo fija cada fila de CatalogoPermisos.
/// Editar es PATCH parcial y activar/desactivar es PATCH .../estado: no hay borrado físico.
/// </summary>
[ApiController]
[Route("api")]
public sealed class CatalogosController : ControllerBase
{
    // ---- api/rubros ----

    [HttpGet("rubros")]
    [ProducesResponseType(typeof(IReadOnlyList<RubroResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarRubros(
        [FromServices] ListarRubrosServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    [HttpPost("rubros")]
    [ProducesResponseType(typeof(RubroResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearRubro(
        CrearRubroRequestDto request, [FromServices] CrearRubroServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    [HttpPatch("rubros/{id:int}")]
    [ProducesResponseType(typeof(RubroResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarRubro(
        short id, ActualizarRubroRequestDto request, [FromServices] ActualizarRubroServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    [HttpPatch("rubros/{id:int}/estado")]
    [ProducesResponseType(typeof(RubroResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstadoRubro(
        short id, CambiarEstadoRubroRequestDto request, [FromServices] CambiarEstadoRubroServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    // ---- api/categorias-riesgo ----

    [HttpGet("categorias-riesgo")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaRiesgoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarCategoriasRiesgo(
        [FromQuery] short? rubroId, [FromServices] ListarCategoriasRiesgoServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(rubroId, cancellationToken));

    [HttpPost("categorias-riesgo")]
    [ProducesResponseType(typeof(CategoriaRiesgoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearCategoriaRiesgo(
        CrearCategoriaRiesgoRequestDto request, [FromServices] CrearCategoriaRiesgoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    [HttpPatch("categorias-riesgo/{id:guid}")]
    [ProducesResponseType(typeof(CategoriaRiesgoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarCategoriaRiesgo(
        Guid id, ActualizarCategoriaRiesgoRequestDto request, [FromServices] ActualizarCategoriaRiesgoServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    [HttpPatch("categorias-riesgo/{id:guid}/estado")]
    [ProducesResponseType(typeof(CategoriaRiesgoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstadoCategoriaRiesgo(
        Guid id, CambiarEstadoCategoriaRiesgoRequestDto request, [FromServices] CambiarEstadoCategoriaRiesgoServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    // ---- api/peligros ----

    [HttpGet("peligros")]
    [ProducesResponseType(typeof(IReadOnlyList<PeligroResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPeligros(
        [FromServices] ListarPeligrosServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    [HttpPost("peligros")]
    [ProducesResponseType(typeof(PeligroResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearPeligro(
        CrearPeligroRequestDto request, [FromServices] CrearPeligroServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    [HttpPatch("peligros/{id:guid}")]
    [ProducesResponseType(typeof(PeligroResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarPeligro(
        Guid id, ActualizarPeligroRequestDto request, [FromServices] ActualizarPeligroServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    [HttpPatch("peligros/{id:guid}/estado")]
    [ProducesResponseType(typeof(PeligroResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstadoPeligro(
        Guid id, CambiarEstadoPeligroRequestDto request, [FromServices] CambiarEstadoPeligroServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    // ---- api/preguntas ----

    [HttpGet("preguntas")]
    [ProducesResponseType(typeof(IReadOnlyList<PreguntaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPreguntas(
        [FromQuery] Guid? categoriaRiesgoId, [FromServices] ListarPreguntasServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(categoriaRiesgoId, cancellationToken));

    [HttpPost("preguntas")]
    [ProducesResponseType(typeof(PreguntaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearPregunta(
        CrearPreguntaRequestDto request, [FromServices] CrearPreguntaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    [HttpPatch("preguntas/{id:guid}")]
    [ProducesResponseType(typeof(PreguntaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarPregunta(
        Guid id, ActualizarPreguntaRequestDto request, [FromServices] ActualizarPreguntaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    [HttpPatch("preguntas/{id:guid}/estado")]
    [ProducesResponseType(typeof(PreguntaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstadoPregunta(
        Guid id, CambiarEstadoPreguntaRequestDto request, [FromServices] CambiarEstadoPreguntaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    // ---- api/plantillas ----

    [HttpGet("plantillas")]
    [ProducesResponseType(typeof(IReadOnlyList<PlantillaDocumentoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPlantillasDocumento(
        [FromServices] ListarPlantillasDocumentoServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    [HttpPost("plantillas")]
    [ProducesResponseType(typeof(PlantillaDocumentoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearPlantillaDocumento(
        CrearPlantillaDocumentoRequestDto request, [FromServices] CrearPlantillaDocumentoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    [HttpPatch("plantillas/{id:guid}")]
    [ProducesResponseType(typeof(PlantillaDocumentoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarPlantillaDocumento(
        Guid id, ActualizarPlantillaDocumentoRequestDto request, [FromServices] ActualizarPlantillaDocumentoServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    [HttpPatch("plantillas/{id:guid}/estado")]
    [ProducesResponseType(typeof(PlantillaDocumentoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstadoPlantillaDocumento(
        Guid id, CambiarEstadoPlantillaDocumentoRequestDto request, [FromServices] CambiarEstadoPlantillaDocumentoServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    // ---- api/plantillas-tarea (controles sugeridos y tareas modelo de cada peligro) ----

    [HttpGet("plantillas-tarea")]
    [ProducesResponseType(typeof(IReadOnlyList<PlantillaTareaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPlantillasTarea(
        [FromQuery] Guid? peligroId, [FromServices] ListarPlantillasTareaServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(peligroId, cancellationToken));

    [HttpPost("plantillas-tarea")]
    [ProducesResponseType(typeof(PlantillaTareaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearPlantillaTarea(
        CrearPlantillaTareaRequestDto request, [FromServices] CrearPlantillaTareaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    [HttpPatch("plantillas-tarea/{id:guid}")]
    [ProducesResponseType(typeof(PlantillaTareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarPlantillaTarea(
        Guid id, ActualizarPlantillaTareaRequestDto request, [FromServices] ActualizarPlantillaTareaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    [HttpPatch("plantillas-tarea/{id:guid}/estado")]
    [ProducesResponseType(typeof(PlantillaTareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstadoPlantillaTarea(
        Guid id, CambiarEstadoPlantillaTareaRequestDto request, [FromServices] CambiarEstadoPlantillaTareaServicio servicio,
        CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);
}
