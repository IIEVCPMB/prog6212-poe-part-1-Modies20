using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ContractMonthlyClaimSystem.Controllers;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using System;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Tests
{
    public class ClaimControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task IndexReturnsViewResultWithListOfClaims()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new ClaimController(context, null);

            // Add test data
            context.Claims.Add(new Claim { LecturerId = "TEST001", HoursWorked = 10, HourlyRate = 100 });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Claim>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new ClaimController(context, null);
            controller.ModelState.AddModelError("HoursWorked", "Required");

            var claim = new Claim { LecturerName = "Test" };

            // Act
            var result = await controller.Create(claim, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(claim, viewResult.Model);
        }
    }
}