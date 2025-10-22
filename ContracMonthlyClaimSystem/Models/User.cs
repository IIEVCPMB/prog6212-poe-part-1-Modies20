namespace ContractMonthlyClaimSystem.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
    }

    public enum UserRole
    {
        Lecturer,
        ProgrammeCoordinator,
        AcademicManager
    }
}

