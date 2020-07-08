using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class statement3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_AccountingDate",
                table: "Statements",
                newName: "IX_SubmissionDeadline");

            migrationBuilder.RenameIndex(
                name: "IX_SnapshotDate",
                table: "OrganisationScopes",
                newName: "IX_SubmissionDeadline");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmissionDeadline",
                table: "Statements",
                type: "Date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmissionDeadline",
                table: "OrganisationScopes",
                type: "date",
                nullable: false,
                defaultValueSql: "('1900-01-01T00:00:00.000')",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "('1900-01-01T00:00:00.000')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_SubmissionDeadline",
                table: "Statements",
                newName: "IX_AccountingDate");

            migrationBuilder.RenameIndex(
                name: "IX_SubmissionDeadline",
                table: "OrganisationScopes",
                newName: "IX_SnapshotDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmissionDeadline",
                table: "Statements",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "Date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmissionDeadline",
                table: "OrganisationScopes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "('1900-01-01T00:00:00.000')",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "('1900-01-01T00:00:00.000')");
        }
    }
}
