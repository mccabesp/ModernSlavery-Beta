using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class UpdateEmployerReferenceToOrganisationReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_EmployerReference",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "EmployerReference",
                table: "Organisations");

            migrationBuilder.AddColumn<string>(
                name: "OrganisationReference",
                table: "Organisations",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_OrganisationReference",
                table: "Organisations",
                column: "OrganisationReference",
                unique: true,
                filter: "([OrganisationReference] IS NOT NULL)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_OrganisationReference",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "OrganisationReference",
                table: "Organisations");

            migrationBuilder.AddColumn<string>(
                name: "EmployerReference",
                table: "Organisations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_EmployerReference",
                table: "Organisations",
                column: "EmployerReference",
                unique: true,
                filter: "([EmployerReference] IS NOT NULL)");
        }
    }
}
