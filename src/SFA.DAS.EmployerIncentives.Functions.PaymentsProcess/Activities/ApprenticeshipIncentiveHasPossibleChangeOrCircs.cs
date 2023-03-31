using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class ApprenticeshipIncentiveHasPossibleChangeOrCircs
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public ApprenticeshipIncentiveHasPossibleChangeOrCircs(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [Function(nameof(ApprenticeshipIncentiveHasPossibleChangeOrCircs))]
        public async Task<bool> Get([ActivityTrigger] Guid apprenticeshipIncentiveId)
        {
            return (await _queryDispatcher.Send<ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest, ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse>(new ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest(apprenticeshipIncentiveId))).HasPossibleChangeOfCircumstances;
        }
    }
}
