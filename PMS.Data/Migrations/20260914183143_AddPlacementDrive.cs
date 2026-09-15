using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlacementDrive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementDrive_Companies_CompanyId",
                table: "PlacementDrive");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlacementDrive",
                table: "PlacementDrive");

            migrationBuilder.RenameTable(
                name: "PlacementDrive",
                newName: "PlacementDrives");

            migrationBuilder.RenameColumn(
                name: "MinimumCGPA",
                table: "PlacementDrives",
                newName: "MinimumCgpa");

            migrationBuilder.RenameIndex(
                name: "IX_PlacementDrive_CompanyId",
                table: "PlacementDrives",
                newName: "IX_PlacementDrives_CompanyId");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredSkills",
                table: "PlacementDrives",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumCgpa",
                table: "PlacementDrives",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "PlacementDrives",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "PlacementDrives",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "JobDescription",
                table: "PlacementDrives",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "PlacementDrives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "PlacementDrives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "PlacementDrives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "PlacementDrives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PlacementDrives",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PlacementDrives",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "PlacementDrives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "PlacementDrives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlacementDrives",
                table: "PlacementDrives",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PlacementDriveDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlacementDriveId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementDriveDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacementDriveDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementDriveDepartments_PlacementDrives_PlacementDriveId",
                        column: x => x.PlacementDriveId,
                        principalTable: "PlacementDrives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementDriveDepartments_DepartmentId",
                table: "PlacementDriveDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementDriveDepartments_PlacementDriveId_DepartmentId",
                table: "PlacementDriveDepartments",
                columns: new[] { "PlacementDriveId", "DepartmentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementDrives_Companies_CompanyId",
                table: "PlacementDrives",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementDrives_Companies_CompanyId",
                table: "PlacementDrives");

            migrationBuilder.DropTable(
                name: "PlacementDriveDepartments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlacementDrives",
                table: "PlacementDrives");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PlacementDrives");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "PlacementDrives");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "PlacementDrives");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "PlacementDrives");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PlacementDrives");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PlacementDrives");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PlacementDrives");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "PlacementDrives");

            migrationBuilder.RenameTable(
                name: "PlacementDrives",
                newName: "PlacementDrive");

            migrationBuilder.RenameColumn(
                name: "MinimumCgpa",
                table: "PlacementDrive",
                newName: "MinimumCGPA");

            migrationBuilder.RenameIndex(
                name: "IX_PlacementDrives_CompanyId",
                table: "PlacementDrive",
                newName: "IX_PlacementDrive_CompanyId");

            migrationBuilder.AlterColumn<string>(
                name: "RequiredSkills",
                table: "PlacementDrive",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumCGPA",
                table: "PlacementDrive",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldPrecision: 4,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "PlacementDrive",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "PlacementDrive",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "JobDescription",
                table: "PlacementDrive",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlacementDrive",
                table: "PlacementDrive",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementDrive_Companies_CompanyId",
                table: "PlacementDrive",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
