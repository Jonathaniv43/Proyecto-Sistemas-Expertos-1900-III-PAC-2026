using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TareaPlanHu042 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tarea",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Prioridad = table.Column<short>(type: "smallint", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaLimite = table.Column<DateOnly>(type: "date", nullable: true),
                    RequiereEvidencia = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PlantillaTareaId = table.Column<Guid>(type: "uuid", nullable: true),
                    RiesgoEmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ControlId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccidenteId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreadaPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CompletadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarea", x => x.Id);
                    table.UniqueConstraint("AK_Tarea_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_Tarea_CompletadaEn", "(\"CompletadaEn\" IS NOT NULL) = (\"Estado\" = 'completada')");
                    table.CheckConstraint("CK_Tarea_ControlConRiesgo", "\"ControlId\" IS NULL OR \"RiesgoEmpresaId\" IS NOT NULL");
                    table.CheckConstraint("CK_Tarea_Estado", "\"Estado\" IN ('pendiente', 'en_progreso', 'completada', 'cancelada')");
                    table.CheckConstraint("CK_Tarea_IniciadaConResponsable", "\"Estado\" NOT IN ('en_progreso', 'completada') OR (\"ResponsableId\" IS NOT NULL AND \"FechaLimite\" IS NOT NULL)");
                    table.CheckConstraint("CK_Tarea_Prioridad", "\"Prioridad\" BETWEEN 1 AND 3");
                    table.CheckConstraint("CK_Tarea_UnOrigen", "\"RiesgoEmpresaId\" IS NULL OR \"AccidenteId\" IS NULL");
                    table.ForeignKey(
                        name: "FK_Tarea_Accidente_EmpresaId_AccidenteId",
                        columns: x => new { x.EmpresaId, x.AccidenteId },
                        principalTable: "Accidente",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarea_Control_RiesgoEmpresaId_ControlId",
                        columns: x => new { x.RiesgoEmpresaId, x.ControlId },
                        principalTable: "Control",
                        principalColumns: new[] { "RiesgoEmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarea_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarea_PlantillaTarea_PlantillaTareaId",
                        column: x => x.PlantillaTareaId,
                        principalTable: "PlantillaTarea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarea_RiesgoEmpresa_EmpresaId_RiesgoEmpresaId",
                        columns: x => new { x.EmpresaId, x.RiesgoEmpresaId },
                        principalTable: "RiesgoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarea_VinculoEmpresa_EmpresaId_CreadaPorId",
                        columns: x => new { x.EmpresaId, x.CreadaPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tarea_VinculoEmpresa_EmpresaId_ResponsableId",
                        columns: x => new { x.EmpresaId, x.ResponsableId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_ControlId",
                table: "Tarea",
                column: "ControlId",
                unique: true,
                filter: "\"ControlId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_EmpresaId_AccidenteId",
                table: "Tarea",
                columns: new[] { "EmpresaId", "AccidenteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_EmpresaId_CreadaPorId",
                table: "Tarea",
                columns: new[] { "EmpresaId", "CreadaPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_EmpresaId_Estado_Prioridad",
                table: "Tarea",
                columns: new[] { "EmpresaId", "Estado", "Prioridad" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_EmpresaId_ResponsableId",
                table: "Tarea",
                columns: new[] { "EmpresaId", "ResponsableId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_EmpresaId_RiesgoEmpresaId",
                table: "Tarea",
                columns: new[] { "EmpresaId", "RiesgoEmpresaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_PlantillaTareaId",
                table: "Tarea",
                column: "PlantillaTareaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_RiesgoEmpresaId_ControlId",
                table: "Tarea",
                columns: new[] { "RiesgoEmpresaId", "ControlId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarea_RiesgoEmpresaId_PlantillaTareaId",
                table: "Tarea",
                columns: new[] { "RiesgoEmpresaId", "PlantillaTareaId" },
                unique: true,
                filter: "\"RiesgoEmpresaId\" IS NOT NULL AND \"PlantillaTareaId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tarea");
        }
    }
}
