using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class statement7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatementStatuses_Statements_StatementId1",
                table: "StatementStatuses");

            migrationBuilder.DropIndex(
                name: "IX_StatementStatuses_StatementId1",
                table: "StatementStatuses");

            migrationBuilder.DropColumn(
                name: "StatementId1",
                table: "StatementStatuses");

            migrationBuilder.DropColumn(
                name: "IncludedOrganistionCount",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "StatementDiligenceParentTypeId",
                table: "StatementDiligenceTypes");

            migrationBuilder.RenameTable(
                name: "StatementTrainings",
                newName: "StatementTraining");

            migrationBuilder.RenameIndex(
                name: "IX_ReportingStartDate",
                table: "Statements",
                newName: "IX_StatementStartDate");

            migrationBuilder.RenameIndex(
                name: "IX_ReportingEndDate",
                table: "Statements",
                newName: "IX_StatementEndDate");

            migrationBuilder.RenameIndex(
                name: "IX_StatementTrainings_StatementId",
                table: "StatementTraining",
                newName: "IX_StatementTraining_StatementId");

            migrationBuilder.AlterColumn<string>(
                name: "TrainingDetails",
                table: "Statements",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StructureDetails",
                table: "Statements",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "StatusId",
                table: "Statements",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "StatementUrl",
                table: "Statements",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatementStartDate",
                table: "Statements",
                type: "Date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatementEndDate",
                table: "Statements",
                type: "Date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "RisksDetails",
                table: "Statements",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PolicyDetails",
                table: "Statements",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OtherRelevantRisks",
                table: "Statements",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MinTurnover",
                table: "Statements",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "MaxTurnover",
                table: "Statements",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "GoalsDetails",
                table: "Statements",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ExcludedOrganisationCount",
                table: "Statements",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DueDiligenceDetails",
                table: "Statements",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                table: "Statements",
                type: "Date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "IncludedOrganisationCount",
                table: "Statements",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "ParentDiligenceTypeId",
                table: "StatementDiligenceTypes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludedOrganisationCount",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "ParentDiligenceTypeId",
                table: "StatementDiligenceTypes");

            migrationBuilder.RenameTable(
                name: "StatementTraining",
                newName: "StatementTrainings");

            migrationBuilder.RenameIndex(
                name: "IX_StatementStartDate",
                table: "Statements",
                newName: "IX_ReportingStartDate");

            migrationBuilder.RenameIndex(
                name: "IX_StatementEndDate",
                table: "Statements",
                newName: "IX_ReportingEndDate");

            migrationBuilder.RenameIndex(
                name: "IX_StatementTraining_StatementId",
                table: "StatementTrainings",
                newName: "IX_StatementTrainings_StatementId");

            migrationBuilder.AddColumn<long>(
                name: "StatementId1",
                table: "StatementStatuses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrainingDetails",
                table: "Statements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StructureDetails",
                table: "Statements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "StatusId",
                table: "Statements",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<string>(
                name: "StatementUrl",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatementStartDate",
                table: "Statements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "Date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatementEndDate",
                table: "Statements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "Date");

            migrationBuilder.AlterColumn<string>(
                name: "RisksDetails",
                table: "Statements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PolicyDetails",
                table: "Statements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OtherRelevantRisks",
                table: "Statements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinTurnover",
                table: "Statements",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxTurnover",
                table: "Statements",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "GoalsDetails",
                table: "Statements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ExcludedOrganisationCount",
                table: "Statements",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "DueDiligenceDetails",
                table: "Statements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedDate",
                table: "Statements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "Date");

            migrationBuilder.AddColumn<int>(
                name: "IncludedOrganistionCount",
                table: "Statements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "StatementDiligenceParentTypeId",
                table: "StatementDiligenceTypes",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatementStatuses_StatementId1",
                table: "StatementStatuses",
                column: "StatementId1",
                unique: true,
                filter: "[StatementId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_StatementStatuses_Statements_StatementId1",
                table: "StatementStatuses",
                column: "StatementId1",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
