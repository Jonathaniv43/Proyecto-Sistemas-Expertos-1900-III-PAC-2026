using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccesoProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Acceso",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateTable(
                name: "AccesoProveedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccesoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Proveedor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProveedorUserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccesoProveedor", x => x.Id);
                    table.CheckConstraint("CK_AccesoProveedor_Proveedor", "\"Proveedor\" IN ('microsoft')");
                    table.ForeignKey(
                        name: "FK_AccesoProveedor_Acceso_AccesoId",
                        column: x => x.AccesoId,
                        principalTable: "Acceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccesoProveedor_AccesoId_Proveedor",
                table: "AccesoProveedor",
                columns: new[] { "AccesoId", "Proveedor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccesoProveedor_Proveedor_ProveedorUserId",
                table: "AccesoProveedor",
                columns: new[] { "Proveedor", "ProveedorUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccesoProveedor");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Acceso",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
