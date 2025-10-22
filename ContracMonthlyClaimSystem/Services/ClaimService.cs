using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;

namespace ContractMonthlyClaimSystem.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Claim>> GetAllClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetClaimsByLecturerAsync(string lecturerId)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.LecturerId == lecturerId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetPendingClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.Status == ClaimStatus.Submitted ||
                           c.Status == ClaimStatus.ApprovedByCoordinator)
                .OrderBy(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<Claim> GetClaimByIdAsync(int id)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimId == id);
        }

        public async Task<Claim> SubmitClaimAsync(Claim claim)
        {
            claim.SubmissionDate = DateTime.Now;
            claim.LastUpdated = DateTime.Now;

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return claim;
        }

        public async Task<Claim> ApproveByCoordinatorAsync(int claimId, string approvedBy)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) return null;

            claim.Status = ClaimStatus.ApprovedByCoordinator;
            claim.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<Claim> ApproveByManagerAsync(int claimId, string approvedBy)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) return null;

            claim.Status = ClaimStatus.ApprovedByManager;
            claim.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<Claim> RejectClaimAsync(int claimId, string rejectedBy, string reason)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) return null;

            claim.Status = ClaimStatus.Rejected;
            claim.RejectionReason = reason;
            claim.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<bool> DeleteClaimAsync(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return false;

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}