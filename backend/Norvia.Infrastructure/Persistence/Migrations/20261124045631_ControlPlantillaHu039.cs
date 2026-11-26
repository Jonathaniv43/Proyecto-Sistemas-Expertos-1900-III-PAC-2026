using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ControlPlantillaHu039 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlantillaTareaId",
                table: "Control",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Control_PlantillaTareaId",
                table: "Control",
                column: "PlantillaTareaId");

            migrationBuilder.CreateIndex(
                name: "IX_Control_RiesgoEmpresaId_PlantillaTareaId",
                table: "Control",
                columns: new[] { "RiesgoEmpresaId", "PlantillaTareaId" },
                unique: true,
                filter: "\"PlantillaTareaId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Control_PlantillaTarea_PlantillaTareaId",
                table: "Control",
                column: "PlantillaTareaId",
                principalTable: "PlantillaTarea",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Control_PlantillaTarea_PlantillaTareaId",
                table: "Control");

            migrationBuilder.DropIndex(
                name: "IX_Control_PlantillaTareaId",
                table: "Control");

            migrationBuilder.DropIndex(
                name: "IX_Control_RiesgoEmpresaId_PlantillaTareaId",
                table: "Control");

            migrationBuilder.DropColumn(
                name: "PlantillaTareaId",
                table: "Control");
        }
    }
}
