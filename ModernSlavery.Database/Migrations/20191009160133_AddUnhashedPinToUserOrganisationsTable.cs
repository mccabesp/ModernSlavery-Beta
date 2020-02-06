using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Database.Core21.Migrations
{
    public partial class AddUnhashedPinToUserOrganisationsTable : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                "PIN",
                "UserOrganisations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "PIN",
                "UserOrganisations");
        }

    }
}
