using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class LinkedReminderEmailsToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "ReminderEmails",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ReminderEmails_UserId",
                table: "ReminderEmails",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderEmails_Users_UserId",
                table: "ReminderEmails",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReminderEmails_Users_UserId",
                table: "ReminderEmails");

            migrationBuilder.DropIndex(
                name: "IX_ReminderEmails_UserId",
                table: "ReminderEmails");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "ReminderEmails");
        }
    }
}
