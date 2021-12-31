namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class WithdrawApplicationRequest
    {
        public WithdrawalType WithdrawalType { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long ULN { get; set; }
        public ServiceRequest ServiceRequest { get; set; }
        public long AccountId { get; set; }
        public string EmailAddress { get; set; }
    }
}
