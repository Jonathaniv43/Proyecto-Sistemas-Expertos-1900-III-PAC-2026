using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EventosM6Hu047 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accidente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    EstadoEnfermedad = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    EsGrave = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FechaHoraEvento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Lugar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    RegistradoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Estado = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CerradoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DescartadoPorId = table.Column<Guid>(type: "uuid", nullable: true),
                    MotivoDescarte = table.Column<string>(type: "text", nullable: true),
                    DescartadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accidente", x => x.Id);
                    table.UniqueConstraint("AK_Accidente_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_Accidente_CerradoEn", "(\"CerradoEn\" IS NOT NULL) = (\"Estado\" = 'cerrado')");
                    table.CheckConstraint("CK_Accidente_Descarte", "(\"DescartadoPorId\" IS NOT NULL AND \"DescartadoEn\" IS NOT NULL) = (\"Estado\" = 'descartado')");
                    table.CheckConstraint("CK_Accidente_Estado", "\"Estado\" IN ('abierto', 'en_investigacion', 'cerrado', 'descartado')");
                    table.CheckConstraint("CK_Accidente_EstadoEnfermedad", "\"EstadoEnfermedad\" IN ('sospechosa', 'diagnosticada')");
                    table.CheckConstraint("CK_Accidente_EstadoEnfermedadSegunTipo", "(\"EstadoEnfermedad\" IS NOT NULL) = (\"Tipo\" = 'enfermedad_profesional')");
                    table.CheckConstraint("CK_Accidente_MotivoDescarte", "\"MotivoDescarte\" IS NULL OR \"Estado\" = 'descartado'");
                    table.CheckConstraint("CK_Accidente_Tipo", "\"Tipo\" IN ('accidente', 'incidente', 'enfermedad_profesional', 'condicion_insegura')");
                    table.ForeignKey(
                        name: "FK_Accidente_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accidente_VinculoEmpresa_EmpresaId_DescartadoPorId",
                        columns: x => new { x.EmpresaId, x.DescartadoPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accidente_EmpresaId_DescartadoPorId",
                table: "Accidente",
                columns: new[] { "EmpresaId", "DescartadoPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Accidente_EmpresaId_FechaHoraEvento",
                table: "Accidente",
                columns: new[] { "EmpresaId", "FechaHoraEvento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accidente");
        }
    }
}
