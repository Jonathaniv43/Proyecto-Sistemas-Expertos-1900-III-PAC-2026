using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccesoYRefresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VinculoEmpresa_Email",
                table: "VinculoEmpresa");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VinculoEmpresa_EmailEnMinusculas",
                table: "VinculoEmpresa");

            migrationBuilder.DropIndex(
                name: "IX_StaffPlataforma_Email",
                table: "StaffPlataforma");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StaffPlataforma_EmailEnMinusculas",
                table: "StaffPlataforma");

            migrationBuilder.DropColumn(
                name: "DebeCambiarPassword",
                table: "VinculoEmpresa");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "VinculoEmpresa");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "VinculoEmpresa");

            migrationBuilder.DropColumn(
                name: "DebeCambiarPassword",
                table: "StaffPlataforma");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StaffPlataforma");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "StaffPlataforma");

            migrationBuilder.CreateTable(
                name: "Acceso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Verificado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acceso", x => x.Id);
                    table.CheckConstraint("CK_Acceso_EmailEnMinusculas", "\"Email\" = lower(\"Email\")");
                });

            migrationBuilder.AddColumn<Guid>(
                name: "AccesoId",
                table: "VinculoEmpresa",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccesoId",
                table: "StaffPlataforma",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccesoId = table.Column<Guid>(type: "uuid", nullable: false),
                    VinculoEmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TipoSesion = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.CheckConstraint("CK_RefreshToken_TipoSesion", "\"TipoSesion\" IN ('empresa', 'plataforma')");
                    table.ForeignKey(
                        name: "FK_RefreshToken_Acceso_AccesoId",
                        column: x => x.AccesoId,
                        principalTable: "Acceso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefreshToken_VinculoEmpresa_VinculoEmpresaId",
                        column: x => x.VinculoEmpresaId,
                        principalTable: "VinculoEmpresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VinculoEmpresa_AccesoId",
                table: "VinculoEmpresa",
                column: "AccesoId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculoEmpresa_EmpresaId_AccesoId",
                table: "VinculoEmpresa",
                columns: new[] { "EmpresaId", "AccesoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlataforma_AccesoId",
                table: "StaffPlataforma",
                column: "AccesoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Acceso_Email",
                table: "Acceso",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_AccesoId",
                table: "RefreshToken",
                column: "AccesoId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_VinculoEmpresaId",
                table: "RefreshToken",
                column: "VinculoEmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffPlataforma_Acceso_AccesoId",
                table: "StaffPlataforma",
                column: "AccesoId",
                principalTable: "Acceso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VinculoEmpresa_Acceso_AccesoId",
                table: "VinculoEmpresa",
                column: "AccesoId",
                principalTable: "Acceso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                ALTER TABLE "VinculoEmpresa" ALTER COLUMN "AccesoId" SET NOT NULL;
                ALTER TABLE "StaffPlataforma" ALTER COLUMN "AccesoId" SET NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffPlataforma_Acceso_AccesoId",
                table: "StaffPlataforma");

            migrationBuilder.DropForeignKey(
                name: "FK_VinculoEmpresa_Acceso_AccesoId",
                table: "VinculoEmpresa");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "Acceso");

            migrationBuilder.DropIndex(
                name: "IX_VinculoEmpresa_AccesoId",
                table: "VinculoEmpresa");

            migrationBuilder.DropIndex(
                name: "IX_VinculoEmpresa_EmpresaId_AccesoId",
                table: "VinculoEmpresa");

            migrationBuilder.DropIndex(
                name: "IX_StaffPlataforma_AccesoId",
                table: "StaffPlataforma");

            migrationBuilder.DropColumn(
                name: "AccesoId",
                table: "VinculoEmpresa");

            migrationBuilder.DropColumn(
                name: "AccesoId",
                table: "StaffPlataforma");

            migrationBuilder.AddColumn<bool>(
                name: "DebeCambiarPassword",
                table: "VinculoEmpresa",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "VinculoEmpresa",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "VinculoEmpresa",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DebeCambiarPassword",
                table: "StaffPlataforma",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StaffPlataforma",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "StaffPlataforma",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VinculoEmpresa_Email",
                table: "VinculoEmpresa",
                column: "Email",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_VinculoEmpresa_EmailEnMinusculas",
                table: "VinculoEmpresa",
                sql: "\"Email\" = lower(\"Email\")");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlataforma_Email",
                table: "StaffPlataforma",
                column: "Email",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StaffPlataforma_EmailEnMinusculas",
                table: "StaffPlataforma",
                sql: "\"Email\" = lower(\"Email\")");
        }
    }
}
