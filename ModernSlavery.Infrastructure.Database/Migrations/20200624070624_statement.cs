using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class statement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatementDiligenceTypes",
                columns: table => new
                {
                    StatementDiligenceTypeId = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementDiligenceTypes", x => x.StatementDiligenceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementDivisionTypes",
                columns: table => new
                {
                    StatementDivisionTypeId = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementDivisionTypes", x => x.StatementDivisionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementPolicyTypes",
                columns: table => new
                {
                    StatementPolicyTypeId = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementPolicyTypes", x => x.StatementPolicyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementRiskTypes",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentRiskTypeId = table.Column<short>(nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementRiskTypes", x => x.StatementRiskTypeId);
                    table.ForeignKey(
                        name: "FK_dbo.StatementRisks_dbo.StatementRisks_StatementRiskId",
                        column: x => x.ParentRiskTypeId,
                        principalTable: "StatementRiskTypes",
                        principalColumn: "StatementRiskTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Statements",
                columns: table => new
                {
                    StatementId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganisationId = table.Column<long>(nullable: false),
                    StatementStartDate = table.Column<DateTime>(nullable: false),
                    StatementEndDate = table.Column<DateTime>(nullable: false),
                    SubmissionDeadline = table.Column<DateTime>(nullable: false),
                    StatementUrl = table.Column<string>(nullable: true),
                    IncludesGoalsId = table.Column<byte>(nullable: false),
                    IncludesStructure = table.Column<bool>(nullable: false),
                    IncludesPolicies = table.Column<bool>(nullable: false),
                    IncludesMethods = table.Column<bool>(nullable: false),
                    IncludesRisks = table.Column<bool>(nullable: false),
                    IncludesEffectiveness = table.Column<bool>(nullable: false),
                    IncludesTraining = table.Column<bool>(nullable: false),
                    IncludedOrganistionCount = table.Column<int>(nullable: false),
                    ExcludedOrganisationCount = table.Column<int>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    JobTitle = table.Column<string>(maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(maxLength: 50, nullable: true),
                    LastName = table.Column<string>(maxLength: 50, nullable: true),
                    ApprovedDate = table.Column<DateTime>(nullable: false),
                    MinTurnover = table.Column<decimal>(nullable: false),
                    MaxTurnover = table.Column<decimal>(nullable: false),
                    LateReason = table.Column<string>(maxLength: 200, nullable: true),
                    OtherTrainingDivision = table.Column<string>(nullable: true),
                    OtherPolicy = table.Column<string>(nullable: true),
                    OtherRisk = table.Column<string>(nullable: true),
                    OtherSector = table.Column<string>(nullable: true),
                    MeasuringProgress = table.Column<string>(nullable: true),
                    EHRCResponse = table.Column<bool>(nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Statements", x => x.StatementId);
                    table.ForeignKey(
                        name: "FK_dbo.Statements_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementSectorTypes",
                columns: table => new
                {
                    StatementSectorTypeId = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementSectorTypes", x => x.StatementSectorTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StatementDiligences",
                columns: table => new
                {
                    StatementDiligenceTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    StatementDiligenceId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementDiligences", x => new { x.StatementDiligenceTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_dbo.StatementDiligences_dbo.StatementDiligenceType_StatementDiligenceTypeId",
                        column: x => x.StatementDiligenceTypeId,
                        principalTable: "StatementDiligenceTypes",
                        principalColumn: "StatementDiligenceTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.StatementDiligences_dbo.Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementOrganisations",
                columns: table => new
                {
                    StatementOrganisationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatementId = table.Column<long>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: true),
                    Included = table.Column<bool>(nullable: false),
                    OrganisationName = table.Column<string>(maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementOrganisations", x => x.StatementOrganisationId);
                    table.ForeignKey(
                        name: "FK_dbo.StatementOrganisations_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.StatementOrganisations_dbo.Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementPolicies",
                columns: table => new
                {
                    StatementPolicyTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementPolicies", x => new { x.StatementPolicyTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_dbo.StatementPolicies_dbo.Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.StatementPolicies_dbo.StatementPolicyType_StatementPolicyTypeId",
                        column: x => x.StatementPolicyTypeId,
                        principalTable: "StatementPolicyTypes",
                        principalColumn: "StatementPolicyTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementRisks",
                columns: table => new
                {
                    StatementRiskTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    StatementRiskId = table.Column<short>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "StatementStatuses",
                columns: table => new
                {
                    StatementStatusId = table.Column<long>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementStatuses", x => new { x.StatementStatusId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_dbo.StatementStatuses_dbo.Users_UserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementStatuses_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatementTrainingDivisions",
                columns: table => new
                {
                    StatementDivisionTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "StatementSectors",
                columns: table => new
                {
                    StatementSectorTypeId = table.Column<short>(nullable: false),
                    StatementId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.StatementSectors", x => new { x.StatementSectorTypeId, x.StatementId });
                    table.ForeignKey(
                        name: "FK_StatementSectors_Statements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "Statements",
                        principalColumn: "StatementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatementSectors_StatementSectorTypes_StatementSectorTypeId",
                        column: x => x.StatementSectorTypeId,
                        principalTable: "StatementSectorTypes",
                        principalColumn: "StatementSectorTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatementDiligences_StatementId",
                table: "StatementDiligences",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementOrganisations_OrganisationId",
                table: "StatementOrganisations",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementOrganisations_StatementId",
                table: "StatementOrganisations",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementPolicies_StatementId",
                table: "StatementPolicies",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRisks_StatementId",
                table: "StatementRisks",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementRiskTypes_ParentRiskTypeId",
                table: "StatementRiskTypes",
                column: "ParentRiskTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesEffectiveness",
                table: "Statements",
                column: "IncludesEffectiveness");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesGoals",
                table: "Statements",
                column: "IncludesGoalsId");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesMethods",
                table: "Statements",
                column: "IncludesMethods");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesPolicies",
                table: "Statements",
                column: "IncludesPolicies");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesRisks",
                table: "Statements",
                column: "IncludesRisks");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesStructure",
                table: "Statements",
                column: "IncludesStructure");

            migrationBuilder.CreateIndex(
                name: "IX_IncludesTraining",
                table: "Statements",
                column: "IncludesTraining");

            migrationBuilder.CreateIndex(
                name: "IX_MaxTurnover",
                table: "Statements",
                column: "MaxTurnover");

            migrationBuilder.CreateIndex(
                name: "IX_MinTurnover",
                table: "Statements",
                column: "MinTurnover");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "Statements",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingEndDate",
                table: "Statements",
                column: "StatementEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingStartDate",
                table: "Statements",
                column: "StatementStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_StatusId",
                table: "Statements",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingDate",
                table: "Statements",
                column: "SubmissionDeadline");

            migrationBuilder.CreateIndex(
                name: "IX_StatementSectors_StatementId",
                table: "StatementSectors",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementStatuses_ByUserId",
                table: "StatementStatuses",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementStatuses_StatementId",
                table: "StatementStatuses",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_StatementTrainingDivisions_StatementId",
                table: "StatementTrainingDivisions",
                column: "StatementId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatementDiligences");

            migrationBuilder.DropTable(
                name: "StatementOrganisations");

            migrationBuilder.DropTable(
                name: "StatementPolicies");

            migrationBuilder.DropTable(
                name: "StatementRisks");

            migrationBuilder.DropTable(
                name: "StatementSectors");

            migrationBuilder.DropTable(
                name: "StatementStatuses");

            migrationBuilder.DropTable(
                name: "StatementTrainingDivisions");

            migrationBuilder.DropTable(
                name: "StatementDiligenceTypes");

            migrationBuilder.DropTable(
                name: "StatementPolicyTypes");

            migrationBuilder.DropTable(
                name: "StatementRiskTypes");

            migrationBuilder.DropTable(
                name: "StatementSectorTypes");

            migrationBuilder.DropTable(
                name: "StatementDivisionTypes");

            migrationBuilder.DropTable(
                name: "Statements");
        }
    }
}
