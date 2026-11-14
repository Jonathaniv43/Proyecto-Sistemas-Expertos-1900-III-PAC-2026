using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlcanceReducido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiesgoEmpresa_Area_EmpresaId_AreaId",
                table: "RiesgoEmpresa");

            migrationBuilder.DropTable(
                name: "Area");

            migrationBuilder.DropIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_AreaId_Estado",
                table: "RiesgoEmpresa");

            migrationBuilder.DropIndex(
                name: "IX_RiesgoEmpresa_EncuestaId_PeligroId_AreaId",
                table: "RiesgoEmpresa");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PreguntaExploratoria_TextoSinPeligro",
                table: "PreguntaExploratoria");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PreguntaExploratoria_TipoRespuesta",
                table: "PreguntaExploratoria");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "RiesgoEmpresa");

            migrationBuilder.AlterColumn<string>(
                name: "TipoRespuesta",
                table: "PreguntaExploratoria",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<bool>(
                name: "Obligatoria",
                table: "PreguntaExploratoria",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_Estado",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EncuestaId_PeligroId",
                table: "RiesgoEmpresa",
                columns: new[] { "EncuestaId", "PeligroId" },
                unique: true,
                filter: "\"EncuestaId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PreguntaExploratoria_RespuestaBreveSinPeligro",
                table: "PreguntaExploratoria",
                sql: "\"TipoRespuesta\" <> 'respuesta_breve' OR \"PeligroId\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PreguntaExploratoria_TipoRespuesta",
                table: "PreguntaExploratoria",
                sql: "\"TipoRespuesta\" IN ('respuesta_breve', 'seleccion_unica', 'seleccion_multiple')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_Estado",
                table: "RiesgoEmpresa");

            migrationBuilder.DropIndex(
                name: "IX_RiesgoEmpresa_EncuestaId_PeligroId",
                table: "RiesgoEmpresa");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PreguntaExploratoria_RespuestaBreveSinPeligro",
                table: "PreguntaExploratoria");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PreguntaExploratoria_TipoRespuesta",
                table: "PreguntaExploratoria");

            migrationBuilder.DropColumn(
                name: "Obligatoria",
                table: "PreguntaExploratoria");

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "RiesgoEmpresa",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "TipoRespuesta",
                table: "PreguntaExploratoria",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.CreateTable(
                name: "Area",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncargadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_AreaId_Estado",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "AreaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EncuestaId_PeligroId_AreaId",
                table: "RiesgoEmpresa",
                columns: new[] { "EncuestaId", "PeligroId", "AreaId" },
                unique: true,
                filter: "\"EncuestaId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PreguntaExploratoria_TextoSinPeligro",
                table: "PreguntaExploratoria",
                sql: "\"TipoRespuesta\" <> 'texto' OR \"PeligroId\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PreguntaExploratoria_TipoRespuesta",
                table: "PreguntaExploratoria",
                sql: "\"TipoRespuesta\" IN ('si_no', 'opcion_unica', 'texto')");

            migrationBuilder.CreateIndex(
                name: "IX_Area_EmpresaId_EncargadoId",
                table: "Area",
                columns: new[] { "EmpresaId", "EncargadoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Area_EmpresaId_Nombre",
                table: "Area",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RiesgoEmpresa_Area_EmpresaId_AreaId",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "AreaId" },
                principalTable: "Area",
                principalColumns: new[] { "EmpresaId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
