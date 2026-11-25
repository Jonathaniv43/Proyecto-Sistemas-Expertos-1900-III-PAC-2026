using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VerificacionCorreoMicrosoft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VerificacionCorreoMicrosoft",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    ProveedorUserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CodigoHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Intentos = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ExpiraEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnviosEnVentana = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    VentanaEnviosExpiraEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificacionCorreoMicrosoft", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerificacionCorreoMicrosoft_Email_ProveedorUserId",
                table: "VerificacionCorreoMicrosoft",
                columns: new[] { "Email", "ProveedorUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerificacionCorreoMicrosoft");
        }
    }
}
