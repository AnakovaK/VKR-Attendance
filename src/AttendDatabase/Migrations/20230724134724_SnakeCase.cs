using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Start",
                table: "semester",
                newName: "start");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "semester",
                newName: "end");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "start",
                table: "semester",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "end",
                table: "semester",
                newName: "End");
        }
    }
}
