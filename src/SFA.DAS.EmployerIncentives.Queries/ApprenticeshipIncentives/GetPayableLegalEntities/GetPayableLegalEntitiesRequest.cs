using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities
{
    public class GetPayableLegalEntitiesRequest : IQuery
    {
        public short PaymentYear { get; }
        public byte PeriodNumber { get; }

        public GetPayableLegalEntitiesRequest(short collectionPeriodYear, byte collectionPeriodNumber)
        {
            PaymentYear = collectionPeriodYear;
            PeriodNumber = collectionPeriodNumber;
        }
    }
}
