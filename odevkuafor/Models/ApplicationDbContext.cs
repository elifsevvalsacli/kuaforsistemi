using Microsoft.EntityFrameworkCore;
using odevkuafor.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<User>? Users { get; set; }
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<EmployeeService> EmployeeServices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Appointment tablosu
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasOne(a => a.Service)
                  .WithMany()
                  .HasForeignKey(a => a.ServiceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Employee)
                  .WithMany()
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Service tablosu
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Price).IsRequired();
        });

        // Employee tablosu
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Specialization).IsRequired().HasMaxLength(100);
        });

        // EmployeeService Many-to-Many ilişki
        modelBuilder.Entity<EmployeeService>(entity =>
        {
            entity.HasKey(es => new { es.EmployeeId, es.ServiceId });

            entity.HasOne(es => es.Employee)
                  .WithMany(e => e.EmployeeServices)
                  .HasForeignKey(es => es.EmployeeId);

            entity.HasOne(es => es.Service)
                  .WithMany(s => s.EmployeeServices)
                  .HasForeignKey(es => es.ServiceId);
        });
    }

    // Admin kullanıcısını veritabanına ekleyecek Seed metodu
    public static void SeedAdminUser(ApplicationDbContext context)
    {
        var adminEmail = "b211210068@sakarya.edu.tr";
        var adminPassword = "sau";  // Şifreyi burada belirtiyoruz
        var adminUser = context.Users.FirstOrDefault(u => u.Email == adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                Email = adminEmail,
                Password = adminPassword,
                IsAdmin = true  // Admin olarak işaretliyoruz
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}
