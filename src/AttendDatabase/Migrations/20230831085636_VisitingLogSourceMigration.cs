using Microsoft.EntityFrameworkCore.Migrations;
using RTUAttendAPI.AttendDatabase.Models;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class VisitingLogSourceMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attend_type", "unknown,absent,excused_absence,present")
                .Annotation("Npgsql:Enum:author_type", "unknown,elder,student,teacher")
                .Annotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .Annotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive")
                .Annotation("Npgsql:Enum:visiting_log_source", "unknown,schedule,manual")
                .OldAnnotation("Npgsql:Enum:attend_type", "unknown,absent,excused_absence,present")
                .OldAnnotation("Npgsql:Enum:author_type", "unknown,elder,student,teacher")
                .OldAnnotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .OldAnnotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive");

            migrationBuilder.AddColumn<VisitingLogSource>(
                name: "source",
                table: "visiting_log",
                type: "visiting_log_source",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source",
                table: "visiting_log");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attend_type", "unknown,absent,excused_absence,present")
                .Annotation("Npgsql:Enum:author_type", "unknown,elder,student,teacher")
                .Annotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .Annotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive")
                .OldAnnotation("Npgsql:Enum:attend_type", "unknown,absent,excused_absence,present")
                .OldAnnotation("Npgsql:Enum:author_type", "unknown,elder,student,teacher")
                .OldAnnotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .OldAnnotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive")
                .OldAnnotation("Npgsql:Enum:visiting_log_source", "unknown,schedule,manual");
        }
    }
}
