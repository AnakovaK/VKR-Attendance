using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class PeopleAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "nsi_student_id",
                table: "student_membership",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "nsi_human",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lastname = table.Column<string>(type: "text", nullable: false),
                    firstname = table.Column<string>(type: "text", nullable: false),
                    middlename = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nsi_human", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nsi_student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_nsi_human_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nsi_student", x => x.id);
                    table.ForeignKey(
                        name: "FK_nsi_student_nsi_human_fk_nsi_human_id",
                        column: x => x.fk_nsi_human_id,
                        principalTable: "nsi_human",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teacher",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fk_nsi_human_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher", x => x.id);
                    table.ForeignKey(
                        name: "FK_teacher_nsi_human_fk_nsi_human_id",
                        column: x => x.fk_nsi_human_id,
                        principalTable: "nsi_human",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nsi_student_fk_nsi_human_id",
                table: "nsi_student",
                column: "fk_nsi_human_id");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_fk_nsi_human_id",
                table: "teacher",
                column: "fk_nsi_human_id");

            migrationBuilder.AddForeignKey(
                name: "FK_student_membership_nsi_student_nsi_student_id",
                table: "student_membership",
                column: "nsi_student_id",
                principalTable: "nsi_student",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_student_membership_nsi_student_nsi_student_id",
                table: "student_membership");

            migrationBuilder.DropTable(
                name: "nsi_student");

            migrationBuilder.DropTable(
                name: "teacher");

            migrationBuilder.DropTable(
                name: "nsi_human");

            migrationBuilder.AlterColumn<Guid>(
                name: "nsi_student_id",
                table: "student_membership",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
