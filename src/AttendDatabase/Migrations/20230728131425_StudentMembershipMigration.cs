using System;
using Microsoft.EntityFrameworkCore.Migrations;
using RTUAttendAPI.AttendDatabase.Models;

#nullable disable

namespace AttendDatabase.Migrations
{
    /// <inheritdoc />
    public partial class StudentMembershipMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .Annotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive");

            migrationBuilder.CreateTable(
                name: "student_membership",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nsi_student_id = table.Column<Guid>(type: "uuid", nullable: true),
                    visiting_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membership_type = table.Column<StudentMembershipType>(type: "student_membership_type", nullable: false),
                    membership_role = table.Column<StudentMembershipRole>(type: "student_membership_role", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("student_membership_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_membership_visiting_log_visiting_log_id",
                        column: x => x.visiting_log_id,
                        principalTable: "visiting_log",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_student_membership_visiting_log_id",
                table: "student_membership",
                column: "visiting_log_id");

            migrationBuilder.CreateIndex(
                name: "student_membership_student_in_group_once",
                table: "student_membership",
                columns: new[] { "nsi_student_id", "visiting_log_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_membership");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:student_membership_role", "unknown,student,elder,vice_elder")
                .OldAnnotation("Npgsql:Enum:student_membership_type", "unknown,active,inactive");
        }
    }
}
