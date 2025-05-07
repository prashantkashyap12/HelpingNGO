using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace userPanelOMR.Migrations
{
    /// <inheritdoc />
    public partial class testOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DynamicForm",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UCont = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UAdd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Furl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicForm", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeesTab",
                columns: table => new
                {
                    EmpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bankName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeesTab", x => x.EmpId);
                });

            migrationBuilder.CreateTable(
                name: "singUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sContact = table.Column<int>(type: "int", nullable: false),
                    sOtp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_singUps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersTab",
                columns: table => new
                {
                    CstId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    customerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    donateAmt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersTab", x => x.CstId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicForm");

            migrationBuilder.DropTable(
                name: "EmployeesTab");

            migrationBuilder.DropTable(
                name: "singUps");

            migrationBuilder.DropTable(
                name: "UsersTab");
        }
    }
}
