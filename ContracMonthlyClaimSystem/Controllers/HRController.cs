using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRController(ApplicationDbContext context)
        {
            _context = context;
        }

        // HR Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var model = new HRDashboardViewModel
            {
                TotalClaims = await _context.Claims.CountAsync(),
                PendingClaims = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.ApprovedByCoordinator),
                ApprovedClaims = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.ApprovedByManager),
                PaidClaims = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Paid),
                TotalAmount = await _context.Claims.Where(c => c.Status == ClaimStatus.ApprovedByManager).SumAsync(c => c.TotalAmount)
            };

            return View(model);
        }

        // Claims ready for payment
        public async Task<IActionResult> ClaimsForPayment()
        {
            var claims = await _context.Claims
                .Where(c => c.Status == ClaimStatus.ApprovedByManager)
                .OrderBy(c => c.MonthYear)
                .ToListAsync();

            return View(claims);
        }

        // Mark claim as paid
        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = ClaimStatus.Paid;
                claim.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Claim #{claim.ClaimId} marked as paid.";
            }
            return RedirectToAction(nameof(ClaimsForPayment));
        }

        // Generate reports
        public async Task<IActionResult> Reports()
        {
            var model = new ReportViewModel
            {
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(ReportViewModel model)
        {
            var claims = await _context.Claims
                .Where(c => c.SubmissionDate >= model.StartDate && c.SubmissionDate <= model.EndDate)
                .OrderBy(c => c.SubmissionDate)
                .ToListAsync();

            ViewBag.ReportData = claims;
            ViewBag.ReportPeriod = $"{model.StartDate:dd MMM yyyy} to {model.EndDate:dd MMM yyyy}";
            ViewBag.TotalAmount = claims.Sum(c => c.TotalAmount);
            ViewBag.ClaimCount = claims.Count;

            return View("ReportResults", claims);
        }

        // Lecturer management
        public IActionResult LecturerManagement()
        {
            // In a real application, this would come from a database
            var lecturers = new List<Lecturer>
            {
                new Lecturer { Id = "LEC001", Name = "Dr. Sarah Johnson", Email = "sarah.johnson@university.edu", Department = "Computer Science", ContractType = "Part-Time" },
                new Lecturer { Id = "LEC002", Name = "Prof. Michael Williams", Email = "michael.williams@university.edu", Department = "Information Technology", ContractType = "Full-Time" },
                new Lecturer { Id = "LEC003", Name = "Dr. Elizabeth Brown", Email = "elizabeth.brown@university.edu", Department = "Software Engineering", ContractType = "Part-Time" }
            };

            return View(lecturers);
        }
    }

    // View Models
    public class HRDashboardViewModel
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int PaidClaims { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; }
    }

    public class Lecturer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string ContractType { get; set; }
    }
}
