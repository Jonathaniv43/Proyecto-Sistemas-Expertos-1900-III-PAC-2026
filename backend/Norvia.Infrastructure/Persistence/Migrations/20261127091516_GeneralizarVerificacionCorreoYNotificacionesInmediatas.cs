using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GeneralizarVerificacionCorreoYNotificacionesInmediatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerificacionCorreoMicrosoft");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Notificacion_CorreoEstado",
                table: "Notificacion");

            migrationBuilder.DropColumn(
                name: "DebeCambiarPassword",
                table: "Acceso");

            migrationBuilder.CreateTable(
                name: "VerificacionCorreo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Proposito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    ProveedorUserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccesoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodigoHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Intentos = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ExpiraEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnviosEnVentana = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    VentanaEnviosExpiraEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificacionCorreo", x => x.Id);
                    table.CheckConstraint("CK_VerificacionCorreo_Proposito", "(\"Proposito\" = 'microsoft' AND \"ProveedorUserId\" IS NOT NULL AND \"AccesoId\" IS NULL) OR (\"Proposito\" = 'activacion_cuenta' AND \"AccesoId\" IS NOT NULL AND \"ProveedorUserId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_VerificacionCorreo_Acceso_AccesoId",
                        column: x => x.AccesoId,
                        principalTable: "Acceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notificacion_CorreoEstado",
                table: "Notificacion",
                sql: "\"CorreoEstado\" IN ('pendiente', 'enviado', 'fallido', 'en_proceso')");

            migrationBuilder.CreateIndex(
                name: "IX_VerificacionCorreo_AccesoId",
                table: "VerificacionCorreo",
                column: "AccesoId",
                unique: true,
                filter: "\"AccesoId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VerificacionCorreo_Email_ProveedorUserId",
                table: "VerificacionCorreo",
                columns: new[] { "Email", "ProveedorUserId" },
                unique: true,
                filter: "\"ProveedorUserId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerificacionCorreo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Notificacion_CorreoEstado",
                table: "Notificacion");

            migrationBuilder.AddColumn<bool>(
                name: "DebeCambiarPassword",
                table: "Acceso",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VerificacionCorreoMicrosoft",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    EnviosEnVentana = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ExpiraEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Intentos = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ProveedorUserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    VentanaEnviosExpiraEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificacionCorreoMicrosoft", x => x.Id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notificacion_CorreoEstado",
                table: "Notificacion",
                sql: "\"CorreoEstado\" IN ('pendiente', 'enviado', 'fallido')");

            migrationBuilder.CreateIndex(
                name: "IX_VerificacionCorreoMicrosoft_Email_ProveedorUserId",
                table: "VerificacionCorreoMicrosoft",
                columns: new[] { "Email", "ProveedorUserId" },
                unique: true);
        }
    }
}
