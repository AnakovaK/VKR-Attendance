using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class HumanLoginMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nsi_login_id",
                columns: table => new
                {
                    login_id = table.Column<Guid>(type: "uuid", nullable: false),
                    human_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nsi_login_id", x => x.login_id);
                    table.ForeignKey(
                        name: "nsi_login_id_fk_nsi_human_foreign",
                        column: x => x.human_id,
                        principalTable: "nsi_human",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nsi_login_id_human_id",
                table: "nsi_login_id",
                column: "human_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nsi_login_id");
        }
    }
}
