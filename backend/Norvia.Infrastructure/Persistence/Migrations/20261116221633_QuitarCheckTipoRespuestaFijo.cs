using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QuitarCheckTipoRespuestaFijo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PreguntaExploratoria_RespuestaBreveSinPeligro",
                table: "PreguntaExploratoria");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EncuestaPregunta_RespuestaBreveSinPeligro",
                table: "EncuestaPregunta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_PreguntaExploratoria_RespuestaBreveSinPeligro",
                table: "PreguntaExploratoria",
                sql: "\"TipoRespuestaPreguntaId\" <> '019b0000-0001-7000-8000-000000000001' OR \"PeligroId\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EncuestaPregunta_RespuestaBreveSinPeligro",
                table: "EncuestaPregunta",
                sql: "\"TipoRespuestaPreguntaId\" <> '019b0000-0001-7000-8000-000000000001' OR \"PeligroId\" IS NULL");
        }
    }
}
