using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Models;

namespace SmartCertificateSystem.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Employer> Employers => Set<Employer>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Transcript> Transcripts => Set<Transcript>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Student>(UserRoles.Student)
            .HasValue<Admin>(UserRoles.Admin)
            .HasValue<Employer>(UserRoles.Employer);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Certificate>()
            .HasIndex(c => c.CertificateId)
            .IsUnique();

        modelBuilder.Entity<Certificate>()
            .HasOne(c => c.Student)
            .WithMany(s => s.Certificates)
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Certificate>()
            .HasOne(c => c.Transcript)
            .WithMany()
            .HasForeignKey(c => c.TranscriptId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transcript>()
            .HasOne(t => t.Student)
            .WithMany(s => s.Transcripts)
            .HasForeignKey(t => t.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Transcript)
            .WithMany(t => t.Grades)
            .HasForeignKey(g => g.TranscriptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Module>()
            .HasOne(m => m.Course)
            .WithMany(c => c.Modules)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();
    }
}
