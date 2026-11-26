using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRegistroAuditoriaHu099 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistroAuditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VinculoEmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffPlataformaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Entidad = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Detalle = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistroAuditoria", x => x.Id);
                    table.UniqueConstraint("AK_RegistroAuditoria_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_RegistroAuditoria_AutorExcluyente", "\"VinculoEmpresaId\" IS NULL OR \"StaffPlataformaId\" IS NULL");
                    table.ForeignKey(
                        name: "FK_RegistroAuditoria_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistroAuditoria_StaffPlataforma_StaffPlataformaId",
                        column: x => x.StaffPlataformaId,
                        principalTable: "StaffPlataforma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistroAuditoria_VinculoEmpresa_EmpresaId_VinculoEmpresaId",
                        columns: x => new { x.EmpresaId, x.VinculoEmpresaId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistroAuditoria_EmpresaId_Fecha",
                table: "RegistroAuditoria",
                columns: new[] { "EmpresaId", "Fecha" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_RegistroAuditoria_EmpresaId_VinculoEmpresaId",
                table: "RegistroAuditoria",
                columns: new[] { "EmpresaId", "VinculoEmpresaId" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistroAuditoria_Entidad_EntidadId",
                table: "RegistroAuditoria",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistroAuditoria_StaffPlataformaId",
                table: "RegistroAuditoria",
                column: "StaffPlataformaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistroAuditoria");
        }
    }
}
