namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives
{
    public class ClawbackLegalEntityDto
    {
        public bool IsSent { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long AccountId { get; set; }
    }
}
