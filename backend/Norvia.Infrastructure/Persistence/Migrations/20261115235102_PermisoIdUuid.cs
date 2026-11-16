using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PermisoIdUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolPermiso_Permiso_PermisoId",
                table: "RolPermiso");

            migrationBuilder.DropForeignKey(
                name: "FK_RolStaffPlataformaPermiso_Permiso_PermisoId",
                table: "RolStaffPlataformaPermiso");

            // Dev: seeder vuelve a cargar permisos y asignaciones tras el cambio de tipo.
            migrationBuilder.Sql("""
                DELETE FROM "RolStaffPlataformaPermiso";
                DELETE FROM "RolPermiso";
                DELETE FROM "Permiso";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "RolStaffPlataformaPermiso" ALTER COLUMN "PermisoId" TYPE uuid USING gen_random_uuid();
                ALTER TABLE "RolPermiso" ALTER COLUMN "PermisoId" TYPE uuid USING gen_random_uuid();
                ALTER TABLE "Permiso" ALTER COLUMN "Id" TYPE uuid USING gen_random_uuid();
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Permiso_MetodoHttp_RutaPatron",
                table: "Permiso",
                columns: new[] { "MetodoHttp", "RutaPatron" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RolPermiso_Permiso_PermisoId",
                table: "RolPermiso",
                column: "PermisoId",
                principalTable: "Permiso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolStaffPlataformaPermiso_Permiso_PermisoId",
                table: "RolStaffPlataformaPermiso",
                column: "PermisoId",
                principalTable: "Permiso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolPermiso_Permiso_PermisoId",
                table: "RolPermiso");

            migrationBuilder.DropForeignKey(
                name: "FK_RolStaffPlataformaPermiso_Permiso_PermisoId",
                table: "RolStaffPlataformaPermiso");

            migrationBuilder.DropIndex(
                name: "IX_Permiso_MetodoHttp_RutaPatron",
                table: "Permiso");

            migrationBuilder.Sql("""
                DELETE FROM "RolStaffPlataformaPermiso";
                DELETE FROM "RolPermiso";
                DELETE FROM "Permiso";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "RolStaffPlataformaPermiso" ALTER COLUMN "PermisoId" TYPE smallint USING 0;
                ALTER TABLE "RolPermiso" ALTER COLUMN "PermisoId" TYPE smallint USING 0;
                ALTER TABLE "Permiso" ALTER COLUMN "Id" TYPE smallint USING 0;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_RolPermiso_Permiso_PermisoId",
                table: "RolPermiso",
                column: "PermisoId",
                principalTable: "Permiso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolStaffPlataformaPermiso_Permiso_PermisoId",
                table: "RolStaffPlataformaPermiso",
                column: "PermisoId",
                principalTable: "Permiso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
