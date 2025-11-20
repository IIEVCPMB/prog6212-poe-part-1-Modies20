using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClaimController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Claim
        public async Task<IActionResult> Index()
        {
            try
            {
                // For demo - in real app, you'd filter by logged-in user
                var claims = await _context.Claims
                    .Include(c => c.Documents)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();
                return View(claims);
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error loading claims: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading claims. Please try again.";
                return View(new List<Claim>());
            }
        }

        // GET: Claim/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Claim/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set default values
                    claim.SubmissionDate = DateTime.Now;
                    claim.LastUpdated = DateTime.Now;
                    claim.LecturerId = "LEC001"; // In real app, get from logged in user
                    claim.LecturerName = "Dr. Sarah Johnson"; // In real app, get from logged in user

                    _context.Add(claim);
                    await _context.SaveChangesAsync();

                    // Handle file uploads
                    if (files != null && files.Count > 0)
                    {
                        await UploadDocuments(claim.ClaimId, files);
                    }

                    TempData["SuccessMessage"] = "Claim submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the error
                    Debug.WriteLine($"Error creating claim: {ex.Message}");
                    ModelState.AddModelError("", $"Error submitting claim: {ex.Message}");
                }
            }
            return View(claim);
        }

        private async Task UploadDocuments(int claimId, List<IFormFile> files)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Validate file size (5MB limit)
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            TempData["WarningMessage"] = $"File {file.FileName} exceeds 5MB size limit and was not uploaded.";
                            continue;
                        }

                        // Validate file extension
                        var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".jpg", ".jpeg", ".png" };
                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            TempData["WarningMessage"] = $"File {file.FileName} has an invalid file type. Allowed types: PDF, DOC, DOCX, JPG, PNG.";
                            continue;
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var document = new SupportingDocument
                        {
                            ClaimId = claimId,
                            FileName = file.FileName,
                            FilePath = uniqueFileName,
                            FileSize = file.Length,
                            UploadDate = DateTime.Now
                        };

                        _context.SupportingDocuments.Add(document);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error uploading documents: {ex.Message}");
                TempData["WarningMessage"] = "Some files could not be uploaded. Please try again.";
            }
        }

        // GET: Claim/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Claim ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var claim = await _context.Claims
                    .Include(c => c.Documents)
                    .FirstOrDefaultAsync(m => m.ClaimId == id);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(claim);
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error loading claim details: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading claim details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // NEW METHODS FOR STATUS TRACKING AND APPROVAL

        // GET: Claim/PendingClaims - For Coordinators and Managers
        public async Task<IActionResult> PendingClaims()
        {
            try
            {
                var pendingClaims = await _context.Claims
                    .Where(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.ApprovedByCoordinator)
                    .Include(c => c.Documents)
                    .OrderBy(c => c.SubmissionDate)
                    .ToListAsync();

                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                // Log error
                Debug.WriteLine($"Error loading pending claims: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading pending claims.";
                return View(new List<Claim>());
            }
        }

        // POST: Claim/ApproveByCoordinator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveByCoordinator(int id)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(PendingClaims));
                }

                if (claim.Status != ClaimStatus.Submitted)
                {
                    TempData["ErrorMessage"] = "Claim is not in the correct status for coordinator approval.";
                    return RedirectToAction(nameof(PendingClaims));
                }

                claim.Status = ClaimStatus.ApprovedByCoordinator;
                claim.CoordinatorApprovedBy = "Programme Coordinator"; // In real app, get from logged in user
                claim.CoordinatorApprovalDate = DateTime.Now;
                claim.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim approved by coordinator successfully!";
                return RedirectToAction(nameof(PendingClaims));
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error approving claim by coordinator: {ex.Message}");
                TempData["ErrorMessage"] = $"Error approving claim: {ex.Message}";
                return RedirectToAction(nameof(PendingClaims));
            }
        }

        // POST: Claim/ApproveByManager
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveByManager(int id)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(PendingClaims));
                }

                if (claim.Status != ClaimStatus.ApprovedByCoordinator)
                {
                    TempData["ErrorMessage"] = "Claim must be approved by coordinator before manager approval.";
                    return RedirectToAction(nameof(PendingClaims));
                }

                claim.Status = ClaimStatus.ApprovedByManager;
                claim.ManagerApprovedBy = "Academic Manager"; // In real app, get from logged in user
                claim.ManagerApprovalDate = DateTime.Now;
                claim.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim approved by manager successfully!";
                return RedirectToAction(nameof(PendingClaims));
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error approving claim by manager: {ex.Message}");
                TempData["ErrorMessage"] = $"Error approving claim: {ex.Message}";
                return RedirectToAction(nameof(PendingClaims));
            }
        }

        // POST: Claim/RejectClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id, string rejectionReason)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(PendingClaims));
                }

                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    TempData["ErrorMessage"] = "Rejection reason is required.";
                    return RedirectToAction(nameof(PendingClaims));
                }

                if (claim.Status != ClaimStatus.Submitted && claim.Status != ClaimStatus.ApprovedByCoordinator)
                {
                    TempData["ErrorMessage"] = "Claim cannot be rejected in its current status.";
                    return RedirectToAction(nameof(PendingClaims));
                }

                claim.Status = ClaimStatus.Rejected;
                claim.RejectionReason = rejectionReason.Trim();
                claim.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim rejected successfully!";
                return RedirectToAction(nameof(PendingClaims));
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error rejecting claim: {ex.Message}");
                TempData["ErrorMessage"] = $"Error rejecting claim: {ex.Message}";
                return RedirectToAction(nameof(PendingClaims));
            }
        }

        // GET: Claim/StatusTracking - For testing status tracking view
        public async Task<IActionResult> StatusTracking(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Claim ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var claim = await _context.Claims
                    .Include(c => c.Documents)
                    .FirstOrDefaultAsync(m => m.ClaimId == id);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(claim);
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error loading claim for status tracking: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading claim status. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}