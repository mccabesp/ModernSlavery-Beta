using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class statement5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatementRiskCountries_Statements_StatementId",
                table: "StatementRiskCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_StatementRiskCountries_StatementRiskTypes_StatementRiskTypeId",
                table: "StatementRiskCountries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.StatementRiskCountry",
                table: "StatementRiskCountries");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "FoundModernSlaveryInOperations",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "IdentifiedForcedLabour",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherRelavantRisks",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherTrainingTypes",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "StatementRiskCountryId",
                table: "StatementRiskCountries");

            migrationBuilder.AddColumn<string>(
                name: "ApproverFirstName",
                table: "Statements",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproverJobTitle",
                table: "Statements",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproverLastName",
                table: "Statements",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherRelevantRisks",
                table: "Statements",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherTraining",
                table: "Statements",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "StatementLocationRiskId",
                table: "StatementRiskCountries",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.StatementLocationRisks",
                table: "StatementRiskCountries",
                columns: new[] { "StatementRiskTypeId", "StatementId" });

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.StatementLocationRisks_dbo.Statements_StatementId",
                table: "StatementRiskCountries",
                column: "StatementId",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.StatementLocationRisks_dbo.StatementRiskType_StatementRiskTypeId",
                table: "StatementRiskCountries",
                column: "StatementRiskTypeId",
                principalTable: "StatementRiskTypes",
                principalColumn: "StatementRiskTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.StatementLocationRisks_dbo.Statements_StatementId",
                table: "StatementRiskCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.StatementLocationRisks_dbo.StatementRiskType_StatementRiskTypeId",
                table: "StatementRiskCountries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.StatementLocationRisks",
                table: "StatementRiskCountries");

            migrationBuilder.DropColumn(
                name: "ApproverFirstName",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "ApproverJobTitle",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "ApproverLastName",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherRelevantRisks",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "OtherTraining",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "StatementLocationRiskId",
                table: "StatementRiskCountries");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Statements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FoundModernSlaveryInOperations",
                table: "Statements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IdentifiedForcedLabour",
                table: "Statements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Statements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Statements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherRelavantRisks",
                table: "Statements",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherTrainingTypes",
                table: "Statements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "StatementRiskCountryId",
                table: "StatementRiskCountries",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.StatementRiskCountry",
                table: "StatementRiskCountries",
                columns: new[] { "StatementRiskTypeId", "StatementId" });

            migrationBuilder.AddForeignKey(
                name: "FK_StatementRiskCountries_Statements_StatementId",
                table: "StatementRiskCountries",
                column: "StatementId",
                principalTable: "Statements",
                principalColumn: "StatementId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StatementRiskCountries_StatementRiskTypes_StatementRiskTypeId",
                table: "StatementRiskCountries",
                column: "StatementRiskTypeId",
                principalTable: "StatementRiskTypes",
                principalColumn: "StatementRiskTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
