using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EncuestasM3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Encuesta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreadaPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CerradaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encuesta", x => x.Id);
                    table.UniqueConstraint("AK_Encuesta_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_Encuesta_CerradaEn", "(\"CerradaEn\" IS NOT NULL) = (\"Estado\" = 'cerrada')");
                    table.CheckConstraint("CK_Encuesta_Estado", "\"Estado\" IN ('borrador', 'publicada', 'cerrada')");
                    table.ForeignKey(
                        name: "FK_Encuesta_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Encuesta_VinculoEmpresa_EmpresaId_CreadaPorId",
                        columns: x => new { x.EmpresaId, x.CreadaPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EncuestaVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroVersion = table.Column<int>(type: "integer", nullable: false),
                    Instrucciones = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CreadaPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    PublicadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestaVersion", x => x.Id);
                    table.UniqueConstraint("AK_EncuestaVersion_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_EncuestaVersion_Estado", "\"Estado\" IN ('borrador', 'publicada', 'reemplazada')");
                    table.CheckConstraint("CK_EncuestaVersion_NumeroVersion", "\"NumeroVersion\" >= 1");
                    table.CheckConstraint("CK_EncuestaVersion_PublicadaEn", "\"Estado\" = 'borrador' OR \"PublicadaEn\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_EncuestaVersion_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaVersion_Encuesta_EmpresaId_EncuestaId",
                        columns: x => new { x.EmpresaId, x.EncuestaId },
                        principalTable: "Encuesta",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaVersion_VinculoEmpresa_EmpresaId_CreadaPorId",
                        columns: x => new { x.EmpresaId, x.CreadaPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EncuestaCategoria",
                columns: table => new
                {
                    EncuestaVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoriaRiesgoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestaCategoria", x => new { x.EncuestaVersionId, x.CategoriaRiesgoId });
                    table.ForeignKey(
                        name: "FK_EncuestaCategoria_CategoriaRiesgo_CategoriaRiesgoId",
                        column: x => x.CategoriaRiesgoId,
                        principalTable: "CategoriaRiesgo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaCategoria_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaCategoria_EncuestaVersion_EmpresaId_EncuestaVersion~",
                        columns: x => new { x.EmpresaId, x.EncuestaVersionId },
                        principalTable: "EncuestaVersion",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EncuestaPregunta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreguntaExploratoriaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Texto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoRespuesta = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    Obligatoria = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PeligroId = table.Column<Guid>(type: "uuid", nullable: true),
                    Orden = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestaPregunta", x => x.Id);
                    table.UniqueConstraint("AK_EncuestaPregunta_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.UniqueConstraint("AK_EncuestaPregunta_EncuestaVersionId_Id", x => new { x.EncuestaVersionId, x.Id });
                    table.CheckConstraint("CK_EncuestaPregunta_RespuestaBreveSinPeligro", "\"TipoRespuesta\" <> 'respuesta_breve' OR \"PeligroId\" IS NULL");
                    table.CheckConstraint("CK_EncuestaPregunta_TipoRespuesta", "\"TipoRespuesta\" IN ('respuesta_breve', 'seleccion_unica', 'seleccion_multiple')");
                    table.ForeignKey(
                        name: "FK_EncuestaPregunta_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaPregunta_EncuestaVersion_EmpresaId_EncuestaVersionId",
                        columns: x => new { x.EmpresaId, x.EncuestaVersionId },
                        principalTable: "EncuestaVersion",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaPregunta_Peligro_PeligroId",
                        column: x => x.PeligroId,
                        principalTable: "Peligro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaPregunta_PreguntaExploratoria_PreguntaExploratoriaId",
                        column: x => x.PreguntaExploratoriaId,
                        principalTable: "PreguntaExploratoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EncuestaOpcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaPreguntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Orden = table.Column<short>(type: "smallint", nullable: false),
                    IndicaExposicion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestaOpcion", x => x.Id);
                    table.UniqueConstraint("AK_EncuestaOpcion_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.UniqueConstraint("AK_EncuestaOpcion_EncuestaPreguntaId_Id", x => new { x.EncuestaPreguntaId, x.Id });
                    table.ForeignKey(
                        name: "FK_EncuestaOpcion_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaOpcion_EncuestaPregunta_EmpresaId_EncuestaPreguntaId",
                        columns: x => new { x.EmpresaId, x.EncuestaPreguntaId },
                        principalTable: "EncuestaPregunta",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_EncuestaId",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "EncuestaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Encuesta_EmpresaId_CreadaPorId",
                table: "Encuesta",
                columns: new[] { "EmpresaId", "CreadaPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Encuesta_EmpresaId_Estado",
                table: "Encuesta",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaCategoria_CategoriaRiesgoId",
                table: "EncuestaCategoria",
                column: "CategoriaRiesgoId");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaCategoria_EmpresaId_EncuestaVersionId",
                table: "EncuestaCategoria",
                columns: new[] { "EmpresaId", "EncuestaVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaOpcion_EmpresaId_EncuestaPreguntaId",
                table: "EncuestaOpcion",
                columns: new[] { "EmpresaId", "EncuestaPreguntaId" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaOpcion_EncuestaPreguntaId_Orden",
                table: "EncuestaOpcion",
                columns: new[] { "EncuestaPreguntaId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaPregunta_EmpresaId_EncuestaVersionId",
                table: "EncuestaPregunta",
                columns: new[] { "EmpresaId", "EncuestaVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaPregunta_EncuestaVersionId_Orden",
                table: "EncuestaPregunta",
                columns: new[] { "EncuestaVersionId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaPregunta_PeligroId",
                table: "EncuestaPregunta",
                column: "PeligroId");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaPregunta_PreguntaExploratoriaId",
                table: "EncuestaPregunta",
                column: "PreguntaExploratoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaVersion_EmpresaId_CreadaPorId",
                table: "EncuestaVersion",
                columns: new[] { "EmpresaId", "CreadaPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaVersion_EmpresaId_EncuestaId",
                table: "EncuestaVersion",
                columns: new[] { "EmpresaId", "EncuestaId" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaVersion_EncuestaId_Borrador",
                table: "EncuestaVersion",
                column: "EncuestaId",
                unique: true,
                filter: "\"Estado\" = 'borrador'");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaVersion_EncuestaId_NumeroVersion",
                table: "EncuestaVersion",
                columns: new[] { "EncuestaId", "NumeroVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaVersion_EncuestaId_Publicada",
                table: "EncuestaVersion",
                column: "EncuestaId",
                unique: true,
                filter: "\"Estado\" = 'publicada'");

            migrationBuilder.AddForeignKey(
                name: "FK_RiesgoEmpresa_Encuesta_EmpresaId_EncuestaId",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "EncuestaId" },
                principalTable: "Encuesta",
                principalColumns: new[] { "EmpresaId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiesgoEmpresa_Encuesta_EmpresaId_EncuestaId",
                table: "RiesgoEmpresa");

            migrationBuilder.DropTable(
                name: "EncuestaCategoria");

            migrationBuilder.DropTable(
                name: "EncuestaOpcion");

            migrationBuilder.DropTable(
                name: "EncuestaPregunta");

            migrationBuilder.DropTable(
                name: "EncuestaVersion");

            migrationBuilder.DropTable(
                name: "Encuesta");

            migrationBuilder.DropIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_EncuestaId",
                table: "RiesgoEmpresa");
        }
    }
}
