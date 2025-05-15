using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleSourceIdForLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_from_tandem_event_id",
                table: "lesson",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_from_tandem_event_id",
                table: "lesson");
        }
    }
}
