using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PendienteModeloCorreoYPersona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoDocumentoEspecificacion",
                table: "Persona",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlantillaCorreo",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Asunto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CuerpoHtml = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillaCorreo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaCorreo_Codigo",
                table: "PlantillaCorreo",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantillaCorreo");

            migrationBuilder.DropColumn(
                name: "TipoDocumentoEspecificacion",
                table: "Persona");
        }
    }
}
