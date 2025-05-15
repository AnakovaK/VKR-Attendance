using System;
using Microsoft.EntityFrameworkCore.Migrations;
using RTUAttendAPI.AttendDatabase.Models;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class NewAttendanceAddedWithEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attend_type", "unknown,absent,excused_absence,present")
                .Annotation("Npgsql:Enum:author_type", "unknown,elder,student,teacher")
                .Annotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .Annotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive")
                .OldAnnotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .OldAnnotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive");

            migrationBuilder.CreateTable(
                name: "attendance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_lesson_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_type = table.Column<AuthorType>(type: "author_type", nullable: false),
                    attend_type = table.Column<AttendType>(type: "attend_type", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance", x => x.id);
                    table.ForeignKey(
                        name: "FK_attendance_lesson_fk_lesson_id",
                        column: x => x.fk_lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_attendance_nsi_student_fk_student_id",
                        column: x => x.fk_student_id,
                        principalTable: "nsi_student",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_attendance_teacher_fk_teacher_id",
                        column: x => x.fk_teacher_id,
                        principalTable: "teacher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_fk_lesson_id",
                table: "attendance",
                column: "fk_lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_fk_student_id",
                table: "attendance",
                column: "fk_student_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_fk_teacher_id",
                table: "attendance",
                column: "fk_teacher_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .Annotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive")
                .OldAnnotation("Npgsql:Enum:attend_type", "unknown,absent,excused_absence,present")
                .OldAnnotation("Npgsql:Enum:author_type", "unknown,elder,student,teacher")
                .OldAnnotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .OldAnnotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive");
        }
    }
}
