using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlantillasCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantillaDocumento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoDocumento = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    RubroId = table.Column<short>(type: "smallint", nullable: true),
                    Contenido = table.Column<string>(type: "text", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillaDocumento", x => x.Id);
                    table.CheckConstraint("CK_PlantillaDocumento_TipoDocumento", "\"TipoDocumento\" IN ('politica', 'procedimiento')");
                    table.ForeignKey(
                        name: "FK_PlantillaDocumento_Rubro_RubroId",
                        column: x => x.RubroId,
                        principalTable: "Rubro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantillaTarea",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeligroId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoControl = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    PrioridadSugerida = table.Column<short>(type: "smallint", nullable: false),
                    RequiereEvidencia = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillaTarea", x => x.Id);
                    table.CheckConstraint("CK_PlantillaTarea_PrioridadSugerida", "\"PrioridadSugerida\" BETWEEN 1 AND 3");
                    table.CheckConstraint("CK_PlantillaTarea_TipoControl", "\"TipoControl\" IN ('eliminacion', 'sustitucion', 'ingenieria', 'administrativo', 'epp')");
                    table.ForeignKey(
                        name: "FK_PlantillaTarea_Peligro_PeligroId",
                        column: x => x.PeligroId,
                        principalTable: "Peligro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaDocumento_Codigo",
                table: "PlantillaDocumento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaDocumento_RubroId",
                table: "PlantillaDocumento",
                column: "RubroId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaTarea_PeligroId",
                table: "PlantillaTarea",
                column: "PeligroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantillaDocumento");

            migrationBuilder.DropTable(
                name: "PlantillaTarea");
        }
    }
}
