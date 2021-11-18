using System;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs
{
    public class ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest : IQuery, ILogWriter
    {
        public ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest(Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        public Guid ApprenticeshipIncentiveId { get; }
        public Log Log
        {
            get
            {
                var message = $"Apprenticeship Incentive Has Possible Change of Circumstances Query for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
