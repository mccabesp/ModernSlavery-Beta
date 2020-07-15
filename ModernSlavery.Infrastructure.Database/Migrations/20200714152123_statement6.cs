using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class statement6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatementRisks");

            migrationBuilder.RenameTable(
                name: "StatementRiskCountries",
                newName: "StatementLocationRisks");

            migrationBuilder.RenameIndex(
                name: "IX_StatementRiskCountries_StatementId",
                table: "StatementLocationRisks",
                newName: "IX_StatementLocationRisks_StatementId");

            migrationBuilder.CreateTable(
                name: "StatementRelevantRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    StatementRelevantRiskId = table.Column<short>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_dbo.StatementRisks_dbo.Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.StatementRisk_dbo.StatementRiskType_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatementRelevantRisks_StatementId",
                table: "StatementRelevantRisks",
                column: "StatementId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatementRelevantRisks");

            migrationBuilder.RenameTable(
                name: "StatementLocationRisks",
                newName: "StatementRiskCountries");

            migrationBuilder.RenameIndex(
                name: "IX_StatementLocationRisks_StatementId",
                table: "StatementRiskCountries",
                newName: "IX_StatementRiskCountries_StatementId");

            migrationBuilder.CreateTable(
                name: "StatementRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatementRiskId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_dbo.StatementRisks_dbo.Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.StatementRisk_dbo.StatementRiskType_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatementRisks_StatementId",
                table: "StatementRisks",
                column: "StatementId");
        }
    }
}
