using System;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs
{
    public class ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest : IQuery, IRequestLogWriterWithArgs
    {
        public ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest(Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        public Guid ApprenticeshipIncentiveId { get; }

        [Newtonsoft.Json.JsonIgnore]
        public RequestLogWithArgs Log
        {
            get
            {
                var message = "Checking whether apprenticeship incentive has possible change of circs {apprenticeshipIncentiveId}";
                return new RequestLogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>(message, new object[] { ApprenticeshipIncentiveId }),
                    OnError = () => new Tuple<string, object[]>(message, new object[] { ApprenticeshipIncentiveId })
                };
            }
        }
    }
}
