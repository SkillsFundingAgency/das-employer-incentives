using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;

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
        public async Task Complete([ActivityTrigger] SendMetricsReportEmailInput input)
        {
            try
            {
                await _commandDispatcher.Send(new SendMetricsReportEmailCommand(new Domain.ValueObjects.CollectionPeriod(input.CollectionPeriod.Period, input.CollectionPeriod.Year), input.EmailAddress));
            }
            catch (Exception ex)
            {
                var x = 0;
            }
        }
    }
}
