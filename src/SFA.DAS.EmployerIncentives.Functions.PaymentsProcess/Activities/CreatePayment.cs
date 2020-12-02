using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class CreatePayment
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<CreatePayment> _logger;

        public CreatePayment(ICommandDispatcher commandDispatcher, ILogger<CreatePayment> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(CreatePayment))]
        public async Task Create([ActivityTrigger] CreatePaymentInput input)
        {
            _logger.LogInformation("Creating Payment for apprenticeship incentive id {apprenticeshipIncentiveId}, pending payment id {pendingPaymentId}, collection period {collectionPeriod}", input.ApprenticeshipIncentiveId, input.PendingPaymentId, input.CollectionPeriod);
            await _commandDispatcher.Send(new CreatePaymentCommand(input.ApprenticeshipIncentiveId, input.PendingPaymentId, input.CollectionPeriod.Year, input.CollectionPeriod.Period));
            _logger.LogInformation("Created Payment for apprenticeship incentive id {apprenticeshipIncentiveId}, pending payment id {pendingPaymentId}, collection period {collectionPeriod}", input.ApprenticeshipIncentiveId, input.PendingPaymentId, input.CollectionPeriod);
        }
    }
}
