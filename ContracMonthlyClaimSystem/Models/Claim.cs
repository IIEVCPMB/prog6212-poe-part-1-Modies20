namespace ContracMonthlyClaimSystem.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }
        public int LecturerId { get; set; }
        public DateTime SubmissionDate { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrentStatus { get; set; } // Pending, Approved, Rejected, etc.
    }
}
