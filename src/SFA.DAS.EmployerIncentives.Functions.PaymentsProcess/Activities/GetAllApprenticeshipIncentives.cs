using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class GetAllApprenticeshipIncentives
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private ILogger<GetAllApprenticeshipIncentives> _logger;

        public GetAllApprenticeshipIncentives(IQueryDispatcher queryDispatcher, ILogger<GetAllApprenticeshipIncentives> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName("GetAllApprenticeshipIncentives")]
        public async Task<List<ApprenticeshipIncentiveOutput>> Get([ActivityTrigger]object input)
        {
            _logger.LogInformation($"Getting all Apprenticeship Incentives");
            var response = await _queryDispatcher.Send<GetApprenticeshipIncentivesRequest, GetApprenticeshipIncentivesResponse>(new GetApprenticeshipIncentivesRequest());
            _logger.LogInformation($"{response.ApprenticeshipIncentives.Count} returned");
            return response.ApprenticeshipIncentives.Select(x=> new ApprenticeshipIncentiveOutput { Id = x.Id, ULN = x.ULN }).ToList();
        }
    }
}
