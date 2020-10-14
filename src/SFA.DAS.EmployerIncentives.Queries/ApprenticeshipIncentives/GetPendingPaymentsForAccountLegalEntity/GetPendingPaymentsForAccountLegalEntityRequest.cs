using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity
{
    public class GetPendingPaymentsForAccountLegalEntityRequest : IQuery
    {
        public long AccountLegalEntityId { get; }
        public short CollectionPeriodYear { get; }
        public short CollectionPeriodMonth { get; }

        public GetPendingPaymentsForAccountLegalEntityRequest(long accountLegalEntityId, short collectionPeriodYear, short collectionPeriodMonth)
        {
            AccountLegalEntityId = accountLegalEntityId;
            CollectionPeriodYear = collectionPeriodYear;
            CollectionPeriodMonth = collectionPeriodMonth;
        }
    }
}
