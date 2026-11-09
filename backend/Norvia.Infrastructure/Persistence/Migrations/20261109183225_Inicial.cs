using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Peligro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Clasificacion = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peligro", x => x.Id);
                    table.CheckConstraint("CK_Peligro_Clasificacion", "\"Clasificacion\" IN ('fisico', 'mecanico', 'electrico', 'quimico', 'biologico', 'ergonomico', 'psicosocial', 'locativo', 'otro')");
                });

            migrationBuilder.CreateTable(
                name: "Permiso",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Alcance = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MetodoHttp = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RutaPatron = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permiso", x => x.Id);
                    table.CheckConstraint("CK_Permiso_Alcance", "\"Alcance\" IN ('empresa', 'plataforma')");
                    table.CheckConstraint("CK_Permiso_MetodoHttp", "\"MetodoHttp\" IN ('GET', 'POST', 'PUT', 'PATCH', 'DELETE')");
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimerNombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SegundoNombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    PrimerApellido = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SegundoApellido = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    TipoDocumento = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    NumeroDocumento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.Id);
                    table.CheckConstraint("CK_Persona_TipoDocumento", "\"TipoDocumento\" IN ('dni', 'pasaporte', 'otro')");
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolStaffPlataforma",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolStaffPlataforma", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rubro",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rubro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuario_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolPermiso",
                columns: table => new
                {
                    RolId = table.Column<short>(type: "smallint", nullable: false),
                    PermisoId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermiso", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolPermiso_Permiso_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permiso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolPermiso_Rol_RolId",
                        column: x => x.RolId,
                        principalTable: "Rol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolStaffPlataformaPermiso",
                columns: table => new
                {
                    RolStaffPlataformaId = table.Column<short>(type: "smallint", nullable: false),
                    PermisoId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolStaffPlataformaPermiso", x => new { x.RolStaffPlataformaId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolStaffPlataformaPermiso_Permiso_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permiso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolStaffPlataformaPermiso_RolStaffPlataforma_RolStaffPlataf~",
                        column: x => x.RolStaffPlataformaId,
                        principalTable: "RolStaffPlataforma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffPlataforma",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RolStaffPlataformaId = table.Column<short>(type: "smallint", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPlataforma", x => x.Id);
                    table.CheckConstraint("CK_StaffPlataforma_EmailEnMinusculas", "\"Email\" = lower(\"Email\")");
                    table.ForeignKey(
                        name: "FK_StaffPlataforma_RolStaffPlataforma_RolStaffPlataformaId",
                        column: x => x.RolStaffPlataformaId,
                        principalTable: "RolStaffPlataforma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoriaRiesgo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RubroId = table.Column<short>(type: "smallint", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaRiesgo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriaRiesgo_Rubro_RubroId",
                        column: x => x.RubroId,
                        principalTable: "Rubro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RazonSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RTN = table.Column<string>(type: "character(14)", fixedLength: true, maxLength: 14, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ActividadEconomica = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RubroId = table.Column<short>(type: "smallint", nullable: true),
                    TrabajadoresPermanentes = table.Column<int>(type: "integer", nullable: true),
                    FechaInicioOperaciones = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id);
                    table.CheckConstraint("CK_Empresa_Estado", "\"Estado\" IN ('activa', 'suspendida', 'inactiva')");
                    table.CheckConstraint("CK_Empresa_RTN", "\"RTN\" ~ '^[0-9]{14}$'");
                    table.CheckConstraint("CK_Empresa_TrabajadoresPermanentes", "\"TrabajadoresPermanentes\" >= 0");
                    table.ForeignKey(
                        name: "FK_Empresa_Rubro_RubroId",
                        column: x => x.RubroId,
                        principalTable: "Rubro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreguntaExploratoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoriaRiesgoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoRespuesta = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Orden = table.Column<short>(type: "smallint", nullable: false),
                    PeligroId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreguntaExploratoria", x => x.Id);
                    table.CheckConstraint("CK_PreguntaExploratoria_TextoSinPeligro", "\"TipoRespuesta\" <> 'texto' OR \"PeligroId\" IS NULL");
                    table.CheckConstraint("CK_PreguntaExploratoria_TipoRespuesta", "\"TipoRespuesta\" IN ('si_no', 'opcion_unica', 'texto')");
                    table.ForeignKey(
                        name: "FK_PreguntaExploratoria_CategoriaRiesgo_CategoriaRiesgoId",
                        column: x => x.CategoriaRiesgoId,
                        principalTable: "CategoriaRiesgo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreguntaExploratoria_Peligro_PeligroId",
                        column: x => x.PeligroId,
                        principalTable: "Peligro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialEstadoEmpresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    EstadoNuevo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    MotivoCodigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MotivoDetalle = table.Column<string>(type: "text", nullable: true),
                    RegistradoPorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistradoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadoEmpresa", x => x.Id);
                    table.UniqueConstraint("AK_HistorialEstadoEmpresa_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_HistorialEstadoEmpresa_EstadoAnterior", "\"EstadoAnterior\" IN ('activa', 'suspendida', 'inactiva')");
                    table.CheckConstraint("CK_HistorialEstadoEmpresa_EstadoNuevo", "\"EstadoNuevo\" IN ('activa', 'suspendida', 'inactiva')");
                    table.CheckConstraint("CK_HistorialEstadoEmpresa_MotivoCodigo", "\"MotivoCodigo\" IN ('falta_pago_plataforma', 'reactivacion', 'otro')");
                    table.ForeignKey(
                        name: "FK_HistorialEstadoEmpresa_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoEmpresa_StaffPlataforma_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "StaffPlataforma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VinculoEmpresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonaId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolId = table.Column<short>(type: "smallint", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VinculoEmpresa", x => x.Id);
                    table.UniqueConstraint("AK_VinculoEmpresa_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_VinculoEmpresa_EmailEnMinusculas", "\"Email\" = lower(\"Email\")");
                    table.CheckConstraint("CK_VinculoEmpresa_Estado", "\"Estado\" IN ('activo', 'suspendido', 'baja')");
                    table.ForeignKey(
                        name: "FK_VinculoEmpresa_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VinculoEmpresa_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VinculoEmpresa_Rol_RolId",
                        column: x => x.RolId,
                        principalTable: "Rol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpcionPregunta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreguntaExploratoriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Orden = table.Column<short>(type: "smallint", nullable: false),
                    IndicaExposicion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcionPregunta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpcionPregunta_PreguntaExploratoria_PreguntaExploratoriaId",
                        column: x => x.PreguntaExploratoriaId,
                        principalTable: "PreguntaExploratoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Area",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    EncargadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Area", x => x.Id);
                    table.UniqueConstraint("AK_Area_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.ForeignKey(
                        name: "FK_Area_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Area_VinculoEmpresa_EmpresaId_EncargadoId",
                        columns: x => new { x.EmpresaId, x.EncargadoId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialEstadoVinculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VinculoEmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    EstadoNuevo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    MotivoCodigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MotivoDetalle = table.Column<string>(type: "text", nullable: true),
                    RegistradoPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistradoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadoVinculo", x => x.Id);
                    table.UniqueConstraint("AK_HistorialEstadoVinculo_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_HistorialEstadoVinculo_EstadoAnterior", "\"EstadoAnterior\" IN ('activo', 'suspendido', 'baja')");
                    table.CheckConstraint("CK_HistorialEstadoVinculo_EstadoNuevo", "\"EstadoNuevo\" IN ('activo', 'suspendido', 'baja')");
                    table.CheckConstraint("CK_HistorialEstadoVinculo_MotivoCodigo", "\"MotivoCodigo\" IN ('decision_admin', 'baja_laboral', 'falta_pago_empresa', 'otro')");
                    table.ForeignKey(
                        name: "FK_HistorialEstadoVinculo_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoVinculo_VinculoEmpresa_EmpresaId_RegistradoP~",
                        columns: x => new { x.EmpresaId, x.RegistradoPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEstadoVinculo_VinculoEmpresa_EmpresaId_VinculoEmpr~",
                        columns: x => new { x.EmpresaId, x.VinculoEmpresaId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiesgoEmpresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeligroId = table.Column<Guid>(type: "uuid", nullable: true),
                    DescripcionPeligro = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Actividad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    EncuestaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    RevisadoPorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevisadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiesgoEmpresa", x => x.Id);
                    table.UniqueConstraint("AK_RiesgoEmpresa_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_RiesgoEmpresa_Estado", "\"Estado\" IN ('sugerido', 'confirmado', 'descartado')");
                    table.CheckConstraint("CK_RiesgoEmpresa_PeligroODescripcion", "\"PeligroId\" IS NOT NULL OR \"DescripcionPeligro\" IS NOT NULL");
                    table.CheckConstraint("CK_RiesgoEmpresa_SugeridoConOrigen", "\"Estado\" <> 'sugerido' OR (\"EncuestaId\" IS NOT NULL AND \"PeligroId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_RiesgoEmpresa_Area_EmpresaId_AreaId",
                        columns: x => new { x.EmpresaId, x.AreaId },
                        principalTable: "Area",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiesgoEmpresa_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiesgoEmpresa_Peligro_PeligroId",
                        column: x => x.PeligroId,
                        principalTable: "Peligro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiesgoEmpresa_VinculoEmpresa_EmpresaId_RevisadoPorId",
                        columns: x => new { x.EmpresaId, x.RevisadoPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Control",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiesgoEmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Control", x => x.Id);
                    table.UniqueConstraint("AK_Control_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.UniqueConstraint("AK_Control_RiesgoEmpresaId_Id", x => new { x.RiesgoEmpresaId, x.Id });
                    table.CheckConstraint("CK_Control_Estado", "\"Estado\" IN ('propuesto', 'implementado')");
                    table.CheckConstraint("CK_Control_Tipo", "\"Tipo\" IN ('eliminacion', 'sustitucion', 'ingenieria', 'administrativo', 'epp')");
                    table.ForeignKey(
                        name: "FK_Control_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Control_RiesgoEmpresa_EmpresaId_RiesgoEmpresaId",
                        columns: x => new { x.EmpresaId, x.RiesgoEmpresaId },
                        principalTable: "RiesgoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluacionRiesgo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiesgoEmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Probabilidad = table.Column<short>(type: "smallint", nullable: false),
                    Severidad = table.Column<short>(type: "smallint", nullable: false),
                    EvaluadoPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluacionRiesgo", x => x.Id);
                    table.UniqueConstraint("AK_EvaluacionRiesgo_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_EvaluacionRiesgo_Probabilidad", "\"Probabilidad\" BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_EvaluacionRiesgo_Severidad", "\"Severidad\" BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_EvaluacionRiesgo_Tipo", "\"Tipo\" IN ('inicial', 'residual')");
                    table.ForeignKey(
                        name: "FK_EvaluacionRiesgo_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluacionRiesgo_RiesgoEmpresa_EmpresaId_RiesgoEmpresaId",
                        columns: x => new { x.EmpresaId, x.RiesgoEmpresaId },
                        principalTable: "RiesgoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluacionRiesgo_VinculoEmpresa_EmpresaId_EvaluadoPorId",
                        columns: x => new { x.EmpresaId, x.EvaluadoPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Area_EmpresaId_EncargadoId",
                table: "Area",
                columns: new[] { "EmpresaId", "EncargadoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Area_EmpresaId_Nombre",
                table: "Area",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaRiesgo_RubroId_Nombre",
                table: "CategoriaRiesgo",
                columns: new[] { "RubroId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Control_EmpresaId_RiesgoEmpresaId",
                table: "Control",
                columns: new[] { "EmpresaId", "RiesgoEmpresaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_RTN",
                table: "Empresa",
                column: "RTN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_RubroId",
                table: "Empresa",
                column: "RubroId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionRiesgo_EmpresaId_EvaluadoPorId",
                table: "EvaluacionRiesgo",
                columns: new[] { "EmpresaId", "EvaluadoPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionRiesgo_EmpresaId_RiesgoEmpresaId",
                table: "EvaluacionRiesgo",
                columns: new[] { "EmpresaId", "RiesgoEmpresaId" });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionRiesgo_RiesgoEmpresaId_EvaluadoEn",
                table: "EvaluacionRiesgo",
                columns: new[] { "RiesgoEmpresaId", "EvaluadoEn" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoEmpresa_RegistradoPorId",
                table: "HistorialEstadoEmpresa",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoVinculo_EmpresaId_RegistradoPorId",
                table: "HistorialEstadoVinculo",
                columns: new[] { "EmpresaId", "RegistradoPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadoVinculo_EmpresaId_VinculoEmpresaId",
                table: "HistorialEstadoVinculo",
                columns: new[] { "EmpresaId", "VinculoEmpresaId" });

            migrationBuilder.CreateIndex(
                name: "IX_OpcionPregunta_PreguntaExploratoriaId_Orden",
                table: "OpcionPregunta",
                columns: new[] { "PreguntaExploratoriaId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Peligro_Codigo",
                table: "Peligro",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permiso_Codigo",
                table: "Permiso",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persona_TipoDocumento_NumeroDocumento",
                table: "Persona",
                columns: new[] { "TipoDocumento", "NumeroDocumento" },
                unique: true,
                filter: "\"TipoDocumento\" IS NOT NULL AND \"NumeroDocumento\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PreguntaExploratoria_CategoriaRiesgoId_Orden",
                table: "PreguntaExploratoria",
                columns: new[] { "CategoriaRiesgoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreguntaExploratoria_PeligroId",
                table: "PreguntaExploratoria",
                column: "PeligroId");

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_AreaId_Estado",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "AreaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_RevisadoPorId",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "RevisadoPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EncuestaId_PeligroId_AreaId",
                table: "RiesgoEmpresa",
                columns: new[] { "EncuestaId", "PeligroId", "AreaId" },
                unique: true,
                filter: "\"EncuestaId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_PeligroId",
                table: "RiesgoEmpresa",
                column: "PeligroId");

            migrationBuilder.CreateIndex(
                name: "IX_Rol_Codigo",
                table: "Rol",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolPermiso_PermisoId",
                table: "RolPermiso",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_RolStaffPlataforma_Codigo",
                table: "RolStaffPlataforma",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolStaffPlataformaPermiso_PermisoId",
                table: "RolStaffPlataformaPermiso",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_Rubro_Codigo",
                table: "Rubro",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rubro_Nombre",
                table: "Rubro",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlataforma_Email",
                table: "StaffPlataforma",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlataforma_RolStaffPlataformaId",
                table: "StaffPlataforma",
                column: "RolStaffPlataformaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_PersonaId",
                table: "Usuario",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VinculoEmpresa_Email",
                table: "VinculoEmpresa",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VinculoEmpresa_EmpresaId_PersonaId",
                table: "VinculoEmpresa",
                columns: new[] { "EmpresaId", "PersonaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VinculoEmpresa_PersonaId",
                table: "VinculoEmpresa",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculoEmpresa_RolId",
                table: "VinculoEmpresa",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Control");

            migrationBuilder.DropTable(
                name: "EvaluacionRiesgo");

            migrationBuilder.DropTable(
                name: "HistorialEstadoEmpresa");

            migrationBuilder.DropTable(
                name: "HistorialEstadoVinculo");

            migrationBuilder.DropTable(
                name: "OpcionPregunta");

            migrationBuilder.DropTable(
                name: "RolPermiso");

            migrationBuilder.DropTable(
                name: "RolStaffPlataformaPermiso");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "RiesgoEmpresa");

            migrationBuilder.DropTable(
                name: "StaffPlataforma");

            migrationBuilder.DropTable(
                name: "PreguntaExploratoria");

            migrationBuilder.DropTable(
                name: "Permiso");

            migrationBuilder.DropTable(
                name: "Area");

            migrationBuilder.DropTable(
                name: "RolStaffPlataforma");

            migrationBuilder.DropTable(
                name: "CategoriaRiesgo");

            migrationBuilder.DropTable(
                name: "Peligro");

            migrationBuilder.DropTable(
                name: "VinculoEmpresa");

            migrationBuilder.DropTable(
                name: "Empresa");

            migrationBuilder.DropTable(
                name: "Persona");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "Rubro");
        }
    }
}
