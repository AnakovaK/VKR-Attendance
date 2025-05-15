using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyTeacherLessomANDIsValidatedLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_validated",
                table: "lesson",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "teachers_lessons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_lesson_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teachers_lessons", x => x.id);
                    table.ForeignKey(
                        name: "lessons_fk_teachers_foreign",
                        column: x => x.fk_lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "teachers_fk_lessons_foreign",
                        column: x => x.fk_teacher_id,
                        principalTable: "teacher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_teachers_lessons_fk_lesson_id",
                table: "teachers_lessons",
                column: "fk_lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_teachers_lessons_fk_teacher_id",
                table: "teachers_lessons",
                column: "fk_teacher_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "teachers_lessons");

            migrationBuilder.DropColumn(
                name: "is_validated",
                table: "lesson");
        }
    }
}
