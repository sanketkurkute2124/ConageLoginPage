using LoginRegistration.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginRegistration.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Education> Educations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer Configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.LastName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.HasIndex(e => e.Email)
                      .IsUnique();

                entity.Property(e => e.PhoneNumber)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.DateOfBirth)
                      .IsRequired();

                entity.Property(e => e.Password)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                // One Customer -> Many Education Records
                entity.HasMany(c => c.Educations)
                      .WithOne(e => e.Customer)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Education Configuration
            modelBuilder.Entity<Education>(entity =>
            {
                entity.ToTable("Educations");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Qualification)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.CollegeName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.University)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.PassingYear)
                      .IsRequired();

                entity.Property(e => e.Percentage)
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.CertificatePath)
                      .HasMaxLength(500);
            });
        }
    }
}   