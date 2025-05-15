using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class LessonEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LessonId",
                table: "teacher",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Lesson",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitingLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    Auditorium = table.Column<string>(type: "text", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lesson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lesson_discipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalTable: "discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lesson_visiting_log_VisitingLogId",
                        column: x => x.VisitingLogId,
                        principalTable: "visiting_log",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_teacher_LessonId",
                table: "teacher",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_DisciplineId",
                table: "Lesson",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_VisitingLogId",
                table: "Lesson",
                column: "VisitingLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_Lesson_LessonId",
                table: "teacher",
                column: "LessonId",
                principalTable: "Lesson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teacher_Lesson_LessonId",
                table: "teacher");

            migrationBuilder.DropTable(
                name: "Lesson");

            migrationBuilder.DropIndex(
                name: "IX_teacher_LessonId",
                table: "teacher");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "teacher");
        }
    }
}
