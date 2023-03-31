using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CompletePaymentProcess
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public CompletePaymentProcess(
            ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [Function(nameof(CompletePaymentProcess))]
        public async Task Complete([ActivityTrigger] CompletePaymentProcessInput input)
        {
            await _commandDispatcher.Send(new CompleteCommand(input.CompletionDateTime, new Domain.ValueObjects.CollectionPeriod(input.CollectionPeriod.Period, input.CollectionPeriod.Year)));
        }
    }
}
