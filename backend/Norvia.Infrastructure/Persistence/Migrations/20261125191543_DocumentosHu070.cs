using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Norvia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentosHu070 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantillaDocumentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoDocumento = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    CreadoPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documento", x => x.Id);
                    table.UniqueConstraint("AK_Documento_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_Documento_TipoDocumento", "\"TipoDocumento\" IN ('politica', 'procedimiento')");
                    table.ForeignKey(
                        name: "FK_Documento_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documento_PlantillaDocumento_PlantillaDocumentoId",
                        column: x => x.PlantillaDocumentoId,
                        principalTable: "PlantillaDocumento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documento_VinculoEmpresa_EmpresaId_CreadoPorId",
                        columns: x => new { x.EmpresaId, x.CreadoPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VersionDocumento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroVersion = table.Column<int>(type: "integer", nullable: false),
                    Contenido = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    CreadaPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    AprobadaPorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AprobadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionDocumento", x => x.Id);
                    table.UniqueConstraint("AK_VersionDocumento_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.CheckConstraint("CK_VersionDocumento_Aprobacion", "\"Estado\" = 'borrador' OR (\"AprobadaPorId\" IS NOT NULL AND \"AprobadaEn\" IS NOT NULL)");
                    table.CheckConstraint("CK_VersionDocumento_Estado", "\"Estado\" IN ('borrador', 'vigente', 'reemplazada')");
                    table.CheckConstraint("CK_VersionDocumento_NumeroVersion", "\"NumeroVersion\" >= 1");
                    table.ForeignKey(
                        name: "FK_VersionDocumento_Documento_EmpresaId_DocumentoId",
                        columns: x => new { x.EmpresaId, x.DocumentoId },
                        principalTable: "Documento",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VersionDocumento_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VersionDocumento_VinculoEmpresa_EmpresaId_AprobadaPorId",
                        columns: x => new { x.EmpresaId, x.AprobadaPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VersionDocumento_VinculoEmpresa_EmpresaId_CreadaPorId",
                        columns: x => new { x.EmpresaId, x.CreadaPorId },
                        principalTable: "VinculoEmpresa",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_EmpresaId_VersionDocumentoId",
                table: "Notificacion",
                columns: new[] { "EmpresaId", "VersionDocumentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documento_EmpresaId_CreadoPorId",
                table: "Documento",
                columns: new[] { "EmpresaId", "CreadoPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documento_PlantillaDocumentoId",
                table: "Documento",
                column: "PlantillaDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_VersionDocumento_DocumentoId_Borrador",
                table: "VersionDocumento",
                column: "DocumentoId",
                unique: true,
                filter: "\"Estado\" = 'borrador'");

            migrationBuilder.CreateIndex(
                name: "IX_VersionDocumento_DocumentoId_NumeroVersion",
                table: "VersionDocumento",
                columns: new[] { "DocumentoId", "NumeroVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VersionDocumento_DocumentoId_Vigente",
                table: "VersionDocumento",
                column: "DocumentoId",
                unique: true,
                filter: "\"Estado\" = 'vigente'");

            migrationBuilder.CreateIndex(
                name: "IX_VersionDocumento_EmpresaId_AprobadaPorId",
                table: "VersionDocumento",
                columns: new[] { "EmpresaId", "AprobadaPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_VersionDocumento_EmpresaId_CreadaPorId",
                table: "VersionDocumento",
                columns: new[] { "EmpresaId", "CreadaPorId" });

            migrationBuilder.CreateIndex(
                name: "IX_VersionDocumento_EmpresaId_DocumentoId",
                table: "VersionDocumento",
                columns: new[] { "EmpresaId", "DocumentoId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notificacion_VersionDocumento_EmpresaId_VersionDocumentoId",
                table: "Notificacion",
                columns: new[] { "EmpresaId", "VersionDocumentoId" },
                principalTable: "VersionDocumento",
                principalColumns: new[] { "EmpresaId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificacion_VersionDocumento_EmpresaId_VersionDocumentoId",
                table: "Notificacion");

            migrationBuilder.DropTable(
                name: "VersionDocumento");

            migrationBuilder.DropTable(
                name: "Documento");

            migrationBuilder.DropIndex(
                name: "IX_Notificacion_EmpresaId_VersionDocumentoId",
                table: "Notificacion");
        }
    }
}
