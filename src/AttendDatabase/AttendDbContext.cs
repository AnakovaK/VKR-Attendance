using System.Transactions;

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using RTUAttendAPI.AttendDatabase.Models;

namespace AttendDatabase;

public class AttendDbContext : DbContext, IDataProtectionKeyContext
{
    public AttendDbContext(DbContextOptions<AttendDbContext> options) : base(options) { }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<VisitingLog> VisitingLogs { get; set; }
    public DbSet<StudentMembership> StudentMemberships { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<VisitingLogLesson> VisitingLogLessons { get; set; }
    public DbSet<NsiHuman> NsiHumans { get; set; }
    public DbSet<NsiStudent> NsiStudents { get; set; }
    public DbSet<NsiHumanLoginId> NsiHumanLoginIds { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Discipline> Disciplines { get; set; }
    public DbSet<DisciplineVisitingLog> DisciplinesVisitingLogs { get; set; }
    public DbSet<LessonType> LessonTypes { get; set; }
    public DbSet<Auditorium> Auditoriums { get; set; }
    public DbSet<TeacherLesson> TeacherLessons { get; set; }
    public DbSet<AttendanceEvent> AttendanceEvents { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<LoginEvent> LoginEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<StudentMembershipType>();
        modelBuilder.HasPostgresEnum<StudentMembershipRole>();
        modelBuilder.HasPostgresEnum<AuthorType>();
        modelBuilder.HasPostgresEnum<AttendType>();
        modelBuilder.HasPostgresEnum<VisitingLogSource>();

        modelBuilder.Entity<Semester>(model =>
        {
            model.HasKey(s => s.Id).HasName("semester_pkey");
            model.ToTable("semester");
            model.Property(s => s.Id)
                .HasColumnName("id");
            model.Property(s => s.Title).HasColumnName("title")
                .HasMaxLength(255);
            model.Property(s => s.Start).HasColumnName("start");
            model.Property(s => s.End).HasColumnName("end");

            model.HasIndex(s => s.Title).IsUnique();
        });
        modelBuilder.Entity<VisitingLog>(model =>
        {
            model.HasKey(s => s.Id).HasName("visiting_log_pkey");
            model.ToTable("visiting_log");
            model.Property(s => s.Id)
                .HasColumnName("id");
            model.Property(s => s.Title).HasColumnName("title")
                .HasMaxLength(255);
            model.Property(s => s.IsArchived).HasColumnName("is_archived");
            model.Property(s => s.SemesterId).HasColumnName("semester_id");
            model.Property(vl => vl.Source).HasColumnName("source");

            model.HasOne(vl => vl.Semester)
                .WithMany(s => s.VisitingLogs)
                .HasForeignKey(vl => vl.SemesterId);
        });
        modelBuilder.Entity<StudentMembership>(model =>
        {
            model.HasKey(s => s.Id).HasName("student_membership_pkey");

            model.HasIndex(s => new { s.NsiStudentId, s.VisitingLogId })
                .HasDatabaseName("student_membership_student_in_group_once")
                .IsUnique();

            model.ToTable("student_membership");
            model.Property(s => s.Id)
                .HasColumnName("id");
            model.Property(s => s.NsiStudentId).HasColumnName("nsi_student_id");
            model.Property(s => s.VisitingLogId).HasColumnName("visiting_log_id");
            model.Property(s => s.MembershipType).HasColumnName("membership_type");
            model.Property(s => s.MembershipRole).HasColumnName("membership_role");

            model.HasOne(vl => vl.VisitingLog)
                .WithMany(s => s.StudentMemberships)
                .HasForeignKey(vl => vl.VisitingLogId);
        });
        modelBuilder.Entity<Discipline>(model =>
        {
            model.ToTable("discipline");
            model.Property(d => d.Id).HasColumnName("id");
            model.Property(d => d.Title).HasColumnName("title");
            model.HasIndex(d => d.Title).IsUnique();
        });
        modelBuilder.Entity<DisciplineVisitingLog>(model =>
        {
            model.ToTable("disciplines_visiting_logs");
            model.Property(dvl => dvl.Id).HasColumnName("id");
            model.Property(dvl => dvl.FkDisciplineId).HasColumnName("fk_discipline_id");
            model.Property(dvl => dvl.FkVisitingLogId).HasColumnName("fk_visiting_log_id");
            model.HasOne(dvl => dvl.FkDisciplineVisitingLog).WithMany(vl => vl.VisitingLogs)
                .HasForeignKey(dvl => dvl.FkDisciplineId)
                .HasConstraintName("disciplines_fk_visiting_logs_foreign");
            model.HasOne(dvl => dvl.FkVisitingLogDiscipline).WithMany(d => d.Disciplines)
                .HasForeignKey(dvl => dvl.FkVisitingLogId)
                .HasConstraintName("visiting_logs_fk_discilplines_logs_foreign");
        });
        modelBuilder.Entity<NsiHuman>(model =>
        {
            model.ToTable("nsi_human");
            model.Property(nh =>  nh.Id).HasColumnName("id");
            model.Property(nh => nh.Lastname).HasColumnName("lastname");
            model.Property(nh => nh.Firstname).HasColumnName("firstname");
            model.Property(nh => nh.Middlename).HasColumnName("middlename");
        });
        modelBuilder.Entity<NsiStudent>(model =>
        {
            model.ToTable("nsi_student");
            model.Property(ns => ns.Id).HasColumnName("id");
            model.Property(ns => ns.PersonalNumber).HasColumnName("personal_number");
            model.Property(ns => ns.NsiHumanId).HasColumnName("fk_nsi_human_id");
            model.HasIndex(ns => ns.PersonalNumber).IsUnique();
        });
        modelBuilder.Entity<Teacher>(model =>
        {
            model.ToTable("teacher");
            model.Property(t => t.Id).HasColumnName("id");
            model.Property(t => t.NsiHumanId).HasColumnName("fk_nsi_human_id");
        });
        modelBuilder.Entity<Lesson>(model =>
        {
            model.ToTable("lesson");
            model.Property(l => l.Id).HasColumnName("id");
            model.Property(l => l.DisciplineId).HasColumnName("fk_discipline_id");
            model.Property(l => l.LessonTypeId).HasColumnName("fk_lesson_type_id");
            model.Property(l => l.IsValidated).HasColumnName("is_validated");
            model.Property(l => l.Start).HasColumnName("start");
            model.Property(l => l.End).HasColumnName("end");
            model.Property(l => l.CreatedFromScheduleEventId).HasColumnName("created_from_tandem_event_id");
        });
        modelBuilder.Entity<Attendance>(model =>
        {
            model.ToTable("attendance");
            model.Property(a => a.Id).HasColumnName("id");
            model.Property(a => a.StudentId).HasColumnName("fk_student_id");
            model.Property(a => a.TeacherId).HasColumnName("fk_teacher_id");
            model.Property(a => a.LessonId).HasColumnName("fk_lesson_id");
            model.Property(a => a.Author).HasColumnName("author_type");
            model.Property(a => a.Attend).HasColumnName("attend_type");
            model.HasIndex(a => new { a.LessonId, a.StudentId }).IsUnique();
        });
        modelBuilder.Entity<TimeSlot>(model =>
        {
            model.ToTable("time_slot");
            model.Property(t => t.Id).HasColumnName("id");
            model.Property(t => t.StartMinutesUTC).HasColumnName("start_minutes_utc");
            model.Property(t => t.EndMinutesUTC).HasColumnName("end_minutes_utc");
            model.Property(t => t.SemesterId).HasColumnName("fk_semester_id");
        });
        modelBuilder.Entity<LessonType>(model =>
        {
            model.ToTable("lesson_type");
            model.Property(l => l.Id).HasColumnName("id");
            model.Property(l => l.LessonTypeName).HasColumnName("lesson_type_name");
        });
        modelBuilder.Entity<TeacherLesson>(model =>
        {
            model.ToTable("teachers_lessons");
            model.Property(tl => tl.Id).HasColumnName("id");
            model.HasIndex(tl => new { tl.TeacherId, tl.LessonId }).IsUnique();

            model.Property(tl => tl.TeacherId).HasColumnName("fk_teacher_id");
            model.Property(tl => tl.LessonId).HasColumnName("fk_lesson_id");
            model.HasOne(tl => tl.Teacher)
                .WithMany(t => t.Lessons)
                .HasForeignKey(tl => tl.TeacherId)
                .HasConstraintName("teachers_fk_lessons_foreign");
            model.HasOne(tl => tl.Lesson)
                .WithMany(l => l.Teachers)
                .HasForeignKey(tl => tl.LessonId)
                .HasConstraintName("lessons_fk_teachers_foreign");
        });
        modelBuilder.Entity<AuditoriumLesson>(model =>
        {
            model.ToTable("auditoriums_lessons");
            model.Property(al => al.Id).HasColumnName("id");
            model.HasIndex(al => new { al.AuditoriumId, al.LessonId }).IsUnique();

            model.Property(al => al.AuditoriumId).HasColumnName("fk_auditorium_id");
            model.Property(al => al.LessonId).HasColumnName("fk_lesson_id");
            model.HasOne(al => al.Auditorium)
                .WithMany(a => a.Lessons)
                .HasForeignKey(al => al.AuditoriumId)
                .HasConstraintName("lessons_fk_auditoriums_foreign");
            model.HasOne(al => al.Lesson)
                .WithMany(l => l.Auditoriums)
                .HasForeignKey(al => al.LessonId)
                .HasConstraintName("auditoriums_fk_lessons_foreign");
        });
        modelBuilder.Entity<VisitingLogLesson>(model =>
        {
            model.ToTable("visiting_logs_lessons");
            model.Property(vll => vll.Id).HasColumnName("id");
            model.HasIndex(al => new { al.VisitingLogId, al.LessonId }).IsUnique();

            model.Property(vll => vll.VisitingLogId).HasColumnName("fk_visiting_log_id");
            model.Property(vll => vll.LessonId).HasColumnName("fk_lesson_id");
            model.HasOne(vll => vll.VisitingLog)
                .WithMany(vl => vl.Lessons)
                .HasForeignKey(vll => vll.VisitingLogId)
                .HasConstraintName("lessons_fk_visiting_logs_foreign");
            model.HasOne(vll => vll.Lesson)
                .WithMany(l => l.VisitingLogs)
                .HasForeignKey(vll => vll.LessonId)
                .HasConstraintName("visiting_logs_fk_lessons_foreign");
        });
        modelBuilder.Entity<NsiHumanLoginId>(model =>
        {
            model.ToTable("nsi_login_id");

            model.HasKey(li => li.LoginId);
            model.Property(li => li.LoginId).HasColumnName("login_id");

            model.Property(li => li.HumanId).HasColumnName("human_id");

            model.HasOne(li => li.Human)
                .WithMany(h => h.Logins)
                .HasForeignKey(li => li.HumanId)
                .HasConstraintName("nsi_login_id_fk_nsi_human_foreign");
        });
        modelBuilder.Entity<Auditorium>(model =>
        {
            model.ToTable("Auditorium"); //TODO: Make good model entity description
        });
        modelBuilder.Entity<AttendanceEvent>(model =>
        {
            model.ToTable("attendance_events");
            model.Property(ae => ae.Id).HasColumnName("id");
            model.Property(ae => ae.AttendanceId).HasColumnName("attendance_id");
            model.Property(ae => ae.AuthorHumanId).HasColumnName("author_human_id");
            model.Property(ae => ae.Date).HasColumnName("date");
            model.Property(ae => ae.AttendType).HasColumnName("attend_type");
            model.Property(ae => ae.Author).HasColumnName("author_type");
        });
        modelBuilder.Entity<LoginEvent>(model =>
        {
            model.ToTable("login_event");
            model.Property(ae => ae.Id).HasColumnName("id");
            model.Property(ae => ae.Date).HasColumnName("date");
            model.Property(ae => ae.LoginInfo).HasColumnName("login_info");
        });
    }
}
