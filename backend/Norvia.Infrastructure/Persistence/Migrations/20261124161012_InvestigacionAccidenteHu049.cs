using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InvestigacionAccidenteHu049 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestigacionAccidente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccidenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausasInmediatas = table.Column<string>(type: "text", nullable: false),
                    CausasBasicas = table.Column<string>(type: "text", nullable: false),
                    Conclusiones = table.Column<string>(type: "text", nullable: true),
                    InvestigadaPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaInvestigacion = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigacionAccidente", x => x.Id);
                    table.UniqueConstraint("AK_InvestigacionAccidente_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.ForeignKey(
                        name: "FK_InvestigacionAccidente_Accidente_EmpresaId_AccidenteId",
                        columns: x => new { x.EmpresaId, x.AccidenteId },
                        principalTable: "Accidente",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestigacionAccidente_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvestigacionAccidente_VinculoEmpresa_EmpresaId_Investigada~",
                        columns: x => new { x.EmpresaId, x.InvestigadaPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestigacionAccidente_EmpresaId_AccidenteId",
                table: "InvestigacionAccidente",
                columns: new[] { "EmpresaId", "AccidenteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestigacionAccidente_EmpresaId_InvestigadaPorId",
                table: "InvestigacionAccidente",
                columns: new[] { "EmpresaId", "InvestigadaPorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestigacionAccidente");
        }
    }
}
