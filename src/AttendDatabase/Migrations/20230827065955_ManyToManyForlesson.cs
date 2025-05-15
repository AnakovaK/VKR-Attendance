using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyForlesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lesson_visiting_log_fk_visiting_log_id",
                table: "lesson");

            migrationBuilder.DropIndex(
                name: "IX_teachers_lessons_fk_teacher_id",
                table: "teachers_lessons");

            migrationBuilder.DropIndex(
                name: "IX_lesson_fk_visiting_log_id",
                table: "lesson");

            migrationBuilder.DropColumn(
                name: "auditorium",
                table: "lesson");

            migrationBuilder.DropColumn(
                name: "fk_visiting_log_id",
                table: "lesson");

            migrationBuilder.CreateTable(
                name: "Auditorium",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "text", nullable: false),
                    Campus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorium", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "visiting_logs_lessons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_visiting_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_lesson_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visiting_logs_lessons", x => x.id);
                    table.ForeignKey(
                        name: "lessons_fk_visiting_logs_foreign",
                        column: x => x.fk_visiting_log_id,
                        principalTable: "visiting_log",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "visiting_logs_fk_lessons_foreign",
                        column: x => x.fk_lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auditoriums_lessons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_auditorium_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_lesson_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoriums_lessons", x => x.id);
                    table.ForeignKey(
                        name: "auditoriums_fk_lessons_foreign",
                        column: x => x.fk_lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "lessons_fk_auditoriums_foreign",
                        column: x => x.fk_auditorium_id,
                        principalTable: "Auditorium",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_teachers_lessons_fk_teacher_id_fk_lesson_id",
                table: "teachers_lessons",
                columns: new[] { "fk_teacher_id", "fk_lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auditoriums_lessons_fk_auditorium_id_fk_lesson_id",
                table: "auditoriums_lessons",
                columns: new[] { "fk_auditorium_id", "fk_lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_auditoriums_lessons_fk_lesson_id",
                table: "auditoriums_lessons",
                column: "fk_lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_visiting_logs_lessons_fk_lesson_id",
                table: "visiting_logs_lessons",
                column: "fk_lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_visiting_logs_lessons_fk_visiting_log_id_fk_lesson_id",
                table: "visiting_logs_lessons",
                columns: new[] { "fk_visiting_log_id", "fk_lesson_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoriums_lessons");

            migrationBuilder.DropTable(
                name: "visiting_logs_lessons");

            migrationBuilder.DropTable(
                name: "Auditorium");

            migrationBuilder.DropIndex(
                name: "IX_teachers_lessons_fk_teacher_id_fk_lesson_id",
                table: "teachers_lessons");

            migrationBuilder.AddColumn<string>(
                name: "auditorium",
                table: "lesson",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "fk_visiting_log_id",
                table: "lesson",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_teachers_lessons_fk_teacher_id",
                table: "teachers_lessons",
                column: "fk_teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_fk_visiting_log_id",
                table: "lesson",
                column: "fk_visiting_log_id");

            migrationBuilder.AddForeignKey(
                name: "FK_lesson_visiting_log_fk_visiting_log_id",
                table: "lesson",
                column: "fk_visiting_log_id",
                principalTable: "visiting_log",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
