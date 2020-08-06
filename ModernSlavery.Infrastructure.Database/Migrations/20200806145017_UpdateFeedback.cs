using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class UpdateFeedback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionsToCloseGpg",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "Charity",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "CloseOrganisationGpg",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "CompanyIntranet",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "EmployeeInterestedInOrganisationData",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "EmployerUnion",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "FindOutAboutGpg",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "InternetSearch",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "LobbyGroup",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "ManagerInvolvedInGpgReport",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "NewsArticle",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "OtherPerson",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "OtherPersonText",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "OtherReason",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "OtherReasonText",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "OtherSource",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "OtherSourceText",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "PersonInterestedInGeneralGpg",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "PersonInterestedInSpecificOrganisationGpg",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "Report",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "ReportOrganisationGpgData",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "ResponsibleForReportingGpg",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "SocialMedia",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "ViewSpecificOrganisationGpg",
                table: "Feedback");

            migrationBuilder.AddColumn<byte>(
                name: "WhyVisitMSUSiteId",
                table: "Feedback",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhyVisitMSUSiteId",
                table: "Feedback");

            migrationBuilder.AddColumn<bool>(
                name: "ActionsToCloseGpg",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Charity",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CloseOrganisationGpg",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CompanyIntranet",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmployeeInterestedInOrganisationData",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmployerUnion",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FindOutAboutGpg",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InternetSearch",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LobbyGroup",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ManagerInvolvedInGpgReport",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NewsArticle",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OtherPerson",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPersonText",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OtherReason",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherReasonText",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OtherSource",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherSourceText",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PersonInterestedInGeneralGpg",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PersonInterestedInSpecificOrganisationGpg",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Report",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReportOrganisationGpgData",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ResponsibleForReportingGpg",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SocialMedia",
                table: "Feedback",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ViewSpecificOrganisationGpg",
                table: "Feedback",
                type: "bit",
                nullable: true);
        }
    }
}
