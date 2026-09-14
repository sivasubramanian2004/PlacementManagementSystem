using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IndustryType = table.Column<int>(type: "int", nullable: false),
                    OtherIndustry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlacementDrive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmploymentType = table.Column<int>(type: "int", nullable: false),
                    WorkMode = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinimumCGPA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumBacklogs = table.Column<int>(type: "int", nullable: false),
                    GraduationYear = table.Column<int>(type: "int", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredSkills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DriveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementDrive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacementDrive_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlacementDrive_CompanyId",
                table: "PlacementDrive",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlacementDrive");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments");
        }
    }
}
