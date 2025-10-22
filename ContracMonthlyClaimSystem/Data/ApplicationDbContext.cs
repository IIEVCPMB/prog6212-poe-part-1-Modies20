using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace ContractMonthlyClaimSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between Claim and Document
            modelBuilder.Entity<Claim>()
                .HasMany(c => c.Documents)
                .WithOne(d => d.Claim)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Cascade); // If a claim is deleted, delete its documents too

            // Configure TotalAmount as a computed property (if supported by your database)
            modelBuilder.Entity<Claim>()
                .Property(c => c.TotalAmount)
                .HasComputedColumnSql("[HoursWorked] * [HourlyRate]");

            // Seed initial data - remove Documents from seed data for now
            modelBuilder.Entity<Claim>().HasData(
                new Claim
                {
                    ClaimId = 1,
                    LecturerId = "LEC001",
                    LecturerName = "Dr. Sarah Johnson",
                    MonthYear = new DateTime(2025, 2, 1),
                    HoursWorked = 40,
                    HourlyRate = 550,
                    Notes = "February teaching hours",
                    Status = ClaimStatus.Submitted,
                    SubmissionDate = DateTime.Now.AddDays(-2),
                    LastUpdated = DateTime.Now.AddDays(-2)
                },
                new Claim
                {
                    ClaimId = 2,
                    LecturerId = "LEC002",
                    LecturerName = "Prof. Michael Williams",
                    MonthYear = new DateTime(2025, 2, 1),
                    HoursWorked = 35,
                    HourlyRate = 600,
                    Notes = "Advanced programming course",
                    Status = ClaimStatus.ApprovedByCoordinator,
                    SubmissionDate = DateTime.Now.AddDays(-5),
                    LastUpdated = DateTime.Now.AddDays(-3)
                }
            );
        }
    }
}