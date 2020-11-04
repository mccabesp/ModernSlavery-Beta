using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class AddNoSQLStatementSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatementDiligences");

            migrationBuilder.DropTable(
                name: "StatementHighRisks");

            migrationBuilder.DropTable(
                name: "StatementLocationRisks");

            migrationBuilder.DropTable(
                name: "StatementPolicies");

            migrationBuilder.DropTable(
                name: "StatementRelevantRisks");

            migrationBuilder.DropTable(
                name: "StatementTrainings");

            migrationBuilder.DropTable(
                name: "StatementDiligenceTypes");

            migrationBuilder.DropTable(
                name: "StatementPolicyTypes");

            migrationBuilder.DropTable(
                name: "StatementRiskTypes");

            migrationBuilder.DropTable(
                name: "StatementTrainingTypes");

            migrationBuilder.DropIndex(
                name: "IX_Statements_MaxTurnover",
                table: "Statements");

            migrationBuilder.DropIndex(
                name: "IX_Statements_MinTurnover",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "EHRCResponse",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "ExcludedOrganisationCount",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "ForcedLabourDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "IncludedOrganisationCount",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "IncludesMeasuringProgress",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "KeyAchievements",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "MaxStatementYears",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "MaxTurnover",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "MinStatementYears",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "MinTurnover",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherHighRisks",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherPolicies",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherRelevantRisks",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherSector",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherTraining",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "ProgressMeasures",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "SlaveryInstanceDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "SlaveryInstanceRemediation",
                table: "Statements");

            migrationBuilder.AddColumn<string>(
                name: "OtherSectors",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatementEmail",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "StatementYearsId",
                table: "Statements",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TurnoverId",
                table: "Statements",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_Statements_StatementYearsId",
                table: "Statements",
                column: "StatementYearsId");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_TurnoverId",
                table: "Statements",
                column: "TurnoverId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Statements_StatementYearsId",
                table: "Statements");

            migrationBuilder.DropIndex(
                name: "IX_Statements_TurnoverId",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherSectors",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "StatementEmail",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "StatementYearsId",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "TurnoverId",
                table: "Statements");

            migrationBuilder.AddColumn<bool>(
                name: "EHRCResponse",
                table: "Statements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<short>(
                name: "ExcludedOrganisationCount",
                table: "Statements",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "ForcedLabourDetails",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "IncludedOrganisationCount",
                table: "Statements",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesMeasuringProgress",
                table: "Statements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KeyAchievements",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "MaxStatementYears",
                table: "Statements",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxTurnover",
                table: "Statements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "MinStatementYears",
                table: "Statements",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "MinTurnover",
                table: "Statements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OtherHighRisks",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPolicies",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherRelevantRisks",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherSector",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherTraining",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressMeasures",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaveryInstanceDetails",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaveryInstanceRemediation",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StatementDiligenceTypes",
                columns: table => new
                {
                    StatementDiligenceTypeId = table.Column<short>(type: "smallint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ParentDiligenceTypeId = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementDiligenceTypes", x => x.StatementDiligenceTypeId);
                    table.ForeignKey(
                        name: "FK_StatementDiligenceTypes_StatementDiligenceTypes_ParentDiligenceTypeId",
                        column: x => x.ParentDiligenceTypeId,
                        principalTable: "StatementDiligenceTypes",
                        principalColumn: "StatementDiligenceTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatementPolicyTypes",
                columns: table => new
                {
                    StatementPolicyTypeId = table.Column<short>(type: "smallint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementPolicyTypes", x => x.StatementPolicyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementRiskTypes",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(type: "smallint", nullable: false),
                    RiskCategoryId = table.Column<byte>(type: "tinyint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ParentRiskTypeId = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementRiskTypes", x => x.StatementRiskTypeId);
                    table.ForeignKey(
                        name: "FK_StatementRiskTypes_StatementRiskTypes_ParentRiskTypeId",
                        column: x => x.ParentRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatementTrainingTypes",
                columns: table => new
                {
                    StatementTrainingTypeId = table.Column<short>(type: "smallint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementTrainingTypes", x => x.StatementTrainingTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementDiligences",
                columns: table => new
                {
                    StatementDiligenceTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementDiligences", x => new { x.StatementDiligenceTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementDiligences_StatementDiligenceTypes_StatementDiligenceTypeId",
                        column: x => x.StatementDiligenceTypeId,
                        principalTable: "StatementDiligenceTypes",
                        principalColumn: "StatementDiligenceTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementDiligences_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementPolicies",
                columns: table => new
                {
                    StatementPolicyTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementPolicies", x => new { x.StatementPolicyTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementPolicies_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementPolicies_StatementPolicyTypes_StatementPolicyTypeId",
                        column: x => x.StatementPolicyTypeId,
                        principalTable: "StatementPolicyTypes",
                        principalColumn: "StatementPolicyTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementHighRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementHighRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementHighRisks_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementHighRisks_StatementRiskTypes_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementLocationRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementLocationRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementLocationRisks_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementLocationRisks_StatementRiskTypes_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementRelevantRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementRelevantRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementRelevantRisks_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementRelevantRisks_StatementRiskTypes_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementTrainings",
                columns: table => new
                {
                    StatementTrainingTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatementTrainings", x => new { x.StatementTrainingTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementTrainings_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementTrainings_StatementTrainingTypes_StatementTrainingTypeId",
                        column: x => x.StatementTrainingTypeId,
                        principalTable: "StatementTrainingTypes",
                        principalColumn: "StatementTrainingTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statements_MaxTurnover",
                table: "Statements",
                column: "MaxTurnover");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_MinTurnover",
                table: "Statements",
                column: "MinTurnover");

            migrationBuilder.CreateIndex(
                name: "IX_StatementDiligences_StatementId",
                table: "StatementDiligences",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementDiligenceTypes_ParentDiligenceTypeId",
                table: "StatementDiligenceTypes",
                column: "ParentDiligenceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementHighRisks_StatementId",
                table: "StatementHighRisks",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementLocationRisks_StatementId",
                table: "StatementLocationRisks",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementPolicies_StatementId",
                table: "StatementPolicies",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRelevantRisks_StatementId",
                table: "StatementRelevantRisks",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRiskTypes_ParentRiskTypeId",
                table: "StatementRiskTypes",
                column: "ParentRiskTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementTrainings_StatementId",
                table: "StatementTrainings",
                column: "StatementId");
        }
    }
}
