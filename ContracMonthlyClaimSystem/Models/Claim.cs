using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public enum ClaimStatus
    {
        Submitted,
        ApprovedByCoordinator,
        ApprovedByManager,
        Rejected,
        Paid
    }

    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        public string LecturerId { get; set; }

        [Required]
        [Display(Name = "Lecturer Name")]
        public string LecturerName { get; set; }

        [Required]
        [Display(Name = "Month and Year")]
        [DataType(DataType.Date)]
        public DateTime MonthYear { get; set; }

        [Required]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(100, 1000, ErrorMessage = "Hourly rate must be between 100 and 1000")]
        [Display(Name = "Hourly Rate (ZAR)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        [StringLength(500)]
        public string Notes { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;

        [Display(Name = "Submission Date")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; }

        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }

        // Navigation property for documents
        public virtual ICollection<SupportingDocument> Documents { get; set; } = new List<SupportingDocument>();

        // NEW PROPERTIES FOR STATUS TRACKING
        [Display(Name = "Coordinator Approval Date")]
        public DateTime? CoordinatorApprovalDate { get; set; }

        [Display(Name = "Coordinator Approved By")]
        public string CoordinatorApprovedBy { get; set; }

        [Display(Name = "Manager Approval Date")]
        public DateTime? ManagerApprovalDate { get; set; }

        [Display(Name = "Manager Approved By")]
        public string ManagerApprovedBy { get; set; }

        // NEW COMPUTED PROPERTIES FOR STATUS TRACKING UI
        [NotMapped]
        [Display(Name = "Progress Percentage")]
        public int ProgressPercentage
        {
            get
            {
                return Status switch
                {
                    ClaimStatus.Submitted => 25,
                    ClaimStatus.ApprovedByCoordinator => 50,
                    ClaimStatus.ApprovedByManager => 75,
                    ClaimStatus.Paid => 100,
                    ClaimStatus.Rejected => 100,
                    _ => 0
                };
            }
        }

        [NotMapped]
        [Display(Name = "Progress Status")]
        public string ProgressStatus
        {
            get
            {
                return Status switch
                {
                    ClaimStatus.Submitted => "Submitted - Waiting for Coordinator Review",
                    ClaimStatus.ApprovedByCoordinator => "Approved by Coordinator - Waiting for Manager Review",
                    ClaimStatus.ApprovedByManager => "Approved by Manager - Ready for Payment",
                    ClaimStatus.Paid => "Payment Processed - Claim Settled",
                    ClaimStatus.Rejected => $"Rejected - {RejectionReason}",
                    _ => "Unknown Status"
                };
            }
        }
        [NotMapped]
        public bool IsWithinPolicy => HoursWorked <= 160 && HourlyRate <= 800;

        [NotMapped]
        public string PolicyValidationMessage
        {
            get
            {
                var messages = new List<string>();
                if (HoursWorked > 160)
                    messages.Add("Hours worked exceed maximum allowed (160 hours)");
                if (HourlyRate > 800)
                    messages.Add("Hourly rate exceeds maximum allowed (ZAR 800)");

                return messages.Any() ? string.Join("; ", messages) : "Within policy limits";
            }
        }

        [NotMapped]
        public string PolicyValidationColor => IsWithinPolicy ? "text-success" : "text-danger";
    }
}
