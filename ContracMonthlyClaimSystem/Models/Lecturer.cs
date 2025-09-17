namespace ContracMonthlyClaimSystem.Models
{
    public class Lecturer
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // Lecturer, ProgrammeCoordinator, AcademicManager
        public decimal HourlyRate { get; set; }
    }
}

