using System;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs
{
    public class ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest : IQuery
    {
        public ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest(Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        public Guid ApprenticeshipIncentiveId { get; }
    }
}
