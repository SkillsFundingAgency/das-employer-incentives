﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetActiveCollectionPeriod;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class GetActiveCollectionPeriod
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger<GetActiveCollectionPeriod> _logger;

        public GetActiveCollectionPeriod(IQueryDispatcher queryDispatcher, ILogger<GetActiveCollectionPeriod> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(GetActiveCollectionPeriod))]
        public async Task<CollectionPeriodDto> Get([ActivityTrigger] object input)
        {
            _logger.LogInformation("Getting active collection period");
            var activePeriod = (await _queryDispatcher.Send<GetActiveCollectionPeriodRequest, GetActiveCollectionPeriodResponse>(new GetActiveCollectionPeriodRequest())).CollectionPeriod;
            _logger.LogInformation($"Active collection period number : {activePeriod.CollectionPeriodNumber}, CollectionYear : {activePeriod.CollectionYear}");
            return activePeriod;
        }
    }
}
