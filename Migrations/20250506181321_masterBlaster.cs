using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace userPanelOMR.Migrations
{
    /// <inheritdoc />
    public partial class masterBlaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "singUps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role",
                table: "singUps");
        }
    }
}
