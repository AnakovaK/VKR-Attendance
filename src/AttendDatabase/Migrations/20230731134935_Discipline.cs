using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class Discipline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discipline",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discipline", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "DisciplineVisitingLog",
                columns: table => new
                {
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitingLogsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisciplineVisitingLog", x => new { x.DisciplineId, x.VisitingLogsId });
                    table.ForeignKey(
                        name: "FK_DisciplineVisitingLog_discipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalTable: "discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisciplineVisitingLog_visiting_log_VisitingLogsId",
                        column: x => x.VisitingLogsId,
                        principalTable: "visiting_log",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisciplineVisitingLog_VisitingLogsId",
                table: "DisciplineVisitingLog",
                column: "VisitingLogsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisciplineVisitingLog");

            migrationBuilder.DropTable(
                name: "discipline");
        }
    }
}
