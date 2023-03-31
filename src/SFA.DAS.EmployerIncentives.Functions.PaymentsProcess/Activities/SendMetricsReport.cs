﻿using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class SendMetricsReport
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public SendMetricsReport(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [Function(nameof(SendMetricsReport))]
        public async Task Complete([ActivityTrigger] SendMetricsReportInput input)
        {
            await _commandDispatcher.Send(new SendMetricsReportCommand(new Domain.ValueObjects.CollectionPeriod(input.CollectionPeriod.Period, input.CollectionPeriod.Year)));
        }
    }
}
