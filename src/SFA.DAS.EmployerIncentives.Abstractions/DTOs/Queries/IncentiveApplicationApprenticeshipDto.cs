namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class IncentiveApplicationApprenticeshipDto
    {
        public int ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double TotalIncentiveAmount { get; set; }
    }
}
