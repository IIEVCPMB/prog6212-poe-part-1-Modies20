using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IDocumentService
    {
        Task<List<Document>> GetDocumentsByClaimAsync(int claimId);
        Task<Document> UploadDocumentAsync(int claimId, IFormFile file);
        Task<bool> DeleteDocumentAsync(int documentId);
        bool ValidateFile(IFormFile file);
    }
}
