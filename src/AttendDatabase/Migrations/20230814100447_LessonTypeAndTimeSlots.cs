using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class LessonTypeAndTimeSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "auditorium",
                table: "lesson",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "fk_lesson_type_id",
                table: "lesson",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "lesson_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_type_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lesson_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "time_slot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_minutes_utc = table.Column<int>(type: "integer", nullable: false),
                    end_minutes_utc = table.Column<int>(type: "integer", nullable: false),
                    fk_semester_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_slot", x => x.id);
                    table.ForeignKey(
                        name: "FK_time_slot_semester_fk_semester_id",
                        column: x => x.fk_semester_id,
                        principalTable: "semester",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lesson_fk_lesson_type_id",
                table: "lesson",
                column: "fk_lesson_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_time_slot_fk_semester_id",
                table: "time_slot",
                column: "fk_semester_id");

            migrationBuilder.AddForeignKey(
                name: "FK_lesson_lesson_type_fk_lesson_type_id",
                table: "lesson",
                column: "fk_lesson_type_id",
                principalTable: "lesson_type",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lesson_lesson_type_fk_lesson_type_id",
                table: "lesson");

            migrationBuilder.DropTable(
                name: "lesson_type");

            migrationBuilder.DropTable(
                name: "time_slot");

            migrationBuilder.DropIndex(
                name: "IX_lesson_fk_lesson_type_id",
                table: "lesson");

            migrationBuilder.DropColumn(
                name: "fk_lesson_type_id",
                table: "lesson");

            migrationBuilder.AlterColumn<string>(
                name: "auditorium",
                table: "lesson",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
