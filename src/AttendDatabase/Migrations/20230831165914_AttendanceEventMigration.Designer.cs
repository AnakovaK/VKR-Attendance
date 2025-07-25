﻿// <auto-generated />
using System;
using AttendDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RTUAttendAPI.AttendDatabase.Models;

#nullable disable

namespace AttendDatabase.Migrations
{
    [DbContext(typeof(AttendDbContext))]
    [Migration("20230831165914_AttendanceEventMigration")]
    partial class AttendanceEventMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "attend_type", new[] { "unknown", "absent", "excused_absence", "present" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "author_type", new[] { "unknown", "elder", "student", "teacher" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "student_membership_role", new[] { "unknown", "student", "elder", "vice_elder" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "student_membership_type", new[] { "unknown", "active", "inactive" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "visiting_log_source", new[] { "unknown", "schedule", "manual" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Attendance", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<AttendType>("Attend")
                        .HasColumnType("attend_type")
                        .HasColumnName("attend_type");

                    b.Property<AuthorType>("Author")
                        .HasColumnType("author_type")
                        .HasColumnName("author_type");

                    b.Property<Guid>("LessonId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_lesson_id");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_student_id");

                    b.Property<Guid?>("TeacherId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_teacher_id");

                    b.HasKey("Id");

                    b.HasIndex("StudentId");

                    b.HasIndex("TeacherId");

                    b.HasIndex("LessonId", "StudentId")
                        .IsUnique();

