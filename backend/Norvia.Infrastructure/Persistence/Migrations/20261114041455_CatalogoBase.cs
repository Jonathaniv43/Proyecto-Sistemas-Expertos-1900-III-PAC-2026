using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Rol",
                columns: new[] { "Id", "Codigo", "Nombre" },
                values: new object[,]
                {
                    { (short)1, "admin_empresa", "Administrador de empresa" },
                    { (short)2, "responsable_sst", "Responsable SST" },
                    { (short)3, "colaborador", "Colaborador" },
                    { (short)4, "encuestado", "Encuestado" }
                });

            migrationBuilder.InsertData(
                table: "RolStaffPlataforma",
                columns: new[] { "Id", "Codigo", "Nombre" },
                values: new object[,]
                {
                    { (short)1, "admin_plataforma", "Administrador de plataforma" },
                    { (short)2, "soporte_plataforma", "Soporte de plataforma" }
                });

            migrationBuilder.InsertData(
                table: "Rubro",
                columns: new[] { "Id", "Activo", "Codigo", "Nombre" },
                values: new object[,]
                {
                    { (short)1, true, "oficina", "Oficina administrativa" },
                    { (short)2, true, "comercio", "Comercio o tienda" },
                    { (short)3, true, "maquila_fabrica", "Maquila o fábrica" },
                    { (short)4, true, "construccion", "Construcción" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rol",
                keyColumn: "Id",
                keyValue: (short)1);

            migrationBuilder.DeleteData(
                table: "Rol",
                keyColumn: "Id",
                keyValue: (short)2);

            migrationBuilder.DeleteData(
                table: "Rol",
                keyColumn: "Id",
                keyValue: (short)3);

            migrationBuilder.DeleteData(
                table: "Rol",
                keyColumn: "Id",
                keyValue: (short)4);

            migrationBuilder.DeleteData(
                table: "RolStaffPlataforma",
                keyColumn: "Id",
                keyValue: (short)1);

            migrationBuilder.DeleteData(
                table: "RolStaffPlataforma",
                keyColumn: "Id",
                keyValue: (short)2);

            migrationBuilder.DeleteData(
                table: "Rubro",
                keyColumn: "Id",
                keyValue: (short)1);

            migrationBuilder.DeleteData(
                table: "Rubro",
                keyColumn: "Id",
                keyValue: (short)2);

            migrationBuilder.DeleteData(
                table: "Rubro",
                keyColumn: "Id",
                keyValue: (short)3);

            migrationBuilder.DeleteData(
                table: "Rubro",
                keyColumn: "Id",
                keyValue: (short)4);
        }
    }
}
