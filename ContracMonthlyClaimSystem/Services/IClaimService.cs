using System.Collections.Generic;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IClaimService
    {
        Task<List<Claim>> GetAllClaimsAsync();
        Task<List<Claim>> GetClaimsByLecturerAsync(string lecturerId);
        Task<List<Claim>> GetPendingClaimsAsync();
        Task<Claim> GetClaimByIdAsync(int id);
        Task<Claim> SubmitClaimAsync(Claim claim);
        Task<Claim> ApproveByCoordinatorAsync(int claimId, string approvedBy);
        Task<Claim> ApproveByManagerAsync(int claimId, string approvedBy);
        Task<Claim> RejectClaimAsync(int claimId, string rejectedBy, string reason);
        Task<bool> DeleteClaimAsync(int id);
    }
}