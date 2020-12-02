using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Exceptions;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class ValidatePendingPayment
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<ValidatePendingPayment> _logger;

        public ValidatePendingPayment(ICommandDispatcher commandDispatcher, ILogger<ValidatePendingPayment> logger)
        {
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        [FunctionName(nameof(ValidatePendingPayment))]
        public async Task Validate([ActivityTrigger] ValidatePendingPaymentData payment)
        {
            _logger.LogInformation("Validating Pending Payment [PendingPaymentId={pendingPaymentId}], [collection period={year}/{period}], [ApprenticeshipIncentiveId={apprenticeshipIncentiveId}]",
               payment.PendingPaymentId, payment.Year, payment.Period, payment.ApprenticeshipIncentiveId);
            try
            {
                await _commandDispatcher.Send(new ValidatePendingPaymentCommand(payment.ApprenticeshipIncentiveId,
                    payment.PendingPaymentId, payment.Year, payment.Period));
            }
            catch (Exception ex)
            {
                throw new ValidatePendingPaymentException(payment.ApprenticeshipIncentiveId, payment.PendingPaymentId, ex);
            }
        }
    }
}
