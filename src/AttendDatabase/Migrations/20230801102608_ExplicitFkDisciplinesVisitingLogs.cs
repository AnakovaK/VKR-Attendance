using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class ExplicitFkDisciplinesVisitingLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discipline_visiting_log");

            migrationBuilder.CreateTable(
                name: "disciplines_visiting_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_discipline_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_visiting_log_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disciplines_visiting_logs", x => x.id);
                    table.ForeignKey(
                        name: "disciplines_fk_visiting_logs_foreign",
                        column: x => x.fk_discipline_id,
                        principalTable: "discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "visiting_logs_fk_discilplines_logs_foreign",
                        column: x => x.fk_visiting_log_id,
                        principalTable: "visiting_log",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_disciplines_visiting_logs_fk_discipline_id",
                table: "disciplines_visiting_logs",
                column: "fk_discipline_id");

            migrationBuilder.CreateIndex(
                name: "IX_disciplines_visiting_logs_fk_visiting_log_id",
                table: "disciplines_visiting_logs",
                column: "fk_visiting_log_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "disciplines_visiting_logs");

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
    }
}
