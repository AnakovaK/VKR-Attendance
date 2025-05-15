using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class DisciplineToVisitLogUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisciplineVisitingLog");

            migrationBuilder.CreateTable(
                name: "discipline_visiting_log",
                columns: table => new
                {
                    discipline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visiting_logs_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discipline_visiting_log", x => new { x.discipline_id, x.visiting_logs_id });
                    table.ForeignKey(
                        name: "FK_discipline_visiting_log_discipline_discipline_id",
                        column: x => x.discipline_id,
                        principalTable: "discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_discipline_visiting_log_visiting_log_visiting_logs_id",
                        column: x => x.visiting_logs_id,
                        principalTable: "visiting_log",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discipline_visiting_log_visiting_logs_id",
                table: "discipline_visiting_log",
                column: "visiting_logs_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discipline_visiting_log");

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
    }
}
