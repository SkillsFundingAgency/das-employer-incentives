using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class CreatePayment
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public CreatePayment(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName(nameof(CreatePayment))]
        public async Task Create([ActivityTrigger] CreatePaymentInput input)
        {
            await _commandDispatcher.Send(new CreatePaymentCommand(input.ApprenticeshipIncentiveId, input.PendingPaymentId, new Domain.ValueObjects.CollectionPeriod(input.CollectionPeriod.Period, input.CollectionPeriod.Year)));
        }
    }
}
