using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RiesgoEventoHu051 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccidenteId",
                table: "RiesgoEmpresa",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccidenteId",
                table: "EvaluacionRiesgo",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_AccidenteId",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "AccidenteId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RiesgoEmpresa_UnSoloOrigen",
                table: "RiesgoEmpresa",
                sql: "\"EncuestaId\" IS NULL OR \"AccidenteId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionRiesgo_EmpresaId_AccidenteId",
                table: "EvaluacionRiesgo",
                columns: new[] { "EmpresaId", "AccidenteId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_EvaluacionRiesgo_AccidenteSoloInicial",
                table: "EvaluacionRiesgo",
                sql: "\"AccidenteId\" IS NULL OR \"Tipo\" = 'inicial'");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluacionRiesgo_Accidente_EmpresaId_AccidenteId",
                table: "EvaluacionRiesgo",
                columns: new[] { "EmpresaId", "AccidenteId" },
                principalTable: "Accidente",
                principalColumns: new[] { "EmpresaId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RiesgoEmpresa_Accidente_EmpresaId_AccidenteId",
                table: "RiesgoEmpresa",
                columns: new[] { "EmpresaId", "AccidenteId" },
                principalTable: "Accidente",
                principalColumns: new[] { "EmpresaId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluacionRiesgo_Accidente_EmpresaId_AccidenteId",
                table: "EvaluacionRiesgo");

            migrationBuilder.DropForeignKey(
                name: "FK_RiesgoEmpresa_Accidente_EmpresaId_AccidenteId",
                table: "RiesgoEmpresa");

            migrationBuilder.DropIndex(
                name: "IX_RiesgoEmpresa_EmpresaId_AccidenteId",
                table: "RiesgoEmpresa");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RiesgoEmpresa_UnSoloOrigen",
                table: "RiesgoEmpresa");

            migrationBuilder.DropIndex(
                name: "IX_EvaluacionRiesgo_EmpresaId_AccidenteId",
                table: "EvaluacionRiesgo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EvaluacionRiesgo_AccidenteSoloInicial",
                table: "EvaluacionRiesgo");

            migrationBuilder.DropColumn(
                name: "AccidenteId",
                table: "RiesgoEmpresa");

            migrationBuilder.DropColumn(
                name: "AccidenteId",
                table: "EvaluacionRiesgo");
        }
    }
}
