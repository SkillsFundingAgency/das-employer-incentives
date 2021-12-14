using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        public ValidatePendingPayment(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [FunctionName(nameof(ValidatePendingPayment))]
        public async Task Validate([ActivityTrigger] ValidatePendingPaymentData payment)
        {
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
