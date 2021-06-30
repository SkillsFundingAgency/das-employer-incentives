using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CompletePaymentProcess
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<CompletePaymentProcess> _logger;

        public CompletePaymentProcess(
            ICommandDispatcher commandDispatcher, 
            ILogger<CompletePaymentProcess> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(CompletePaymentProcess))]
        public async Task Complete([ActivityTrigger] CompletePaymentProcessInput input)
        {
            _logger.LogInformation("Completing payment process");
            await _commandDispatcher.Send(new CompleteCommand(input.CompletionDateTime, new Domain.ValueObjects.CollectionPeriod(input.CollectionPeriod.Period, input.CollectionPeriod.Year)));
            _logger.LogInformation("Payment process completed for collection period {collectionPeriod}", input.CollectionPeriod);
        }
    }
}
