using Microsoft.EntityFrameworkCore;
using Xunit;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Tests
{
    public class ClaimServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateClaim_ValidData_ShouldCreateClaim()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var claim = new Claim
            {
                LecturerId = "TEST001",
                LecturerName = "Test Lecturer",
                MonthYear = new DateTime(2025, 3, 1),
                HoursWorked = 40,
                HourlyRate = 500,
                Notes = "Test claim"
            };

            // Act
            context.Claims.Add(claim);
            await context.SaveChangesAsync();

            // Assert
            var savedClaim = await context.Claims.FirstOrDefaultAsync();
            Assert.NotNull(savedClaim);
            Assert.Equal("TEST001", savedClaim.LecturerId);
            Assert.Equal(20000, savedClaim.TotalAmount); // 40 * 500
            Assert.Equal(ClaimStatus.Submitted, savedClaim.Status);
        }

        [Fact]
        public void CalculateTotalAmount_ValidHoursAndRate_ShouldReturnCorrectTotal()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 35,
                HourlyRate = 550
            };

            // Act
            var total = claim.TotalAmount;

            // Assert
            Assert.Equal(19250, total); // 35 * 550
        }

        [Fact]
        public void ProgressPercentage_SubmittedStatus_ShouldReturn25()
        {
            // Arrange
            var claim = new Claim { Status = ClaimStatus.Submitted };

            // Act
            var progress = claim.ProgressPercentage;

            // Assert
            Assert.Equal(25, progress);
        }

        [Fact]
        public void ProgressPercentage_ApprovedByManager_ShouldReturn75()
        {
            // Arrange
            var claim = new Claim { Status = ClaimStatus.ApprovedByManager };

            // Act
            var progress = claim.ProgressPercentage;

            // Assert
            Assert.Equal(75, progress);
        }
    }
}