                    b.ToTable("attendance", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.AttendanceEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<AttendType>("AttendType")
                        .HasColumnType("attend_type")
                        .HasColumnName("attend_type");

                    b.Property<Guid>("AttendanceId")
                        .HasColumnType("uuid")
                        .HasColumnName("attendance_id");

                    b.Property<AuthorType>("Author")
                        .HasColumnType("author_type")
                        .HasColumnName("author_type");

                    b.Property<Guid>("AuthorHumanId")
                        .HasColumnType("uuid")
                        .HasColumnName("author_human_id");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date");

                    b.HasKey("Id");

                    b.HasIndex("AttendanceId");

                    b.HasIndex("AuthorHumanId");

                    b.ToTable("attendance_events", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Auditorium", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Campus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Auditorium", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.AuditoriumLesson", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("AuditoriumId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_auditorium_id");

                    b.Property<Guid>("LessonId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_lesson_id");

                    b.HasKey("Id");

                    b.HasIndex("LessonId");

                    b.HasIndex("AuditoriumId", "LessonId")
                        .IsUnique();

                    b.ToTable("auditoriums_lessons", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Discipline", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("Id");

                    b.HasIndex("Title")
                        .IsUnique();

                    b.ToTable("discipline", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.DisciplineVisitingLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("FkDisciplineId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_discipline_id");

                    b.Property<Guid>("FkVisitingLogId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_visiting_log_id");

                    b.HasKey("Id");

                    b.HasIndex("FkDisciplineId");

                    b.HasIndex("FkVisitingLogId");

                    b.ToTable("disciplines_visiting_logs", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Lesson", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("CreatedFromScheduleEventId")
                        .HasColumnType("uuid")
                        .HasColumnName("created_from_tandem_event_id");

                    b.Property<Guid>("DisciplineId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_discipline_id");

                    b.Property<DateTimeOffset>("End")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end");

                    b.Property<bool>("IsValidated")
                        .HasColumnType("boolean")
                        .HasColumnName("is_validated");

                    b.Property<Guid?>("LessonTypeId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_lesson_type_id");

                    b.Property<DateTimeOffset>("Start")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start");

                    b.HasKey("Id");

                    b.HasIndex("DisciplineId");

                    b.HasIndex("LessonTypeId");

                    b.ToTable("lesson", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.LessonType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("LessonTypeName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("lesson_type_name");

                    b.HasKey("Id");

                    b.ToTable("lesson_type", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.NsiHuman", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("firstname");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("lastname");

                    b.Property<string>("Middlename")
                        .HasColumnType("text")
                        .HasColumnName("middlename");

                    b.HasKey("Id");

                    b.ToTable("nsi_human", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.NsiHumanLoginId", b =>
                {
                    b.Property<Guid>("LoginId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("login_id");

                    b.Property<Guid>("HumanId")
                        .HasColumnType("uuid")
                        .HasColumnName("human_id");

                    b.HasKey("LoginId");

                    b.HasIndex("HumanId");

                    b.ToTable("nsi_login_id", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.NsiStudent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("NsiHumanId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_nsi_human_id");

                    b.Property<string>("PersonalNumber")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("personal_number");

                    b.HasKey("Id");

                    b.HasIndex("NsiHumanId");

                    b.HasIndex("PersonalNumber")
                        .IsUnique();

                    b.ToTable("nsi_student", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Semester", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("End")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end");

                    b.Property<DateTimeOffset>("Start")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("semester_pkey");

                    b.HasIndex("Title")
                        .IsUnique();

                    b.ToTable("semester", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.StudentMembership", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<StudentMembershipRole>("MembershipRole")
                        .HasColumnType("student_membership_role")
                        .HasColumnName("membership_role");

                    b.Property<StudentMembershipType>("MembershipType")
                        .HasColumnType("student_membership_type")
                        .HasColumnName("membership_type");

                    b.Property<Guid>("NsiStudentId")
                        .HasColumnType("uuid")
                        .HasColumnName("nsi_student_id");

                    b.Property<Guid>("VisitingLogId")
                        .HasColumnType("uuid")
                        .HasColumnName("visiting_log_id");

                    b.HasKey("Id")
                        .HasName("student_membership_pkey");

                    b.HasIndex("VisitingLogId");

                    b.HasIndex("NsiStudentId", "VisitingLogId")
                        .IsUnique()
                        .HasDatabaseName("student_membership_student_in_group_once");

                    b.ToTable("student_membership", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Teacher", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("NsiHumanId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_nsi_human_id");

                    b.HasKey("Id");

                    b.HasIndex("NsiHumanId");

                    b.ToTable("teacher", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.TeacherLesson", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("LessonId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_lesson_id");

                    b.Property<Guid>("TeacherId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_teacher_id");

                    b.HasKey("Id");

                    b.HasIndex("LessonId");

                    b.HasIndex("TeacherId", "LessonId")
                        .IsUnique();

                    b.ToTable("teachers_lessons", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.TimeSlot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("EndMinutesUTC")
                        .HasColumnType("integer")
                        .HasColumnName("end_minutes_utc");

                    b.Property<Guid>("SemesterId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_semester_id");

                    b.Property<int>("StartMinutesUTC")
                        .HasColumnType("integer")
                        .HasColumnName("start_minutes_utc");

                    b.HasKey("Id");

                    b.HasIndex("SemesterId");

                    b.ToTable("time_slot", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.VisitingLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("boolean")
                        .HasColumnName("is_archived");

                    b.Property<Guid>("SemesterId")
                        .HasColumnType("uuid")
                        .HasColumnName("semester_id");

                    b.Property<VisitingLogSource?>("Source")
                        .HasColumnType("visiting_log_source")
                        .HasColumnName("source");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("visiting_log_pkey");

                    b.HasIndex("SemesterId");

                    b.ToTable("visiting_log", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.VisitingLogLesson", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("LessonId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_lesson_id");

                    b.Property<Guid>("VisitingLogId")
                        .HasColumnType("uuid")
                        .HasColumnName("fk_visiting_log_id");

                    b.HasKey("Id");

                    b.HasIndex("LessonId");

                    b.HasIndex("VisitingLogId", "LessonId")
                        .IsUnique();

                    b.ToTable("visiting_logs_lessons", (string)null);
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Attendance", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Lesson", "Lesson")
                        .WithMany("Attendances")
                        .HasForeignKey("LessonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.NsiStudent", "Student")
                        .WithMany("Attendances")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Teacher", "Teacher")
                        .WithMany("Attendances")
                        .HasForeignKey("TeacherId");

                    b.Navigation("Lesson");

                    b.Navigation("Student");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.AttendanceEvent", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Attendance", "Attendance")
                        .WithMany("Events")
                        .HasForeignKey("AttendanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.NsiHuman", "AuthorHuman")
                        .WithMany()
                        .HasForeignKey("AuthorHumanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Attendance");

                    b.Navigation("AuthorHuman");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.AuditoriumLesson", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Auditorium", "Auditorium")
                        .WithMany("Lessons")
                        .HasForeignKey("AuditoriumId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("lessons_fk_auditoriums_foreign");

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Lesson", "Lesson")
                        .WithMany("Auditoriums")
                        .HasForeignKey("LessonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("auditoriums_fk_lessons_foreign");

                    b.Navigation("Auditorium");

                    b.Navigation("Lesson");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.DisciplineVisitingLog", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Discipline", "FkDisciplineVisitingLog")
                        .WithMany("VisitingLogs")
                        .HasForeignKey("FkDisciplineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("disciplines_fk_visiting_logs_foreign");

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.VisitingLog", "FkVisitingLogDiscipline")
                        .WithMany("Disciplines")
                        .HasForeignKey("FkVisitingLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("visiting_logs_fk_discilplines_logs_foreign");

                    b.Navigation("FkDisciplineVisitingLog");

                    b.Navigation("FkVisitingLogDiscipline");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Lesson", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Discipline", "Discipline")
                        .WithMany("Lessons")
                        .HasForeignKey("DisciplineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.LessonType", "LessonType")
                        .WithMany("Lessons")
                        .HasForeignKey("LessonTypeId");

                    b.Navigation("Discipline");

                    b.Navigation("LessonType");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.NsiHumanLoginId", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.NsiHuman", "Human")
                        .WithMany("Logins")
                        .HasForeignKey("HumanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("nsi_login_id_fk_nsi_human_foreign");

                    b.Navigation("Human");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.NsiStudent", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.NsiHuman", "NsiHuman")
                        .WithMany("NsiStudents")
                        .HasForeignKey("NsiHumanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NsiHuman");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.StudentMembership", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.NsiStudent", "NsiStudent")
                        .WithMany("StudentMemberships")
                        .HasForeignKey("NsiStudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.VisitingLog", "VisitingLog")
                        .WithMany("StudentMemberships")
                        .HasForeignKey("VisitingLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NsiStudent");

                    b.Navigation("VisitingLog");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Teacher", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.NsiHuman", "NsiHuman")
                        .WithMany("Teachers")
                        .HasForeignKey("NsiHumanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NsiHuman");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.TeacherLesson", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Lesson", "Lesson")
                        .WithMany("Teachers")
                        .HasForeignKey("LessonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("lessons_fk_teachers_foreign");

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Teacher", "Teacher")
                        .WithMany("Lessons")
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("teachers_fk_lessons_foreign");

                    b.Navigation("Lesson");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.TimeSlot", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Semester", "Semester")
                        .WithMany("TimeSlots")
                        .HasForeignKey("SemesterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Semester");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.VisitingLog", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Semester", "Semester")
                        .WithMany("VisitingLogs")
                        .HasForeignKey("SemesterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Semester");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.VisitingLogLesson", b =>
                {
                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.Lesson", "Lesson")
                        .WithMany("VisitingLogs")
                        .HasForeignKey("LessonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("visiting_logs_fk_lessons_foreign");

                    b.HasOne("RTUAttendAPI.AttendDatabase.Models.VisitingLog", "VisitingLog")
                        .WithMany("Lessons")
                        .HasForeignKey("VisitingLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("lessons_fk_visiting_logs_foreign");

                    b.Navigation("Lesson");

                    b.Navigation("VisitingLog");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Attendance", b =>
                {
                    b.Navigation("Events");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Auditorium", b =>
                {
                    b.Navigation("Lessons");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Discipline", b =>
                {
                    b.Navigation("Lessons");

                    b.Navigation("VisitingLogs");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Lesson", b =>
                {
                    b.Navigation("Attendances");

                    b.Navigation("Auditoriums");

                    b.Navigation("Teachers");

                    b.Navigation("VisitingLogs");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.LessonType", b =>
                {
                    b.Navigation("Lessons");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.NsiHuman", b =>
                {
                    b.Navigation("Logins");

                    b.Navigation("NsiStudents");

                    b.Navigation("Teachers");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.NsiStudent", b =>
                {
                    b.Navigation("Attendances");

                    b.Navigation("StudentMemberships");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Semester", b =>
                {
                    b.Navigation("TimeSlots");

                    b.Navigation("VisitingLogs");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.Teacher", b =>
                {
                    b.Navigation("Attendances");

                    b.Navigation("Lessons");
                });

            modelBuilder.Entity("RTUAttendAPI.AttendDatabase.Models.VisitingLog", b =>
                {
                    b.Navigation("Disciplines");

                    b.Navigation("Lessons");

                    b.Navigation("StudentMemberships");
                });
#pragma warning restore 612, 618
        }
    }
}
