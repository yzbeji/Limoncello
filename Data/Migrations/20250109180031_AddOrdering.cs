using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Limoncello.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "TaskColumns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "ProjectTasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "TaskColumns");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "ProjectTasks");
        }
    }
}
