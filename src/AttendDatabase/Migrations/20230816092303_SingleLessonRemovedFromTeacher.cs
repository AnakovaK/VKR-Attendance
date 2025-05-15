using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class SingleLessonRemovedFromTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teacher_lesson_LessonId",
                table: "teacher");

            migrationBuilder.DropIndex(
                name: "IX_teacher_LessonId",
                table: "teacher");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "teacher");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LessonId",
                table: "teacher",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_teacher_LessonId",
                table: "teacher",
                column: "LessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_lesson_LessonId",
                table: "teacher",
                column: "LessonId",
                principalTable: "lesson",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
