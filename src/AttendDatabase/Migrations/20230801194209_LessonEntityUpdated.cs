using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class LessonEntityUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_discipline_DisciplineId",
                table: "Lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_Lesson_visiting_log_VisitingLogId",
                table: "Lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_teacher_Lesson_LessonId",
                table: "teacher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lesson",
                table: "Lesson");

            migrationBuilder.RenameTable(
                name: "Lesson",
                newName: "lesson");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "lesson",
                newName: "start");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "lesson",
                newName: "end");

            migrationBuilder.RenameColumn(
                name: "Auditorium",
                table: "lesson",
                newName: "auditorium");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "lesson",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VisitingLogId",
                table: "lesson",
                newName: "fk_visiting_log_id");

            migrationBuilder.RenameColumn(
                name: "DisciplineId",
                table: "lesson",
                newName: "fk_discipline_id");

            migrationBuilder.RenameIndex(
                name: "IX_Lesson_VisitingLogId",
                table: "lesson",
                newName: "IX_lesson_fk_visiting_log_id");

            migrationBuilder.RenameIndex(
                name: "IX_Lesson_DisciplineId",
                table: "lesson",
                newName: "IX_lesson_fk_discipline_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_lesson",
                table: "lesson",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_lesson_discipline_fk_discipline_id",
                table: "lesson",
                column: "fk_discipline_id",
                principalTable: "discipline",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lesson_visiting_log_fk_visiting_log_id",
                table: "lesson",
                column: "fk_visiting_log_id",
                principalTable: "visiting_log",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_lesson_LessonId",
                table: "teacher",
                column: "LessonId",
                principalTable: "lesson",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lesson_discipline_fk_discipline_id",
                table: "lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_lesson_visiting_log_fk_visiting_log_id",
                table: "lesson");

            migrationBuilder.DropForeignKey(
                name: "FK_teacher_lesson_LessonId",
                table: "teacher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_lesson",
                table: "lesson");

            migrationBuilder.RenameTable(
                name: "lesson",
                newName: "Lesson");

            migrationBuilder.RenameColumn(
                name: "start",
                table: "Lesson",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "end",
                table: "Lesson",
                newName: "End");

            migrationBuilder.RenameColumn(
                name: "auditorium",
                table: "Lesson",
                newName: "Auditorium");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Lesson",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "fk_visiting_log_id",
                table: "Lesson",
                newName: "VisitingLogId");

            migrationBuilder.RenameColumn(
                name: "fk_discipline_id",
                table: "Lesson",
                newName: "DisciplineId");

            migrationBuilder.RenameIndex(
                name: "IX_lesson_fk_visiting_log_id",
                table: "Lesson",
                newName: "IX_Lesson_VisitingLogId");

            migrationBuilder.RenameIndex(
                name: "IX_lesson_fk_discipline_id",
                table: "Lesson",
                newName: "IX_Lesson_DisciplineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lesson",
                table: "Lesson",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_discipline_DisciplineId",
                table: "Lesson",
                column: "DisciplineId",
                principalTable: "discipline",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_visiting_log_VisitingLogId",
                table: "Lesson",
                column: "VisitingLogId",
                principalTable: "visiting_log",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_Lesson_LessonId",
                table: "teacher",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
