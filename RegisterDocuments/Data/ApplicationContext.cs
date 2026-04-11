using Microsoft.EntityFrameworkCore;
using RegisterDocuments.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegisterDocuments.Data
{
    public class ApplicationContext : DbContext
    {
        private static ApplicationContext context;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<DocumentFile> DocumentFiles { get; set; } = null!;
        public DbSet<FileType> FileTypes { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<EmployeeRole> EmployeeRoles { get; set; } = null!;
        public DbSet<DocumentChangeHistory> DocumentHistory { get; set; }
        public DbSet<RegistrationRequest> RegistrationRequests { get; set; }

        public static ApplicationContext GetContext()
        {
            if (context == null)
                context = new ApplicationContext();
            return context;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=DB_register_documents;Username=postgres;Password=12345Andrew");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasKey(e => e.EmployeeCode);
            modelBuilder.Entity<DocumentType>().HasKey(t => t.CodeTypeDocument);
            modelBuilder.Entity<Document>().HasKey(d => d.CodeDocument);
            modelBuilder.Entity<DocumentFile>().HasKey(f => f.CodeFile);
            modelBuilder.Entity<FileType>().HasKey(f => f.CodeTypeFile);
            modelBuilder.Entity<Status>().HasKey(s => s.CodeStatus);
            modelBuilder.Entity<EmployeeRole>()
                .HasKey(er => new { er.EmployeeCode, er.RoleCode });

            modelBuilder.Entity<Role>().HasKey(r => r.RoleCode);

            modelBuilder.Entity<Employee>().ToTable("Сотрудники");
            modelBuilder.Entity<DocumentType>().ToTable("Тип_документа");
            modelBuilder.Entity<Document>().ToTable("Документы");
            modelBuilder.Entity<DocumentFile>().ToTable("Файлы_документа");
            modelBuilder.Entity<FileType>().ToTable("Тип_файла");
            modelBuilder.Entity<Status>().ToTable("Статус");
            modelBuilder.Entity<Role>().ToTable("Роли");
            modelBuilder.Entity<EmployeeRole>().ToTable("Выдача_роли");
            modelBuilder.Entity<DocumentChangeHistory>().ToTable("История_изменений");
            modelBuilder.Entity<RegistrationRequest>().ToTable("Заявки_на_регистрацию");

            modelBuilder.Entity<Document>()
                .HasOne<DocumentType>()
                .WithMany(t => t.Documents)
                .HasForeignKey(d => d.CodeTypeDocument);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Status)
                .WithMany()
                .HasForeignKey(d => d.CodeStatus);

            modelBuilder.Entity<Document>()
                .HasMany(d => d.Files)
                .WithOne()
                .HasForeignKey(f => f.CodeDocument);

            modelBuilder.Entity<DocumentFile>()
                .HasOne(f => f.FileType)
                .WithMany()
                .HasForeignKey(f => f.CodeTypeFile);

            modelBuilder.Entity<EmployeeRole>()
                .HasOne(er => er.Employee)
                .WithMany(e => e.EmployeeRoles)
                .HasForeignKey(er => er.EmployeeCode);

            modelBuilder.Entity<EmployeeRole>()
                .HasOne(er => er.Role)
                .WithMany(r => r.EmployeeRoles)
                .HasForeignKey(er => er.RoleCode);

            modelBuilder.Entity<DocumentChangeHistory>()
                .HasKey(h => new { h.EmployeeCode, h.DocumentCode, h.ChangeDate });

            modelBuilder.Entity<DocumentChangeHistory>()
                .HasOne(h => h.Employee)
                .WithMany()
                .HasForeignKey(h => h.EmployeeCode)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<DocumentChangeHistory>()
                .HasOne(h => h.Document)
                .WithMany()
                .HasForeignKey(h => h.DocumentCode)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<RegistrationRequest>()
                .HasKey(r => r.CodeApplication);

        }
    }
}
