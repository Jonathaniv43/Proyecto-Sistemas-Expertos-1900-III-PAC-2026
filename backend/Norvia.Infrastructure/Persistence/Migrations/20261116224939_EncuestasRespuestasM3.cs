using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EncuestasRespuestasM3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParticipacionEncuesta",
                columns: table => new
                {
                    EncuestaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VinculoEmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipacionEncuesta", x => new { x.EncuestaId, x.VinculoEmpresaId });
                    table.ForeignKey(
                        name: "FK_ParticipacionEncuesta_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipacionEncuesta_Encuesta_EmpresaId_EncuestaId",
                        columns: x => new { x.EmpresaId, x.EncuestaId },
                        principalTable: "Encuesta",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipacionEncuesta_VinculoEmpresa_EmpresaId_VinculoEmpre~",
                        columns: x => new { x.EmpresaId, x.VinculoEmpresaId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespuestaEncuesta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaRespuesta = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestaEncuesta", x => x.Id);
                    table.UniqueConstraint("AK_RespuestaEncuesta_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.UniqueConstraint("AK_RespuestaEncuesta_EncuestaVersionId_Id", x => new { x.EncuestaVersionId, x.Id });
                    table.ForeignKey(
                        name: "FK_RespuestaEncuesta_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespuestaEncuesta_EncuestaVersion_EmpresaId_EncuestaVersion~",
                        columns: x => new { x.EmpresaId, x.EncuestaVersionId },
                        principalTable: "EncuestaVersion",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleRespuesta",
                columns: table => new
                {
                    RespuestaEncuestaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaPreguntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaOpcionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TextoLibre = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleRespuesta", x => new { x.RespuestaEncuestaId, x.EncuestaPreguntaId });
                    table.UniqueConstraint("AK_DetalleRespuesta_EmpresaId_RespuestaEncuestaId_EncuestaPreg~", x => new { x.EmpresaId, x.RespuestaEncuestaId, x.EncuestaPreguntaId });
                    table.CheckConstraint("CK_DetalleRespuesta_ValorUnico", "(\"EncuestaOpcionId\" IS NULL OR \"TextoLibre\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_DetalleRespuesta_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleRespuesta_EncuestaOpcion_EncuestaPreguntaId_Encuesta~",
                        columns: x => new { x.EncuestaPreguntaId, x.EncuestaOpcionId },
                        principalTable: "EncuestaOpcion",
                        principalColumns: new[] { "EncuestaPreguntaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleRespuesta_EncuestaPregunta_EmpresaId_EncuestaPregunt~",
                        columns: x => new { x.EmpresaId, x.EncuestaPreguntaId },
                        principalTable: "EncuestaPregunta",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleRespuesta_RespuestaEncuesta_EmpresaId_RespuestaEncue~",
                        columns: x => new { x.EmpresaId, x.RespuestaEncuestaId },
                        principalTable: "RespuestaEncuesta",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleRespuestaOpcion",
                columns: table => new
                {
                    RespuestaEncuestaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaPreguntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncuestaOpcionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleRespuestaOpcion", x => new { x.RespuestaEncuestaId, x.EncuestaPreguntaId, x.EncuestaOpcionId });
                    table.ForeignKey(
                        name: "FK_DetalleRespuestaOpcion_DetalleRespuesta_EmpresaId_Respuesta~",
                        columns: x => new { x.EmpresaId, x.RespuestaEncuestaId, x.EncuestaPreguntaId },
                        principalTable: "DetalleRespuesta",
                        principalColumns: new[] { "EmpresaId", "RespuestaEncuestaId", "EncuestaPreguntaId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleRespuestaOpcion_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleRespuestaOpcion_EncuestaOpcion_EncuestaPreguntaId_En~",
                        columns: x => new { x.EncuestaPreguntaId, x.EncuestaOpcionId },
                        principalTable: "EncuestaOpcion",
                        principalColumns: new[] { "EncuestaPreguntaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRespuesta_EmpresaId_EncuestaPreguntaId",
                table: "DetalleRespuesta",
                columns: new[] { "EmpresaId", "EncuestaPreguntaId" });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRespuesta_EncuestaPreguntaId_EncuestaOpcionId",
                table: "DetalleRespuesta",
                columns: new[] { "EncuestaPreguntaId", "EncuestaOpcionId" });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRespuestaOpcion_EmpresaId_RespuestaEncuestaId_Encues~",
                table: "DetalleRespuestaOpcion",
                columns: new[] { "EmpresaId", "RespuestaEncuestaId", "EncuestaPreguntaId" });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRespuestaOpcion_EncuestaPreguntaId_EncuestaOpcionId",
                table: "DetalleRespuestaOpcion",
                columns: new[] { "EncuestaPreguntaId", "EncuestaOpcionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacionEncuesta_EmpresaId_EncuestaId",
                table: "ParticipacionEncuesta",
                columns: new[] { "EmpresaId", "EncuestaId" });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacionEncuesta_EmpresaId_VinculoEmpresaId",
                table: "ParticipacionEncuesta",
                columns: new[] { "EmpresaId", "VinculoEmpresaId" });

            migrationBuilder.CreateIndex(
                name: "IX_RespuestaEncuesta_EmpresaId_EncuestaVersionId",
                table: "RespuestaEncuesta",
                columns: new[] { "EmpresaId", "EncuestaVersionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleRespuestaOpcion");

            migrationBuilder.DropTable(
                name: "ParticipacionEncuesta");

            migrationBuilder.DropTable(
                name: "DetalleRespuesta");

            migrationBuilder.DropTable(
                name: "RespuestaEncuesta");
        }
    }
}
