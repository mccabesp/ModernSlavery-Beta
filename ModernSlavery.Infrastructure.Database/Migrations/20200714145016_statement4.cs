using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class statement4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatementSectors_Statements_StatementId",
                table: "StatementSectors");

            migrationBuilder.DropForeignKey(
                name: "FK_StatementStatuses_Statements_StatementId",
                table: "StatementStatuses");

            migrationBuilder.DropTable(
                name: "StatementTrainingDivisions");

            migrationBuilder.DropTable(
                name: "StatementDivisionTypes");

            migrationBuilder.DropIndex(
                name: "IX_IncludesEffectiveness",
                table: "Statements");

            migrationBuilder.DropIndex(
                name: "IX_IncludesMethods",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "IncludesEffectiveness",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "IncludesMethods",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "MeasuringProgress",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherPolicy",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherRisk",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherTrainingDivision",
                table: "Statements");

            migrationBuilder.RenameColumn(
                name: "IncludesGoalsId",
                table: "Statements",
                newName: "IncludesGoals");

            migrationBuilder.AddColumn<long>(
                name: "StatementId1",
                table: "StatementStatuses",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "StatusId",
                table: "Statements",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<bool>(
                name: "IncludesGoals",
                table: "Statements",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<string>(
                name: "DueDiligenceDetails",
                table: "Statements",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForcedLabourDetails",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FoundModernSlaveryInOperations",
                table: "Statements",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GoalsDetails",
                table: "Statements",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IdentifiedForcedLabour",
                table: "Statements",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesDueDiligence",
                table: "Statements",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesMeasuringProgress",
                table: "Statements",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KeyAchievements",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxStatementYears",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinStatementYears",
                table: "Statements",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OtherHighRisks",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPolicies",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherRelavantRisks",
                table: "Statements",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherTrainingTypes",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolicyDetails",
                table: "Statements",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressMeasures",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RisksDetails",
                table: "Statements",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaveryInstanceDetails",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaveryInstanceRemediation",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StructureDetails",
                table: "Statements",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainingDetails",
                table: "Statements",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "StatementRiskTypes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "StatementRisks",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "StatementDiligenceParentStatementDiligenceTypeId",
                table: "StatementDiligenceTypes",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "StatementDiligenceParentTypeId",
                table: "StatementDiligenceTypes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "StatementDiligences",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StatementHighRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    StatementRiskId = table.Column<short>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementHighRisks", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_dbo.StatementHighRisks_dbo.Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.StatementHighRisks_dbo.StatementRiskType_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementRiskCountries",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    StatementRiskCountryId = table.Column<short>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementRiskCountry", x => new { x.StatementRiskTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementRiskCountries_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementRiskCountries_StatementRiskTypes_StatementRiskTypeId",
                        column: x => x.StatementRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementTrainingTypes",
                columns: table => new
                {
                    StatementTrainingTypeId = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementDivisionTypes", x => x.StatementTrainingTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementTrainings",
                columns: table => new
                {
                    StatementTrainingTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementTrainingDivisions", x => new { x.StatementTrainingTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_dbo.StatementTrainingDivisions_dbo.Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.StatementTrainingDivisions_dbo.StatementDivisionTypes_StatmentDivisionTypeId",
                        column: x => x.StatementTrainingTypeId,
                        principalTable: "StatementTrainingTypes",
                        principalColumn: "StatementTrainingTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatementStatuses_StatementId1",
                table: "StatementStatuses",
                column: "StatementId1",
                unique: true,
                filter: "[StatementId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesDueDiligence",
                table: "Statements",
                column: "IncludesDueDiligence");

            migrationBuilder.CreateIndex(
                name: "IX_StatementDiligenceTypes_StatementDiligenceParentStatementDiligenceTypeId",
                table: "StatementDiligenceTypes",
                column: "StatementDiligenceParentStatementDiligenceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementHighRisks_StatementId",
                table: "StatementHighRisks",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRiskCountries_StatementId",
                table: "StatementRiskCountries",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementTrainings_StatementId",
                table: "StatementTrainings",
                column: "StatementId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatementDiligenceTypes_StatementDiligenceTypes_StatementDiligenceParentStatementDiligenceTypeId",
                table: "StatementDiligenceTypes",
                column: "StatementDiligenceParentStatementDiligenceTypeId",
                principalTable: "StatementDiligenceTypes",
                principalColumn: "StatementDiligenceTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.StatementSectors_dbo.Statements_StatementId",
                table: "StatementSectors",
                column: "StatementId",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.StatementStatuses_dbo.Statement_StatementId",
                table: "StatementStatuses",
                column: "StatementId",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StatementStatuses_Statements_StatementId1",
                table: "StatementStatuses",
                column: "StatementId1",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatementDiligenceTypes_StatementDiligenceTypes_StatementDiligenceParentStatementDiligenceTypeId",
                table: "StatementDiligenceTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.StatementSectors_dbo.Statements_StatementId",
                table: "StatementSectors");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.StatementStatuses_dbo.Statement_StatementId",
                table: "StatementStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_StatementStatuses_Statements_StatementId1",
                table: "StatementStatuses");

            migrationBuilder.DropTable(
                name: "StatementHighRisks");

            migrationBuilder.DropTable(
                name: "StatementRiskCountries");

            migrationBuilder.DropTable(
                name: "StatementTrainings");

            migrationBuilder.DropTable(
                name: "StatementTrainingTypes");

            migrationBuilder.DropIndex(
                name: "IX_StatementStatuses_StatementId1",
                table: "StatementStatuses");

            migrationBuilder.DropIndex(
                name: "IX_IncludesDueDiligence",
                table: "Statements");

            migrationBuilder.DropIndex(
                name: "IX_StatementDiligenceTypes_StatementDiligenceParentStatementDiligenceTypeId",
                table: "StatementDiligenceTypes");

            migrationBuilder.DropColumn(
                name: "StatementId1",
                table: "StatementStatuses");

            migrationBuilder.DropColumn(
                name: "DueDiligenceDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "ForcedLabourDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "FoundModernSlaveryInOperations",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "GoalsDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "IdentifiedForcedLabour",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "IncludesDueDiligence",
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
                name: "MinStatementYears",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherHighRisks",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherPolicies",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherRelavantRisks",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherTrainingTypes",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "PolicyDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "ProgressMeasures",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "RisksDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "SlaveryInstanceDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "SlaveryInstanceRemediation",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "StructureDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "TrainingDetails",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "StatementRiskTypes");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "StatementRisks");

            migrationBuilder.DropColumn(
                name: "StatementDiligenceParentStatementDiligenceTypeId",
                table: "StatementDiligenceTypes");

            migrationBuilder.DropColumn(
                name: "StatementDiligenceParentTypeId",
                table: "StatementDiligenceTypes");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "StatementDiligences");

            migrationBuilder.RenameColumn(
                name: "IncludesGoals",
                table: "Statements",
                newName: "IncludesGoalsId");

            migrationBuilder.AlterColumn<byte>(
                name: "StatusId",
                table: "Statements",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<byte>(
                name: "IncludesGoalsId",
                table: "Statements",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AddColumn<bool>(
                name: "IncludesEffectiveness",
                table: "Statements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesMethods",
                table: "Statements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MeasuringProgress",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPolicy",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherRisk",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherTrainingDivision",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StatementDivisionTypes",
                columns: table => new
                {
                    StatementDivisionTypeId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementDivisionTypes", x => x.StatementDivisionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementTrainingDivisions",
                columns: table => new
                {
                    StatementDivisionTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StatementId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementTrainingDivisions", x => new { x.StatementDivisionTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_dbo.StatementTrainingDivisions_dbo.StatementDivisionTypes_StatmentDivisionTypeId",
                        column: x => x.StatementDivisionTypeId,
                        principalTable: "StatementDivisionTypes",
                        principalColumn: "StatementDivisionTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.StatementTrainingDivisions_dbo.Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncludesEffectiveness",
                table: "Statements",
                column: "IncludesEffectiveness");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesMethods",
                table: "Statements",
                column: "IncludesMethods");

            migrationBuilder.CreateIndex(
                name: "IX_StatementTrainingDivisions_StatementId",
                table: "StatementTrainingDivisions",
                column: "StatementId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatementSectors_Statements_StatementId",
                table: "StatementSectors",
                column: "StatementId",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StatementStatuses_Statements_StatementId",
                table: "StatementStatuses",
                column: "StatementId",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
