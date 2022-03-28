using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralFinancePaymentsServiceWithLogging : IBusinessCentralFinancePaymentsService
    {
        private readonly IBusinessCentralFinancePaymentsService _businessCentralFinancePaymentsService;
        private readonly ILogger<BusinessCentralFinancePaymentsServiceWithLogging> _logger;
        private readonly bool _obfuscateSensitiveData;

        public BusinessCentralFinancePaymentsServiceWithLogging(
            IBusinessCentralFinancePaymentsService businessCentralFinancePaymentsService,
            ILogger<BusinessCentralFinancePaymentsServiceWithLogging> logger,
            bool obfuscateSensitiveData)
        {
            _businessCentralFinancePaymentsService = businessCentralFinancePaymentsService;
            _logger = logger;
            _obfuscateSensitiveData = obfuscateSensitiveData;
        }

        public int PaymentRequestsLimit => _businessCentralFinancePaymentsService.PaymentRequestsLimit;

        public async Task SendPaymentRequests(IList<PaymentDto> payments)
        {
            var paymentRequests = payments.Select(x => x.Map(_obfuscateSensitiveData)).ToList();

            LogNonSensitiveRequestData(paymentRequests);

            await _businessCentralFinancePaymentsService.SendPaymentRequests(payments);
        }

        private void LogNonSensitiveRequestData(IEnumerable<BusinessCentralFinancePaymentRequest> paymentRequests)
        {
            var nonSensitiveData = BusinessCentralPaymentsRequestLogEntry.Create(paymentRequests);
            _logger.Log(LogLevel.Information,
                "[BusinessCentralFinancePaymentsService] Sending {count} payment requests to BC {@data}",
                nonSensitiveData.Count,
                JsonConvert.SerializeObject(nonSensitiveData));
        }

    }
}
