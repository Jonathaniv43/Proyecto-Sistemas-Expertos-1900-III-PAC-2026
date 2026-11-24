using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinatarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: false),
                    VersionDocumentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    TareaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    LeidaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorreoEstado = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CorreoIntentos = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CorreoProximoIntentoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorreoEnviadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorreoUltimoError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacion", x => x.Id);
                    table.UniqueConstraint("AK_Notificacion_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_Notificacion_CorreoEnviadoEn", "(\"CorreoEnviadoEn\" IS NOT NULL) = (\"CorreoEstado\" = 'enviado')");
                    table.CheckConstraint("CK_Notificacion_CorreoEstado", "\"CorreoEstado\" IN ('pendiente', 'enviado', 'fallido')");
                    table.CheckConstraint("CK_Notificacion_CorreoIntentos", "\"CorreoIntentos\" >= 0");
                    table.CheckConstraint("CK_Notificacion_Origen", "(\"Tipo\" = 'politica_publicada' AND \"VersionDocumentoId\" IS NOT NULL AND \"TareaId\" IS NULL) OR (\"Tipo\" = 'tarea_asignada' AND \"TareaId\" IS NOT NULL AND \"VersionDocumentoId\" IS NULL)");
                    table.CheckConstraint("CK_Notificacion_Tipo", "\"Tipo\" IN ('politica_publicada', 'tarea_asignada')");
                    table.ForeignKey(
                        name: "FK_Notificacion_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notificacion_Tarea_EmpresaId_TareaId",
                        columns: x => new { x.EmpresaId, x.TareaId },
                        principalTable: "Tarea",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notificacion_VinculoEmpresa_EmpresaId_DestinatarioId",
                        columns: x => new { x.EmpresaId, x.DestinatarioId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_CorreoProximoIntentoEn",
                table: "Notificacion",
                column: "CorreoProximoIntentoEn",
                filter: "\"CorreoEstado\" = 'pendiente'");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_EmpresaId_DestinatarioId_LeidaEn",
                table: "Notificacion",
                columns: new[] { "EmpresaId", "DestinatarioId", "LeidaEn" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_EmpresaId_TareaId",
                table: "Notificacion",
                columns: new[] { "EmpresaId", "TareaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_VersionDocumentoId_DestinatarioId",
                table: "Notificacion",
                columns: new[] { "VersionDocumentoId", "DestinatarioId" },
                unique: true,
                filter: "\"VersionDocumentoId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificacion");
        }
    }
}
