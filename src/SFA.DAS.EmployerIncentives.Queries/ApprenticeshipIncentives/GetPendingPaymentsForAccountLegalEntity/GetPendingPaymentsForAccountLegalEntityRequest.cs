using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity
{
    public class GetPendingPaymentsForAccountLegalEntityRequest : IQuery
    {
        public long AccountLegalEntityId { get; }
        public short PaymentYear { get; }
        public byte PeriodNumber { get; }

        public GetPendingPaymentsForAccountLegalEntityRequest(long accountLegalEntityId, short paymentYear, byte periodNumber)
        {
            AccountLegalEntityId = accountLegalEntityId;
            PaymentYear = paymentYear;
            PeriodNumber = periodNumber;
        }
    }
}
