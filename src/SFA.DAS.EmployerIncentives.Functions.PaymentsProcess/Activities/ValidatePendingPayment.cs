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

        [FunctionName("ValidatePendingPayment")]
        public async Task Validate([ActivityTrigger] ValidatePendingPaymentData payment)
        {
            _logger.LogInformation($"Validating Pending Payment [PendingPaymentId={payment.PendingPaymentId}], [collection period={payment.Year}/{payment.Period}], [ApprenticeshipIncentiveId={payment.ApprenticeshipIncentiveId}]",
                new { payment });
            try
            {
                await _commandDispatcher.Send(new ValidatePendingPaymentCommand(payment.ApprenticeshipIncentiveId,
                        payment.PendingPaymentId, payment.Year, payment.Period));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Validating Pending Payment [PendingPaymentId={payment.PendingPaymentId}], [collection period={payment.Year}/{payment.Period}], [ApprenticeshipIncentiveId={payment.ApprenticeshipIncentiveId}]", new { payment });
                throw new ValidatePendingPaymentException(payment.ApprenticeshipIncentiveId, payment.PendingPaymentId, ex);
            }
        }
    }
}
