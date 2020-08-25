using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class Add_OrganisationScope : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactJobTitle",
                table: "OrganisationScopes",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TurnOver",
                table: "OrganisationScopes",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactJobTitle",
                table: "OrganisationScopes");

            migrationBuilder.DropColumn(
                name: "TurnOver",
                table: "OrganisationScopes");
        }
    }
}
