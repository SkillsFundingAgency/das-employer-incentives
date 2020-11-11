namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class PeriodDto
    {
        public long ApprenticeshipId { get; set; }
        public bool IsPayable { get; set; }
        public byte Period { get; set; }
    }
}
