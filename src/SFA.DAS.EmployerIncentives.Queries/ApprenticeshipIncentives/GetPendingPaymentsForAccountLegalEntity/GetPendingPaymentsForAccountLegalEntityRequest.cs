using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity
{
    public class GetPendingPaymentsForAccountLegalEntityRequest : IQuery
    {
        public long AccountLegalEntityId { get; }
        public short CollectionPeriodYear { get; }
        public byte CollectionPeriodMonth { get; }

        public GetPendingPaymentsForAccountLegalEntityRequest(long accountLegalEntityId, short collectionPeriodYear, byte collectionPeriodMonth)
        {
            AccountLegalEntityId = accountLegalEntityId;
            CollectionPeriodYear = collectionPeriodYear;
            CollectionPeriodMonth = collectionPeriodMonth;
        }
    }
}
