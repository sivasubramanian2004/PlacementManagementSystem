using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRoleEnumToUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        UPDATE [Users]
        SET [Role] =
            CASE
                WHEN LOWER([Role]) = 'student' THEN '1'
                WHEN LOWER([Role]) = 'admin' THEN '2'
                WHEN LOWER([Role]) = 'placementofficer' THEN '3'
                WHEN LOWER([Role]) = 'recruiter' THEN '4'
                ELSE '1'
            END
        """);

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "student");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        ALTER TABLE [Users]
        ADD [RoleTemp] nvarchar(100) NOT NULL
            CONSTRAINT [DF_Users_RoleTemp] DEFAULT 'student';

        UPDATE [Users]
        SET [RoleTemp] =
            CASE [Role]
                WHEN 1 THEN 'student'
                WHEN 2 THEN 'admin'
                WHEN 3 THEN 'placementofficer'
                WHEN 4 THEN 'recruiter'
                ELSE 'student'
            END;

        ALTER TABLE [Users] DROP COLUMN [Role];

        EXEC sp_rename 'Users.RoleTemp', 'Role', 'COLUMN';
        """);
        }
    }
}
