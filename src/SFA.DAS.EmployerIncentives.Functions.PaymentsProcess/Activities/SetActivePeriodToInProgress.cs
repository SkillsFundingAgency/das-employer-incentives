﻿using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.CollectionCalendar.SetActivePeriodToInProgress;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SetActivePeriodToInProgress
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public SetActivePeriodToInProgress(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName(nameof(SetActivePeriodToInProgress))]
        public async Task Update([ActivityTrigger] object input)
        {
            await _commandDispatcher.Send(new SetActivePeriodToInProgressCommand());
        }
    }
}
