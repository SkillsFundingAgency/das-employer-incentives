namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class PeriodDto
    {
        public byte Period { get; set; }
        public long ApprenticeshipId { get; set; }
        public bool IsPayable { get; set; }
    }
}
