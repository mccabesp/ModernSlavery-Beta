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

            migrationBuilder.RenameColumn(
                name: "EmployerReference",
                newName: "OrganisationReference",
                table: "Organisations");

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

            migrationBuilder.RenameColumn(
                name: "OrganisationReference",
                newName: "EmployerReference",
                table: "Organisations");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_EmployerReference",
                table: "Organisations",
                column: "EmployerReference",
                unique: true,
                filter: "([EmployerReference] IS NOT NULL)");
        }
    }
}
