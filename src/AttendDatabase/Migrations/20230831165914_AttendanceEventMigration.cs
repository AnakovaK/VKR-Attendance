using System;
using Microsoft.EntityFrameworkCore.Migrations;
using RTUAttendAPI.AttendDatabase.Models;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceEventMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attendance_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_human_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    attend_type = table.Column<AttendType>(type: "attend_type", nullable: false),
                    author_type = table.Column<AuthorType>(type: "author_type", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_attendance_events_attendance_attendance_id",
                        column: x => x.attendance_id,
                        principalTable: "attendance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_attendance_events_nsi_human_author_human_id",
                        column: x => x.author_human_id,
                        principalTable: "nsi_human",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_attendance_id",
                table: "attendance_events",
                column: "attendance_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_events_author_human_id",
                table: "attendance_events",
                column: "author_human_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_events");
        }
    }
}
