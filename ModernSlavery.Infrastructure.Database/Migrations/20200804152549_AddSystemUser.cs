using Microsoft.EntityFrameworkCore.Migrations;

namespace ModernSlavery.Infrastructure.Database.Migrations
{
    public partial class AddSystemUser : Migration
    {
        public const string AddSystemUserSql="SET IDENTITY_INSERT [dbo].[Users] ON; INSERT INTO[dbo].[Users]([UserId], [JobTitle], [Firstname], [Lastname], [EmailAddress], [PasswordHash], [HashingAlgorithmId], [StatusId], [StatusDate], [LoginAttempts], [ResetAttempts], [VerifyAttempts], [Created], [Modified]) VALUES(-1, N'SYSTEM', N'SYSTEM', N'SYSTEM', N'SYSTEM', N'SYSTEM', 0, 0, GetDate(), 0, 0, 0, GetDate(), GetDate()); SET IDENTITY_INSERT[dbo].[Users] OFF";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AddSystemUserSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE [dbo].[Users] WHERE UserID=-1");
        }
    }
}
