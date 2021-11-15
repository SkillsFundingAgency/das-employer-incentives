using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class ApprenticeshipIncentiveHasPossibleChangeOrCircs
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger<ApprenticeshipIncentiveHasPossibleChangeOrCircs> _logger;

        public ApprenticeshipIncentiveHasPossibleChangeOrCircs(IQueryDispatcher queryDispatcher, ILogger<ApprenticeshipIncentiveHasPossibleChangeOrCircs> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(ApprenticeshipIncentiveHasPossibleChangeOrCircs))]
        public async Task<bool> Get([ActivityTrigger] Guid apprenticeshipIncentiveId)
        {
            _logger.LogDebug("Checking whether apprenticeship incentive has possible change of circs {apprenticeshipIncentiveId}", apprenticeshipIncentiveId);
            var hasPossibleChangeOfCircs = (await _queryDispatcher.Send<ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest, ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse>(new ApprenticeshipIncentiveHasPossibleChangeOrCircsRequest(apprenticeshipIncentiveId))).HasPossibleChangeOfCircumstances;
            _logger.LogDebug("Apprenticeship incentive {apprenticeshipIncentiveId} has possible change of circs = {hasPossibleChangeOfCircs}", apprenticeshipIncentiveId, hasPossibleChangeOfCircs);
            return hasPossibleChangeOfCircs;
        }
    }
}
