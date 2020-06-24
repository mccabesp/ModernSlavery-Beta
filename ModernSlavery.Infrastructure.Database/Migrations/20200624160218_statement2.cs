using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class statement2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SnapshotDate",
                newName: "SubmissionDeadline",
                table: "OrganisationScopes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubmissionDeadline",
                newName: "SubmissionDeadline",
                table: "OrganisationScopes");
        }
    }
}
