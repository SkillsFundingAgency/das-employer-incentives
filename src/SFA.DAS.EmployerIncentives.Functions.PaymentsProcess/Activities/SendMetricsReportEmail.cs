﻿using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class SendMetricsReportEmail
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public SendMetricsReportEmail(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName(nameof(SendMetricsReportEmail))]
        public Task Complete([ActivityTrigger] SendMetricsReportEmailInput input)
        {
            return _commandDispatcher.Send(new SendMetricsReportEmailCommand(new Domain.ValueObjects.CollectionPeriod(input.CollectionPeriod.Period, input.CollectionPeriod.Year), input.EmailAddress, input.ApprovalLink));
        }
    }
}