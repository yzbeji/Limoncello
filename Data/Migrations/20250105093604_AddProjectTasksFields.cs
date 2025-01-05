using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Limoncello.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTasksFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ProjectTasks",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ProjectTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                table: "ProjectTasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "ProjectTasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ProjectTasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProjectTasks");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "ProjectTasks",
                newName: "Name");
        }
    }
}
