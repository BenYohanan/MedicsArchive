using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class modifiedRepoer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Reports",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reports");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
