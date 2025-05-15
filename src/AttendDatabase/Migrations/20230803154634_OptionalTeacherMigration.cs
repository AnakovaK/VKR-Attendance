using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class OptionalTeacherMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attendance_teacher_fk_teacher_id",
                table: "attendance");

            migrationBuilder.DropIndex(
                name: "IX_attendance_fk_lesson_id",
                table: "attendance");

            migrationBuilder.AlterColumn<Guid>(
                name: "fk_teacher_id",
                table: "attendance",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_fk_lesson_id_fk_student_id",
                table: "attendance",
                columns: new[] { "fk_lesson_id", "fk_student_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_teacher_fk_teacher_id",
                table: "attendance",
                column: "fk_teacher_id",
                principalTable: "teacher",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attendance_teacher_fk_teacher_id",
                table: "attendance");

            migrationBuilder.DropIndex(
                name: "IX_attendance_fk_lesson_id_fk_student_id",
                table: "attendance");

            migrationBuilder.AlterColumn<Guid>(
                name: "fk_teacher_id",
                table: "attendance",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_fk_lesson_id",
                table: "attendance",
                column: "fk_lesson_id");

            migrationBuilder.AddForeignKey(
                name: "FK_attendance_teacher_fk_teacher_id",
                table: "attendance",
                column: "fk_teacher_id",
                principalTable: "teacher",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
