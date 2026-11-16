using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TipoRespuestaYSeccionesEncuesta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PreguntaExploratoria_RespuestaBreveSinPeligro",
                table: "PreguntaExploratoria");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PreguntaExploratoria_TipoRespuesta",
                table: "PreguntaExploratoria");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EncuestaPregunta_RespuestaBreveSinPeligro",
                table: "EncuestaPregunta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EncuestaPregunta_TipoRespuesta",
                table: "EncuestaPregunta");

            migrationBuilder.DropColumn(
                name: "TipoRespuesta",
                table: "PreguntaExploratoria");

            migrationBuilder.DropColumn(
                name: "TipoRespuesta",
                table: "EncuestaPregunta");

            migrationBuilder.AddColumn<Guid>(
                name: "TipoRespuestaPreguntaId",
                table: "PreguntaExploratoria",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaRiesgoId",
                table: "EncuestaPregunta",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EncuestaSeccionId",
                table: "EncuestaPregunta",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipoRespuestaPreguntaId",
                table: "EncuestaPregunta",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "EncuestaSeccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Orden = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestaSeccion", x => x.Id);
                    table.UniqueConstraint("AK_EncuestaSeccion_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.UniqueConstraint("AK_EncuestaSeccion_EncuestaVersionId_Id", x => new { x.EncuestaVersionId, x.Id });
                    table.ForeignKey(
                        name: "FK_EncuestaSeccion_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncuestaSeccion_EncuestaVersion_EmpresaId_EncuestaVersionId",
                        columns: x => new { x.EmpresaId, x.EncuestaVersionId },
                        principalTable: "EncuestaVersion",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TipoRespuestaPregunta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    InstruccionUi = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RequiereOpciones = table.Column<bool>(type: "boolean", nullable: false),
                    MaxCaracteresRespuesta = table.Column<short>(type: "smallint", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoRespuestaPregunta", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreguntaExploratoria_TipoRespuestaPreguntaId",
                table: "PreguntaExploratoria",
                column: "TipoRespuestaPreguntaId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PreguntaExploratoria_RespuestaBreveSinPeligro",
                table: "PreguntaExploratoria",
                sql: "\"TipoRespuestaPreguntaId\" <> '019b0000-0001-7000-8000-000000000001' OR \"PeligroId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaPregunta_CategoriaRiesgoId",
                table: "EncuestaPregunta",
                column: "CategoriaRiesgoId");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaPregunta_EncuestaSeccionId",
                table: "EncuestaPregunta",
                column: "EncuestaSeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaPregunta_TipoRespuestaPreguntaId",
                table: "EncuestaPregunta",
                column: "TipoRespuestaPreguntaId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EncuestaPregunta_RespuestaBreveSinPeligro",
                table: "EncuestaPregunta",
                sql: "\"TipoRespuestaPreguntaId\" <> '019b0000-0001-7000-8000-000000000001' OR \"PeligroId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaSeccion_EmpresaId_EncuestaVersionId",
                table: "EncuestaSeccion",
                columns: new[] { "EmpresaId", "EncuestaVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestaSeccion_EncuestaVersionId_Orden",
                table: "EncuestaSeccion",
                columns: new[] { "EncuestaVersionId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoRespuestaPregunta_Codigo",
                table: "TipoRespuestaPregunta",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EncuestaPregunta_CategoriaRiesgo_CategoriaRiesgoId",
                table: "EncuestaPregunta",
                column: "CategoriaRiesgoId",
                principalTable: "CategoriaRiesgo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EncuestaPregunta_EncuestaSeccion_EncuestaSeccionId",
                table: "EncuestaPregunta",
                column: "EncuestaSeccionId",
                principalTable: "EncuestaSeccion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EncuestaPregunta_TipoRespuestaPregunta_TipoRespuestaPregunt~",
                table: "EncuestaPregunta",
                column: "TipoRespuestaPreguntaId",
                principalTable: "TipoRespuestaPregunta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreguntaExploratoria_TipoRespuestaPregunta_TipoRespuestaPre~",
                table: "PreguntaExploratoria",
                column: "TipoRespuestaPreguntaId",
                principalTable: "TipoRespuestaPregunta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EncuestaPregunta_CategoriaRiesgo_CategoriaRiesgoId",
                table: "EncuestaPregunta");

            migrationBuilder.DropForeignKey(
                name: "FK_EncuestaPregunta_EncuestaSeccion_EncuestaSeccionId",
                table: "EncuestaPregunta");

            migrationBuilder.DropForeignKey(
                name: "FK_EncuestaPregunta_TipoRespuestaPregunta_TipoRespuestaPregunt~",
                table: "EncuestaPregunta");

            migrationBuilder.DropForeignKey(
                name: "FK_PreguntaExploratoria_TipoRespuestaPregunta_TipoRespuestaPre~",
                table: "PreguntaExploratoria");

            migrationBuilder.DropTable(
                name: "EncuestaSeccion");

            migrationBuilder.DropTable(
                name: "TipoRespuestaPregunta");

            migrationBuilder.DropIndex(
                name: "IX_PreguntaExploratoria_TipoRespuestaPreguntaId",
                table: "PreguntaExploratoria");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PreguntaExploratoria_RespuestaBreveSinPeligro",
                table: "PreguntaExploratoria");

            migrationBuilder.DropIndex(
                name: "IX_EncuestaPregunta_CategoriaRiesgoId",
                table: "EncuestaPregunta");

            migrationBuilder.DropIndex(
                name: "IX_EncuestaPregunta_EncuestaSeccionId",
                table: "EncuestaPregunta");

            migrationBuilder.DropIndex(
                name: "IX_EncuestaPregunta_TipoRespuestaPreguntaId",
                table: "EncuestaPregunta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EncuestaPregunta_RespuestaBreveSinPeligro",
                table: "EncuestaPregunta");

            migrationBuilder.DropColumn(
                name: "TipoRespuestaPreguntaId",
                table: "PreguntaExploratoria");

            migrationBuilder.DropColumn(
                name: "CategoriaRiesgoId",
                table: "EncuestaPregunta");

            migrationBuilder.DropColumn(
                name: "EncuestaSeccionId",
                table: "EncuestaPregunta");

            migrationBuilder.DropColumn(
                name: "TipoRespuestaPreguntaId",
                table: "EncuestaPregunta");

            migrationBuilder.AddColumn<string>(
                name: "TipoRespuesta",
                table: "PreguntaExploratoria",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoRespuesta",
                table: "EncuestaPregunta",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PreguntaExploratoria_RespuestaBreveSinPeligro",
                table: "PreguntaExploratoria",
                sql: "\"TipoRespuesta\" <> 'respuesta_breve' OR \"PeligroId\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PreguntaExploratoria_TipoRespuesta",
                table: "PreguntaExploratoria",
                sql: "\"TipoRespuesta\" IN ('respuesta_breve', 'seleccion_unica', 'seleccion_multiple')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EncuestaPregunta_RespuestaBreveSinPeligro",
                table: "EncuestaPregunta",
                sql: "\"TipoRespuesta\" <> 'respuesta_breve' OR \"PeligroId\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EncuestaPregunta_TipoRespuesta",
                table: "EncuestaPregunta",
                sql: "\"TipoRespuesta\" IN ('respuesta_breve', 'seleccion_unica', 'seleccion_multiple')");
        }
    }
}
